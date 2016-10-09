using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using MAD = Microsoft.Azure.Devices;
using MADC = Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client.Exceptions;

namespace Simulator
{
    public class IotHubSink : ISnapshotSink
    {
        private static readonly ConcurrentDictionary<string, MAD.Device> _devices = new ConcurrentDictionary<string, MAD.Device>();
        private static readonly string _hubServiceConnectionString = null;
        private static readonly string _hubHostName = null;

        static IotHubSink()
        {
            _hubServiceConnectionString = ConfigurationManager.AppSettings["iotHubServiceConnection"];
            _hubHostName = ConfigurationManager.AppSettings["iotHubHostName"];
        }

        public static void RemoveAllDevices()
        {
            var registryMgr = MAD.RegistryManager.CreateFromConnectionString(_hubServiceConnectionString);

            foreach (var device in registryMgr.GetDevicesAsync(int.MaxValue).Result)
            {
                registryMgr.RemoveDeviceAsync(device).Wait();
            }
        }

        public void HandleSnapshot(BusInfo bus)
        {
            var device = GetDevice(bus);

            Debug.Assert(device.Id == bus.VehicleId.ToString());

            var authMethod = new MADC.DeviceAuthenticationWithRegistrySymmetricKey(device.Id, device.Authentication.SymmetricKey.PrimaryKey);

            using (var client = MADC.DeviceClient.Create(_hubHostName, authMethod))
            {
                var msg = new MADC.Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(bus)));

                client.OpenAsync().Wait();

                client.SendEventAsync(msg).Wait();
            }
        }

        private MAD.Device GetDevice(BusInfo bus)
        {
            return _devices.GetOrAdd(bus.VehicleId.ToString(), id =>
            {
                var registryMgr = MAD.RegistryManager.CreateFromConnectionString(_hubServiceConnectionString);

                try
                {
                    return registryMgr.AddDeviceAsync(new MAD.Device(id)).Result;
                }
                catch (Microsoft.Azure.Devices.Common.Exceptions.DeviceAlreadyExistsException)
                {
                    return registryMgr.GetDeviceAsync(id).Result;
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException is Microsoft.Azure.Devices.Common.Exceptions.DeviceAlreadyExistsException)
                    {
                        return registryMgr.GetDeviceAsync(id).Result;
                    }
                    else
                    {
                        throw;
                    }
                }
            });
        }
    }
}

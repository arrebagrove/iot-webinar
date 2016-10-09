
using Common;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simulator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //IotHubSink.RemoveAllDevices();
            //return;

            var diContainer = new UnityContainer();

            diContainer.LoadConfiguration();

            var source = diContainer.Resolve<ISnapshotSource>();

            while (true)
            {
                Console.WriteLine("Retrieving snapshots...");

                Parallel.ForEach(source.GetSnapshots(), snapshot =>
                {
                    var handler = diContainer.Resolve<ISnapshotSink>();

                    Console.WriteLine("Writing snapshot...");

                    handler.HandleSnapshot(snapshot);
                });

                Console.WriteLine("Sleeping...");

                Thread.Sleep(TimeSpan.FromSeconds(30));
            }
        }
    }
}

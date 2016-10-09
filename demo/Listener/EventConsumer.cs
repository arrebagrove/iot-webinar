using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Common;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Newtonsoft.Json;
using System.Configuration;
using System.Net.Http.Headers;

namespace Listener
{
    public class EventConsumer : IEventProcessor
    {
        public EventConsumer()
        {
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        public Task OpenAsync(PartitionContext context)
        {
            return Task.FromResult(0);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            // configure REST client
            var client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["webUrl"]);

            var save = false;

            // process incoming messages
            foreach (var msg in messages)
            {
                // get BusInfo from incoming message
                var json = Encoding.UTF8.GetString(msg.GetBytes());

                var bus = JsonConvert.DeserializeObject<BusInfo>(json);

                // create metadata object from busInfo to POST to web app
                var info = new
                {
                    id = bus.VehicleId,
                    lat = bus.Latitude,
                    lng = bus.Longitude,
                    timepoint = bus.Timepoint
                };

                var content = new StringContent(JsonConvert.SerializeObject(info));

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // POST
                await client.PostAsync("api/listener", content);

                save = true;
            }

            // if we successfully processed the messages, make note of that for all other consumers
            if (save)
            {
                await context.CheckpointAsync();
            }
        }
    }
}
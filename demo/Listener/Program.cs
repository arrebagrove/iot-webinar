using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Listener
{
    class Program
    {
        static void Main(string[] args)
        {
            var eventHubConnectionString = ConfigurationManager.AppSettings["ehConnection"];
            var storageConnectionString = ConfigurationManager.AppSettings["storageConnection"];

            var eventProcessorHost = new EventProcessorHost(Guid.NewGuid().ToString(), "messages/events", "location", eventHubConnectionString, storageConnectionString, "messages-events");

            var options = new EventProcessorOptions();
            options.ExceptionReceived += (sender, e) => { Console.WriteLine(e.Exception); };

            eventProcessorHost.RegisterEventProcessorAsync<EventConsumer>(options).Wait();

            Console.WriteLine("Press [Enter] to quit...");
            Console.ReadLine();

            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}

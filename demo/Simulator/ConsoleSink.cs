
using Common;
using Newtonsoft.Json;
using System;

namespace Simulator
{
    public class ConsoleSink : ISnapshotSink
    {
        public ConsoleSink()
        {
        }

        public void HandleSnapshot(BusInfo bus)
        {
            var json = JsonConvert.SerializeObject(bus, Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}

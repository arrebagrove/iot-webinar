
using Common;
using System;

namespace Simulator
{
    public interface ISnapshotSink
    {
        void HandleSnapshot(BusInfo bus);
    }
}

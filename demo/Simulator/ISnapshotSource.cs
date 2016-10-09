
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
    public interface ISnapshotSource
    {
        IEnumerable<BusInfo> GetSnapshots();
    }
}

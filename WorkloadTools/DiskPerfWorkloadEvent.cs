using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace WorkloadTools
{
    [Serializable]
    public class DiskPerfWorkloadEvent : WorkloadEvent
    {
        public DataTable DiskPerf;

        public DiskPerfWorkloadEvent()
        {
            Type = EventType.DiskPerf;
        }

    }
}

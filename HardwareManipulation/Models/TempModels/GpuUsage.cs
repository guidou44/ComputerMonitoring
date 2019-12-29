using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Models
{
    public class GpuUsage : HardwareUsageBase
    {
        public GpuUsage()
        {
            ShortName = "GPU";
        }
        public uint Id { get; set; }
        public double Temperature { get; set; }

        public override string ToString()
        {
            return Main_Value.ToString() + " %";
        }
    }
}

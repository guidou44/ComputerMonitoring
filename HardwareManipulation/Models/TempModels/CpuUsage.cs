using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Models
{
    public class CpuUsage : HardwdareInformation
    {
        public CpuUsage()
        {
            ShortName = "CPU";
        }
        public double Temperature { get; set; }
        public float Current_ClockSpeed { get; set; }
        public uint Number_of_cores { get; set; }
        public uint Thread_count { get; set; }

        public override string ToString()
        {
            return Main_Value.ToString() + " %";
        }
    }
}

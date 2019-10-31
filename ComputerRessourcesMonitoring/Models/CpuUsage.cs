using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerRessourcesMonitoring.Models
{
    public class CpuUsage
    {
        public string Cpu_Name { get; set; }
        public double Cpu_Usage { get; set; }
        public float Cpu_Current_ClockSpeed { get; set; }
        public uint Number_of_cores { get; set; }
        public uint Thread_count { get; set; }

    }
}

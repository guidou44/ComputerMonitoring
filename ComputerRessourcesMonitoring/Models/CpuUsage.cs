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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerRessourcesMonitoring.Models
{
    public enum MonitoringTarget
    { 
        CPU_Usage_PC,
        CPU_Usage,
        GPU_Usage,
        RAM_Usage,
        CPU_Temp,
        GPU_Temp,
        FAN_Speed,
        None
    }
}

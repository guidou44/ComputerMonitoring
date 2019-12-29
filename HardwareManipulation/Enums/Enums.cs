using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Enums
{
    public enum RessourceName
    {
        CPU_Usage,
        CPU_Temp,
        CPU_ClockSpeed,
        CPU_CoreCount,
        CPU_ThreadCount,
        GPU_Usage,
        GPU_Temperatur,
        RAM_Usage,
        FAN_Speed
    }

    public enum MonitoringTarget
    {
        CPU_Usage,
        GPU_Usage,
        RAM_Usage,
        CPU_Temp,
        GPU_Temp,
        FAN_Speed,
        None
    }
}

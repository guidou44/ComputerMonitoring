using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareAccess.Enums
{
    public enum MonitoringTarget
    {
        CPU_Core_Count,
        CPU_Clock_Speed,
        CPU_Make,
        CPU_Temp,
        CPU_Thread_Count,
        CPU_Load,
        FAN_Speed,
        Mother_Board_Make,
        GPU_Clock,
        GPU_Make,
        GPU_Memory_Controller,
        GPU_Memory_Clock,
        GPU_Memory_Load,
        GPU_Shader_Clock,
        GPU_Temp,
        GPU_Load,
        GPU_VideoEngine_Load,
        Primary_Network_HDD_Used_Space,
        Primary_HDD_Used_Space,
        RAM_Usage,
        Secondary_HDD_Used_Space,
        Secondary_Network_HDD_Used_Space,
        Server_CPU_Load,
        Server_CPU_Temp,
        Server_RAM_Usage,
        Server_CPU_ProcessUsage,
        None,
    }
}

using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Components
{
    public interface IOpenHardwareComputer
    {
        void Open();
        void Close();

        bool MainboardEnabled { get; set; }
        bool GPUEnabled { get; set; }

        IOpenHardware[] Hardware { get; }
    }
}

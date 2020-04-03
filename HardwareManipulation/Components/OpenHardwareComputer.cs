using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Components
{
    public class OpenHardwareComputer : Computer 
    {
        public new virtual void Close()
        {
            base.Close();
        }
        public new virtual void Open()
        {
            base.Open();
        }
    }
}

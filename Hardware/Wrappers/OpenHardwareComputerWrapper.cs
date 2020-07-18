using Hardware.Components;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Wrappers
{
    public class OpenHardwareComputerWrapper : Computer, IOpenHardwareComputer
    {        

        public new virtual void Close()
        {
            base.Close();
        }
        public new virtual void Open()
        {
            base.Open();
        }

        IOpenHardware[] IOpenHardwareComputer.Hardware
        {
            get
            {
                return From(base.Hardware);
            }
        }

        private IOpenHardware[] From(IHardware[] hardwares)
        {
            IOpenHardware[] output = new IOpenHardware[hardwares.Count()];
            for (int i = 0; i < hardwares.Count(); i++)
            {
                output[i] = new OpenHardwareWrapper(hardwares[i]);
            }
            return output;
        }
    }
}

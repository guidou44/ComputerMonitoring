using HardwareManipulation.Components;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Wrappers
{
    public class OpenHardwareWrapper : IOpenHardware
    {
        private IHardware _hardware;
        public OpenHardwareWrapper(IHardware hardware)
        {
            _hardware = hardware;
        }

        public OpenHardwareType HardwareType
        {
            get { return MapHarwareType(_hardware.HardwareType); }
        }

        public string Name
        {
            get { return _hardware.Name; }
        }


        public IOpenSensor[] Sensors 
        {
            get { return From(_hardware.Sensors); } 
        }

        public IOpenHardware[] SubHardware
        {
            get { return From(_hardware.SubHardware); }
        }

        public void Update()
        {
            _hardware.Update();
        }

        private IOpenSensor[] From(ISensor[] sensors)
        {
            IOpenSensor[] output = new IOpenSensor[sensors.Count()];
            for (int i = 0; i < sensors.Count(); i++)
            {
                output[i] = new OpenSensorWrapper(sensors[i]);
            }
            return output;
        }

        private IOpenHardware[] From(IHardware[] subHardwares)
        {
            IOpenHardware[] output = new IOpenHardware[subHardwares.Count()];
            for (int i = 0; i < subHardwares.Count(); i++)
            {
                output[i] = new OpenHardwareWrapper(subHardwares[i]);
            }
            return output;
        }

        private OpenHardwareType MapHarwareType(HardwareType type)
        {
            switch (type)
            {

                case OpenHardwareMonitor.Hardware.HardwareType.GpuNvidia:
                    return OpenHardwareType.GpuNvidia;
                case OpenHardwareMonitor.Hardware.HardwareType.GpuAti:
                    return OpenHardwareType.GpuAti;
                case OpenHardwareMonitor.Hardware.HardwareType.Mainboard:
                    return OpenHardwareType.Mainboard;
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }
    }
}

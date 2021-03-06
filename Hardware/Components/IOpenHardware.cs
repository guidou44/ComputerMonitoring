﻿using Hardware.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Components
{
    public interface IOpenHardware
    {
        void Update();
        OpenHardwareType HardwareType { get; }
        IOpenSensor[] Sensors { get; }
        IOpenHardware[] SubHardware { get; }
        string Name { get; }
    }
}

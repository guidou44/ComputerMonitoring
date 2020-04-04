using HardwareManipulation.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Wrappers
{
    public class PerformanceCounterWrapper : IPerformanceCounter
    {
        private PerformanceCounter _perfCounter;

        public PerformanceCounterWrapper()
        {
            _perfCounter = new PerformanceCounter("Processor", "% Idle Time", "_Total");
        }

        public float NextValue()
        {
            return _perfCounter.NextValue();
        }
    }
}

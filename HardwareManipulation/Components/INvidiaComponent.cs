using HardwareManipulation.Wrappers;
using NvAPIWrapper.GPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Components
{
    public interface INvidiaComponent
    {
        void Initialize();
        void Unload();
        IEnumerable<NvidiaGpuWrapper> GetPhysicalGPUs();
    }
}

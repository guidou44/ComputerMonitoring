using Hardware.Wrappers;
using NvAPIWrapper.GPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Components
{
    public interface INvidiaComponent
    {
        void Initialize();
        void Unload();
        IEnumerable<NvidiaGpuWrapper> GetPhysicalGPUs();
    }
}

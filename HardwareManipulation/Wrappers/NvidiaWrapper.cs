using HardwareManipulation.Components;
using NvAPIWrapper;
using NvAPIWrapper.GPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Wrappers
{
    public class NvidiaWrapper : INvidiaComponent
    {
        public IEnumerable<NvidiaGpuWrapper> GetPhysicalGPUs()
        {
            ICollection<NvidiaGpuWrapper> output = new HashSet<NvidiaGpuWrapper>();
            PhysicalGPU[] gpus = PhysicalGPU.GetPhysicalGPUs();
            gpus.ToList().ForEach(gpu => output.Add(new NvidiaGpuWrapper(gpu)));
            return output;
        }

        public void Initialize()
        {
            NVIDIA.Initialize();
        }

        public void Unload()
        {
            NVIDIA.Unload();
        }
    }
}

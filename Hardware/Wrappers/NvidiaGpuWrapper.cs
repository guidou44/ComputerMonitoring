using NvAPIWrapper.GPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Wrappers
{
    public class NvidiaGpuWrapper
    {
        private PhysicalGPU _nvdiaGpu;
        public NvidiaGpuWrapper(PhysicalGPU nvdiaGpu)
        {
            _nvdiaGpu = nvdiaGpu;
        }

        public NvidiaGpuWrapper() { }

        public virtual string FullName
        { 
            get { return _nvdiaGpu.FullName; } 
        }

        public virtual double CurrentTemperature 
        { 
            get { return (double)_nvdiaGpu.ThermalInformation.ThermalSensors.First().CurrentTemperature; } 
        }

        public virtual double Percentage 
        {
            get { return (double)_nvdiaGpu.UsageInformation.GPU.Percentage; }
        }
    }
}

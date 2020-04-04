using HardwareAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using NvAPIWrapper;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Mosaic;
using HardwareAccess.Enums;
using HardwareManipulation.Components;

namespace HardwareAccess.Connectors
{
    public class NVDIA_API_Connector : ConnectorBase
    {
        INvidiaComponent _nvdiaComponent;
        public NVDIA_API_Connector(INvidiaComponent nvdiaComponent)
        {
            _nvdiaComponent = nvdiaComponent;
            nvdiaComponent.Initialize();
        }

        ~NVDIA_API_Connector()
        {
            _nvdiaComponent.Unload();
        }

        #region Private Methods

        private HardwareInformation GetFirstGpuMake()
        {
            var myGPUs = _nvdiaComponent.GetPhysicalGPUs();

            var gpuMakes = myGPUs.ToList().Select(gU => new HardwareInformation()
            {
                MainValue = gU.FullName,
                ShortName = "GPU",
                UnitSymbol = ""
            });

            return gpuMakes.FirstOrDefault();
        }

        private HardwareInformation GetFirstGpuTemp()
        {
            var myGPUs = _nvdiaComponent.GetPhysicalGPUs();

            var gpuUsages = myGPUs.ToList().Select(gU => new HardwareInformation()
            {
                ShortName = "GPU",
                MainValue = gU.CurrentTemperature,
                UnitSymbol = "°C"
            });

            return gpuUsages.FirstOrDefault();
        }

        private HardwareInformation GetFirstGpuLoad()
        {
            var myGPUs = _nvdiaComponent.GetPhysicalGPUs();

            var gpuUsages = myGPUs.ToList().Select(gU => new HardwareInformation()
            { 
                ShortName = "GPU",
                MainValue = gU.Percentage,
                UnitSymbol = "%"
            });

            return gpuUsages.FirstOrDefault();
        }

        #endregion

        public override HardwareInformation GetValue(MonitoringTarget resource)
        {
            switch (resource)
            {
                case MonitoringTarget.GPU_Make:
                    return GetFirstGpuMake();

                case MonitoringTarget.GPU_Temp:
                    return GetFirstGpuTemp();

                case MonitoringTarget.GPU_Load:
                    return GetFirstGpuLoad();

                default:
                    throw new NotImplementedException($"Monitoring target '{resource}' is not implemented for connector {nameof(NVDIA_API_Connector)}");
            }
        }
    }
}

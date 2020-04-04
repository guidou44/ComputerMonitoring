﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareAccess.Enums;
using HardwareAccess.Models;
using HardwareManipulation.Components;
using OpenHardwareMonitor.Hardware;

namespace HardwareAccess.Connectors
{
    public class SystemIO_Connector : ConnectorBase
    {
        private IDriveInfoProvider _provider;
        public SystemIO_Connector(IDriveInfoProvider provider)
        {
            _provider = provider;
        }


        #region Private Methods

        private HardwareInformation GetDriveUsage(IDriveInfo drive)
        {
            if (drive == null) throw new ArgumentNullException("This drive does'nt exist on current computer");
            return new HardwareInformation()
            {
                MainValue = 100 * (1 - Math.Round((((double )drive.TotalFreeSpace) / ((double)drive.TotalSize)), 2)),
                ShortName = "HDD[" + drive.Name + "]",
                UnitSymbol = "%"
            };

        }

        #endregion

        public override HardwareInformation GetValue(MonitoringTarget resource)
        {
            IEnumerable<IDriveInfo> localDrives = _provider.GetLocalDrive();
            IEnumerable<IDriveInfo> networkDrives = _provider.GetNetworkDrive();

            switch (resource)
            {
                case MonitoringTarget.Primary_HDD_Used_Space:
                    return GetDriveUsage(localDrives.SingleOrDefault(D => D.Name.StartsWith("C"))) ;

                case MonitoringTarget.Secondary_HDD_Used_Space:
                    return GetDriveUsage(localDrives.FirstOrDefault(D => !D.Name.StartsWith("C")));

                case MonitoringTarget.Primary_Network_HDD_Used_Space:
                    
                    return GetDriveUsage(networkDrives.FirstOrDefault());

                case MonitoringTarget.Secondary_Network_HDD_Used_Space:
                    return GetDriveUsage(networkDrives.ElementAtOrDefault(1));

                default:
                    throw new NotImplementedException($"Monitoring target {resource} not implemented for connector {nameof(SystemIO_Connector)}");
            }
        }
    }
}

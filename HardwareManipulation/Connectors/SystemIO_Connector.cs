using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareAccess.Enums;
using HardwareAccess.Models;
using OpenHardwareMonitor.Hardware;

namespace HardwareAccess.Connectors
{
    public class SystemIO_Connector : ConnectorBase
    {

        #region Private Methods

        private static HardwareInformation GetDriveUsage(DriveInfo drive)
        {
            if (drive == null) throw new ArgumentNullException("This drive does<nt exist on current computer");
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
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            var networkDrives = allDrives.Where(D => D.DriveType == DriveType.Network).OrderBy(D => ((int)(D.Name[0])));
            var localDrives = allDrives.Except(networkDrives);

            switch (resource)
            {
                case MonitoringTarget.Primary_HDD_Used_Space:
                    return GetDriveUsage(localDrives.Where(D => D.Name.StartsWith("C")).SingleOrDefault()) ;

                case MonitoringTarget.Secondary_HDD_Used_Space:
                    return GetDriveUsage(localDrives.Where(D => !D.Name.StartsWith("C")).FirstOrDefault());

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

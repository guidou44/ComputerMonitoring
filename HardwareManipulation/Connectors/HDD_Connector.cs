using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareManipulation.Enums;
using HardwareManipulation.Models;
using OpenHardwareMonitor.Hardware;

namespace HardwareManipulation.Connectors
{
    public class HDD_Connector : ConnectorBase
    {
        public HDD_Connector()
        {
        }

        ~HDD_Connector()
        {

        }

        #region Private Methods

        private HardwareInformation GetDriveUsage(DriveInfo drive)
        {
            if (drive == null) throw new ArgumentNullException("This drive does<nt exist on current computer");
            var test = drive.TotalSize;
            var test1 = drive.TotalFreeSpace;
            var test2 = test - test1;
            return new HardwareInformation()
            {
                MainValue = Math.Round((double)((drive.TotalSize - drive.TotalFreeSpace) / drive.TotalSize), 2),
                ShortName = "HDD[" + drive.Name + "]",
                UnitSymbol = "%"
            };

        }

        #endregion

        public override HardwareInformation GetValue(MonitoringTarget ressource)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            var networkDrives = allDrives.Where(D => D.DriveType == DriveType.Network).OrderBy(D => ((int)(D.Name[0])));
            var localDrives = allDrives.Except(networkDrives);

            switch (ressource)
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
                    throw new NotImplementedException($"Monitoring target {ressource} not implemented for connector {nameof(HDD_Connector)}");
            }
        }
    }
}

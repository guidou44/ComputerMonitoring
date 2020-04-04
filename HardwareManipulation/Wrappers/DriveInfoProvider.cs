using HardwareManipulation.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Wrappers
{
    public class DriveInfoProvider : IDriveInfoProvider
    {
        public IEnumerable<IDriveInfo> GetLocalDrive()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            var networkDrives = allDrives.Where(D => D.DriveType == DriveType.Network).OrderBy(D => ((int)(D.Name[0])));
            var localDrives = allDrives.Except(networkDrives);
            List<IDriveInfo> driveInfos = new List<IDriveInfo>();
            foreach (var drive in localDrives)
            {
                driveInfos.Add(new DriveInfoWrapper(drive));
            }

            return driveInfos;
        }

        public IEnumerable<IDriveInfo> GetNetworkDrive()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            var networkDrives = allDrives.Where(D => D.DriveType == DriveType.Network).OrderBy(D => ((int)(D.Name[0])));
            List<IDriveInfo> driveInfos = new List<IDriveInfo>();
            foreach (var drive in networkDrives)
            {
                driveInfos.Add(new DriveInfoWrapper(drive));
            }

            return driveInfos;
        }
    }
}

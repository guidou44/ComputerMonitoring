using Hardware.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Wrappers
{
    public class DriveInfoWrapper : IDriveInfo
    {
        DriveInfo _drive;
        public DriveInfoWrapper(DriveInfo drive)
        {
            _drive = drive;
        }

        public string Name
        {
            get { return _drive.Name; }
        }

        public long TotalFreeSpace
        {
            get { return _drive.TotalFreeSpace; }
        }

        public long TotalSize
        {
            get { return _drive.TotalSize; } 
        }

        public DriveType DriveType
        {
            get { return _drive.DriveType; }
        }

    }
}

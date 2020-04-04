using HardwareManipulation.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Components
{
    public interface IDriveInfo
    {
        string Name { get; }
        long TotalFreeSpace { get; }
        long TotalSize { get; }
        DriveType DriveType { get; }
    }
}

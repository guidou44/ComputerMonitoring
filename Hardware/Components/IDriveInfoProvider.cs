using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Components
{
    public interface IDriveInfoProvider
    {
        IEnumerable<IDriveInfo> GetNetworkDrive();
        IEnumerable<IDriveInfo> GetLocalDrive();
    }
}

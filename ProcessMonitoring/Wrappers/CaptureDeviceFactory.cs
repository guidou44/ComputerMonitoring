using ProcessMonitoring.Models;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Wrappers
{
    public class CaptureDeviceFactory : ICaptureFactory<IPacketCaptureDevice>
    {
        public IPacketCaptureDevice CreateInstance(string reference)
        {
            return new CaptureDeviceWrapper(CaptureDeviceList.Instance.Where(CD => !CD.Description.Contains(reference)).FirstOrDefault());
        }
    }
}

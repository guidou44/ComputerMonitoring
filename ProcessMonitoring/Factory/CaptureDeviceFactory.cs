using System.Linq;
using ProcessMonitoring.Models;
using SharpPcap;

namespace ProcessMonitoring.Factory
{
    public class CaptureDeviceFactory : ICaptureDeviceFactory
    {
        public ICaptureDevice CreateInstance(string reference)
        {
            return CaptureDeviceList.Instance.FirstOrDefault(cd => !cd.Description.Contains(reference));
        }
    }
}

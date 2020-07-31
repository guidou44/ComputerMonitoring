using SharpPcap;

namespace ProcessMonitoring.Models
{
    public interface ICaptureDeviceFactory
    {
        ICaptureDevice CreateInstance(string reference);
    }
}

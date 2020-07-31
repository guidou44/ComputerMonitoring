using System.Collections.Generic;

namespace DesktopAssistant.BL.ProcessWatch
{
    public interface IProcessWatcher
    {
        IEnumerable<IProcessWatch> GetProcessUnderWatch();
        void RegisterPacketCaptureObserver(IPacketObserver packetObserver);
        void StartCapture();
        void StopCapture();
        void AddProcessToWatchList(string processName, bool doCapture);
        void RemoveProcessFromWatchList(string processName);
        void UpdateProcessCaptureInWatchList(string processName, bool doCapture);
    }
}
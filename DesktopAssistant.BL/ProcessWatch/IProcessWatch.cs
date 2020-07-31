using System.Diagnostics;

namespace DesktopAssistant.BL.ProcessWatch
{
    public interface IProcessWatch
    {
        void RegisterReporter(IProcessReporter reporter);
        bool DoCapture { get; }
        bool IsRunning { get; }
        string ProcessName { get; }
        byte Data { get; set; }
    }
}
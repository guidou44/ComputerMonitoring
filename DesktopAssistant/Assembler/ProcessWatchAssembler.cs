using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.ViewModels;
using ProcessMonitoring.Models;

namespace DesktopAssistant.Assembler
{
    public static class ProcessWatchAssembler
    {
        public static ProcessViewModel AssembleFromModel(IProcessWatch processWatch)
        {
            return new ProcessViewModel(processWatch.DoCapture, processWatch.ProcessName, processWatch.IsRunning);
        }

        public static IProcessWatch AssembleFromViewModel(ProcessViewModel processViewModel)
        {
            return new ProcessWatch(processViewModel.ProcessName, processViewModel.DoCapture);
        }
    }
}
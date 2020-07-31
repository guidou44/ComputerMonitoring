using System;
using System.Collections.Generic;
using System.ComponentModel;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.BL.ProcessWatch;

namespace DesktopAssistant.BL
{
    public interface IAppManager : IDisposable
    {
        void RegisterManagerObserver(IManagerObserver observer);
        List<MonitoringTarget> GetMonitoringQueue();
        IHardwareInfo GetCalculatedValue(MonitoringTarget monTarget);
        ICollection<MonitoringTarget> GetAllTargets();
        ICollection<IHardwareInfo> HardwareValues { get; }
        IEnumerable<IProcessWatch> ProcessesUnderWatch { get; }
        void Start();

    }
}
using System.Collections.Generic;

namespace DesktopAssistant.BL.Hardware
{
    public interface IHardwareManager
    {
        IEnumerable<MonitoringTarget> GetInitialTargets();
        ICollection<MonitoringTarget> GetAllTargets();
        IEnumerable<IHardwareInfo> GetCalculatedValues(ICollection<MonitoringTarget> targets);
        IHardwareInfo GetCalculatedValue(MonitoringTarget targets);
    }
}
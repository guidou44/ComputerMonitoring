

using System.Collections.Generic;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.BL.ProcessWatch;
using Prism.Events;

namespace DesktopAssistant.BL.Events
{
    public class OnWatchdogTargetChangedEvent : PubSubEvent<IEnumerable<IProcessWatch>> {}
    public class OnMonitoringTargetsChangedEvent : PubSubEvent<List<MonitoringTarget>> {}
    public class OnMonitoringTargetSelectedEvent : PubSubEvent<KeyValuePair<MonitoringTarget, bool>> {}
}

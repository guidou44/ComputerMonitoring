using ComputerRessourcesMonitoring.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerRessourcesMonitoring.Events
{
    public class OnWatchdogTargetChangedEvent : PubSubEvent<string> { }
    public class OnMonitoringTargetsChangedEvent : PubSubEvent<Queue<MonitoringTarget>> { }
    public class OnMonitoringTargetSelectedEvent : PubSubEvent<KeyValuePair<MonitoringTarget, bool>> { }
}

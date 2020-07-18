
using DesktopAssistant.ViewModels;
using Hardware.Enums;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistant.Events
{
    public class OnWatchdogTargetChangedEvent : PubSubEvent<ObservableCollection<ProcessViewModel>> { }
    public class OnMonitoringTargetsChangedEvent : PubSubEvent<List<MonitoringTarget>> { }
    public class OnMonitoringTargetSelectedEvent : PubSubEvent<KeyValuePair<MonitoringTarget, bool>> { }
}

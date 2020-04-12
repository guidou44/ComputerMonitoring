using ComputerRessourcesMonitoring.ViewModels;
using HardwareAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static ComputerRessourcesMonitoring.ViewModels.MonitoringTargetViewModel;

namespace ComputerMonitoringTests.ComputerResourcesMonitoringTests.ViewModels
{
    public class MonitoringTargetViewModelTest
    {
        [Fact]
        public void GivenMonitoredTarget_WhenMonitoringStatusChanged_ThenProperEventIsRaised()
        {
            KeyValuePair<MonitoringTarget, bool> eventResult = new KeyValuePair<MonitoringTarget, bool>(MonitoringTarget.RAM_Usage, false);
            MonitoringTargetViewModel targetSubject = new MonitoringTargetViewModel(MonitoringTarget.CPU_Load);
            targetSubject.IsSelected = true;
            targetSubject.DisplayName = "TEST";
            targetSubject.SelectionChangedEvent += new SelectionChanged((arg) => eventResult = arg);

            targetSubject.PublishMonitoringOptionStatusCommand.Execute(null);

            Assert.Equal(new KeyValuePair<MonitoringTarget, bool>(MonitoringTarget.CPU_Load, true), eventResult);

            targetSubject.SelectionChangedEvent -= (arg) => eventResult = arg;
        }
    }
}

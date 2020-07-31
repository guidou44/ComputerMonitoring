using System.Collections.Generic;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.ViewModels;
using Xunit;
using static DesktopAssistant.ViewModels.MonitoringTargetViewModel;

namespace DesktopAssistant.Tests.DesktopAssistant.Tests.ViewModels
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
            SelectionChanged selectionChangedTestHandler = new SelectionChanged((arg) => eventResult = arg);

            targetSubject.SelectionChangedEvent += selectionChangedTestHandler;
            targetSubject.PublishMonitoringOptionStatusCommand.Execute(null);

            Assert.Equal(new KeyValuePair<MonitoringTarget, bool>(MonitoringTarget.CPU_Load, true), eventResult);

            targetSubject.SelectionChangedEvent -= selectionChangedTestHandler;
        }
    }
}
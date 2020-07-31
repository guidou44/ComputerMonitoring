using System.Collections.Generic;
using System.Linq;
using DesktopAssistant.BL;
using DesktopAssistant.BL.Events;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.Tests.DesktopAssistant.Tests.Helper;
using DesktopAssistant.ViewModels;
using Hardware.Models;
using Moq;
using Prism.Events;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.Tests.ViewModels
{
    public class HardwareSettingsViewModelTest
    {
        private readonly List<MonitoringTarget> _currentTargets = new List<MonitoringTarget>() { MonitoringTarget.CPU_Load, MonitoringTarget.RAM_Usage };

        private readonly List<MonitoringTarget> _allTargets = new List<MonitoringTarget>()
        {
            MonitoringTarget.CPU_Load,
            MonitoringTarget.RAM_Usage,
            MonitoringTarget.CPU_Temp
        };

        private readonly IHardwareInfo _motherBoard = new HardwareInformation(){MainValue = "TEST_MB"};
        private readonly IHardwareInfo _cpuMake = new HardwareInformation(){MainValue = "TEST_CPU"};
        private readonly IHardwareInfo _gpuMake = new HardwareInformation(){MainValue = "TEST_GPU"};

        private Mock<IAppManager> GivenAppManagerMock()
        {
            Mock<IAppManager> managerMock = new Mock<IAppManager>();
            managerMock.Setup(m => m.GetAllTargets()).Returns(_allTargets);
            managerMock.Setup(m => m.GetCalculatedValue(MonitoringTarget.Mother_Board_Make)).Returns(_motherBoard);
            managerMock.Setup(m => m.GetCalculatedValue(MonitoringTarget.CPU_Make)).Returns(_cpuMake);
            managerMock.Setup(m => m.GetCalculatedValue(MonitoringTarget.GPU_Make)).Returns(_gpuMake);
            managerMock.Setup(m => m.GetMonitoringQueue()).Returns(_currentTargets);
            return managerMock;
        }

        private HardwareSettingsViewModel GivenSettingsViewModelSubject()
        {
            Mock<IEventAggregator> eventManager = new Mock<IEventAggregator>();
            Mock<OnWatchdogTargetChangedEvent> watchdogTargetChangedEvent = new Mock<OnWatchdogTargetChangedEvent>();
            Mock<OnMonitoringTargetsChangedEvent> monTargetChangedEvent = new Mock<OnMonitoringTargetsChangedEvent>();
            ComputerMonitoringTestHelper.SetupEventAggMockBehaviour(eventManager, 
                watchdogTargetChangedEvent.Object, 
                monTargetChangedEvent.Object);
            Mock<IAppManager> appManagerMock = GivenAppManagerMock();
            
            HardwareSettingsViewModel hardwareSettingsVm = new HardwareSettingsViewModel(eventManager.Object, appManagerMock.Object);
            return hardwareSettingsVm;
        }

        [Fact]
        public void GivenCurrentlyMonitoredTargets_WhenRemoveSingleTarget_ThenItDoesNotPublishNewCollection()
        {
            List<MonitoringTarget> updatedTargets = null;
            Mock<IEventAggregator> eventManager = new Mock<IEventAggregator>();
            Mock<OnWatchdogTargetChangedEvent> watchdogTargetChangedEvent = new Mock<OnWatchdogTargetChangedEvent>();
            Mock<OnMonitoringTargetsChangedEvent> monTargetChangedEvent = new Mock<OnMonitoringTargetsChangedEvent>();
            monTargetChangedEvent.Setup(e => e.Publish(It.IsAny<List<MonitoringTarget>>())).Verifiable();
            monTargetChangedEvent.Setup(e => e.Publish(It.IsAny<List<MonitoringTarget>>())).Callback<List<MonitoringTarget>>(lru => updatedTargets = lru);
            ComputerMonitoringTestHelper.SetupEventAggMockBehaviour(eventManager, watchdogTargetChangedEvent.Object, monTargetChangedEvent.Object);
            Mock<IAppManager> appManagerMock = GivenAppManagerMock();
            
            HardwareSettingsViewModel hardwareSettingsVm = new HardwareSettingsViewModel(eventManager.Object, appManagerMock.Object);
            updatedTargets = null;
            MonitoringTargetViewModel elementToUpdate = hardwareSettingsVm.MonitoringOptionsCollection.SingleOrDefault(mo => mo.Type == _currentTargets.LastOrDefault());
            elementToUpdate.IsSelected = false;

            elementToUpdate.PublishMonitoringOptionStatusCommand.Execute(new KeyValuePair<MonitoringTarget, bool>(elementToUpdate.Type, elementToUpdate.IsSelected));

            Assert.Null(updatedTargets);
            monTargetChangedEvent.Verify(e => e.Publish(It.IsAny<List<MonitoringTarget>>()), Times.Once);
        }

        [Fact]
        public void GivenInstantiateSettingVm_WhenInitComponent_ThenInitGenericInfoProper()
        {
            HardwareSettingsViewModel hardwareSettingsVm = GivenSettingsViewModelSubject();

            Assert.Equal(_motherBoard.MainValue as string, hardwareSettingsVm.MotherBoardMake);
            Assert.Equal(_cpuMake.MainValue as string, hardwareSettingsVm.CpuMake);
            Assert.Equal(_gpuMake.MainValue as string, hardwareSettingsVm.GpuMake);
            Assert.Equal(_allTargets.Count(), hardwareSettingsVm.MonitoringOptionsCollection.Count());
            Assert.All(_currentTargets, c => Assert.True(hardwareSettingsVm.MonitoringOptionsCollection.SingleOrDefault(mo => mo.Type == c).IsSelected));
            Assert.All(hardwareSettingsVm.MonitoringOptionsCollection.Where(mo => !_currentTargets.Contains(mo.Type)), mo => Assert.False(mo.IsSelected));
        }

        [Fact]
        public void GivenMaxTargetCountTopped_WhenAddingAdditionalTarget_ThenItRemovesLastRecentlyUsed()
        {
            List<MonitoringTarget> updatedTargets = null;
            MonitoringTarget expectedFirstTarget = _currentTargets.ElementAt(1);
            MonitoringTarget notCurrentlyMonitored = _allTargets.Except(_currentTargets).FirstOrDefault();

            Mock<IEventAggregator> eventManager = new Mock<IEventAggregator>();
            Mock<OnWatchdogTargetChangedEvent> watchdogTargetChangedEvent = new Mock<OnWatchdogTargetChangedEvent>();
            Mock<OnMonitoringTargetsChangedEvent> monTargetChangedEvent = new Mock<OnMonitoringTargetsChangedEvent>();
            monTargetChangedEvent.Setup(e => e.Publish(It.IsAny<List<MonitoringTarget>>())).Verifiable();
            monTargetChangedEvent.Setup(e => e.Publish(It.IsAny<List<MonitoringTarget>>())).Callback<List<MonitoringTarget>>(lru => updatedTargets = lru);
            ComputerMonitoringTestHelper.SetupEventAggMockBehaviour(eventManager, watchdogTargetChangedEvent.Object, monTargetChangedEvent.Object);
            Mock<IAppManager> appManagerMock = GivenAppManagerMock();

            HardwareSettingsViewModel hardwareSettingsVm = new HardwareSettingsViewModel(eventManager.Object, appManagerMock.Object);
            MonitoringTargetViewModel elementToAdd = hardwareSettingsVm.MonitoringOptionsCollection.SingleOrDefault(mo => mo.Type == notCurrentlyMonitored);
            elementToAdd.IsSelected = true;
            
            elementToAdd.PublishMonitoringOptionStatusCommand.Execute(new KeyValuePair<MonitoringTarget, bool>(elementToAdd.Type, elementToAdd.IsSelected));


            Assert.Equal(expectedFirstTarget, updatedTargets.FirstOrDefault());
            monTargetChangedEvent.Verify(e => e.Publish(It.IsAny<List<MonitoringTarget>>()), Times.Exactly(2));
        }

        [Fact]
        public void GivenMaxTargetNotCountTopped_WhenAddingAdditionalTarget_ThenItAppendsIt()
        {
            List<MonitoringTarget> updatedTargets = null;
            MonitoringTarget expectedFirstTarget = _currentTargets.ElementAt(0);
            MonitoringTarget notCurrentlyMonitored = _allTargets.Except(_currentTargets).FirstOrDefault();

            Mock<IEventAggregator> eventManager = new Mock<IEventAggregator>();
            Mock<OnWatchdogTargetChangedEvent> watchdogTargetChangedEvent = new Mock<OnWatchdogTargetChangedEvent>();
            Mock<OnMonitoringTargetsChangedEvent> monTargetChangedEvent = new Mock<OnMonitoringTargetsChangedEvent>();
            monTargetChangedEvent.Setup(e => e.Publish(It.IsAny<List<MonitoringTarget>>())).Verifiable();
            monTargetChangedEvent.Setup(e => e.Publish(It.IsAny<List<MonitoringTarget>>())).Callback<List<MonitoringTarget>>(lru => updatedTargets = lru);
            ComputerMonitoringTestHelper.SetupEventAggMockBehaviour(eventManager, watchdogTargetChangedEvent.Object, monTargetChangedEvent.Object);
            Mock<IAppManager> appManagerMock = GivenAppManagerMock();

            HardwareSettingsViewModel hardwareSettingsVm = new HardwareSettingsViewModel(eventManager.Object, appManagerMock.Object);
            hardwareSettingsVm.MaxAllowedMonTargets++;
            MonitoringTargetViewModel elementToAdd = hardwareSettingsVm.MonitoringOptionsCollection.SingleOrDefault(mo => mo.Type == notCurrentlyMonitored);
            elementToAdd.IsSelected = true;

            elementToAdd.PublishMonitoringOptionStatusCommand.Execute(new KeyValuePair<MonitoringTarget, bool>(elementToAdd.Type, elementToAdd.IsSelected));

            Assert.Equal(expectedFirstTarget, updatedTargets.FirstOrDefault());
            monTargetChangedEvent.Verify(e => e.Publish(It.IsAny<List<MonitoringTarget>>()), Times.Exactly(2));
        }
    }
}

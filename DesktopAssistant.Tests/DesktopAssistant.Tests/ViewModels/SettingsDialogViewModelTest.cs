using Common.Reports;
using DesktopAssistantTests.DesktopAssistantTests.Helper;
using DesktopAssistantTests.ProcessMonitoringTests.Helpers;
using DesktopAssistant.Events;
using DesktopAssistant.ViewModels;
using Hardware.Enums;
using Hardware;
using Moq;
using Prism.Events;
using ProcessMonitoring;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesktopAssistantTests.DesktopAssistantTests.ViewModels
{
    public class SettingsDialogViewModelTest
    {
        private List<MonitoringTarget> _currentTargets = new List<MonitoringTarget>() { MonitoringTarget.CPU_Load, MonitoringTarget.RAM_Usage };

        [Fact]
        public void GivenInstantiateSettingVm_WhenInitComponent_ThenInitGenericInfoProper()
        {
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            Mock<DataManager> dataManager = ComputerMonitoringTestHelper.GivenDataManagerMock();
            ProcessWatchDog watchdog = ComputerMonitoringTestHelper.GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();

            SettingsDialogViewModel settingsVm = new SettingsDialogViewModel(ComputerMonitoringTestHelper.EXPECTED_PROCESS,
                                                                             eventManager,
                                                                             _currentTargets,
                                                                             dataManager.Object,
                                                                             watchdog, reporter);

            Assert.Equal(ComputerMonitoringTestHelper.MotherboardMake.MainValue as string, settingsVm.MotherBoardMake);
            Assert.Equal(ComputerMonitoringTestHelper.CpuMake.MainValue as string, settingsVm.CpuMake);
            Assert.Equal(ComputerMonitoringTestHelper.GpuMake.MainValue as string, settingsVm.GpuMake);
            Assert.Equal(ComputerMonitoringTestHelper.EXPECTED_TARGETS.Count(), settingsVm.MonitoringOptionsCollection.Count());
            Assert.All(_currentTargets, c => Assert.True(settingsVm.MonitoringOptionsCollection.SingleOrDefault(mo => mo.Type == c).IsSelected));
            Assert.All(settingsVm.MonitoringOptionsCollection.Where(mo => !_currentTargets.Contains(mo.Type)), mo => Assert.False(mo.IsSelected));
        }

        [Fact]
        public void GivenMaxTargetCountNotTopped_WhenAddingAdditionalTarget_ThenItRemovesLastRecentlyUsed()
        {
            List<MonitoringTarget> updatedTargets = null;
            MonitoringTarget expectedFirstTarget = _currentTargets.ElementAt(1);
            MonitoringTarget notCurrentlyMonitored =
                ComputerMonitoringTestHelper.EXPECTED_TARGETS.Where(t => !_currentTargets.Contains(t)).FirstOrDefault();

            Mock<IEventAggregator> eventManager = new Mock<IEventAggregator>();
            Mock<OnWatchdogTargetChangedEvent> watchdogTargetChangedEvent = new Mock<OnWatchdogTargetChangedEvent>();
            Mock<OnMonitoringTargetsChangedEvent> monTargetChangedEvent = new Mock<OnMonitoringTargetsChangedEvent>();
            monTargetChangedEvent.Setup(e => e.Publish(It.IsAny<List<MonitoringTarget>>())).Verifiable();
            monTargetChangedEvent.Setup(e => e.Publish(It.IsAny<List<MonitoringTarget>>())).Callback<List<MonitoringTarget>>(lru => updatedTargets = lru);
            ComputerMonitoringTestHelper.SetupEventAggMockBehaviour(eventManager, watchdogTargetChangedEvent.Object, monTargetChangedEvent.Object);

            Mock<DataManager> dataManager = ComputerMonitoringTestHelper.GivenDataManagerMock();
            ProcessWatchDog watchdog = ComputerMonitoringTestHelper.GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();         
            SettingsDialogViewModel settingsVm = new SettingsDialogViewModel(ComputerMonitoringTestHelper.EXPECTED_PROCESS,
                                                                             eventManager.Object,
                                                                             _currentTargets,
                                                                             dataManager.Object,
                                                                             watchdog, reporter);
            MonitoringTargetViewModel elementToAdd = settingsVm.MonitoringOptionsCollection.SingleOrDefault(mo => mo.Type == notCurrentlyMonitored);
            elementToAdd.IsSelected = true;

            elementToAdd.PublishMonitoringOptionStatusCommand.Execute(new KeyValuePair<MonitoringTarget, bool>(elementToAdd.Type, elementToAdd.IsSelected));


            Assert.Equal(expectedFirstTarget, updatedTargets.FirstOrDefault());
            monTargetChangedEvent.Verify(e => e.Publish(It.IsAny<List<MonitoringTarget>>()), Times.Exactly(2));
        }

        [Fact]
        public void GivenMaxTargetCountTopped_WhenAddingAdditionalTarget_ThenItAppendsIt()
        {
            List<MonitoringTarget> updatedTargets = null;
            MonitoringTarget expectedFirstTarget = _currentTargets.ElementAt(0);
            MonitoringTarget notCurrentlyMonitored =
                ComputerMonitoringTestHelper.EXPECTED_TARGETS.Where(t => !_currentTargets.Contains(t)).FirstOrDefault();

            Mock<IEventAggregator> eventManager = new Mock<IEventAggregator>();
            Mock<OnWatchdogTargetChangedEvent> watchdogTargetChangedEvent = new Mock<OnWatchdogTargetChangedEvent>();
            Mock<OnMonitoringTargetsChangedEvent> monTargetChangedEvent = new Mock<OnMonitoringTargetsChangedEvent>();
            monTargetChangedEvent.Setup(e => e.Publish(It.IsAny<List<MonitoringTarget>>())).Verifiable();
            monTargetChangedEvent.Setup(e => e.Publish(It.IsAny<List<MonitoringTarget>>())).Callback<List<MonitoringTarget>>(lru => updatedTargets = lru);
            ComputerMonitoringTestHelper.SetupEventAggMockBehaviour(eventManager, watchdogTargetChangedEvent.Object, monTargetChangedEvent.Object);

            Mock<DataManager> dataManager = ComputerMonitoringTestHelper.GivenDataManagerMock();
            ProcessWatchDog watchdog = ComputerMonitoringTestHelper.GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            SettingsDialogViewModel settingsVm = new SettingsDialogViewModel(ComputerMonitoringTestHelper.EXPECTED_PROCESS,
                                                                             eventManager.Object,
                                                                             _currentTargets,
                                                                             dataManager.Object,
                                                                             watchdog, reporter);
            settingsVm.MaxAllowedMonTargets++;
            MonitoringTargetViewModel elementToAdd = settingsVm.MonitoringOptionsCollection.SingleOrDefault(mo => mo.Type == notCurrentlyMonitored);
            elementToAdd.IsSelected = true;

            elementToAdd.PublishMonitoringOptionStatusCommand.Execute(new KeyValuePair<MonitoringTarget, bool>(elementToAdd.Type, elementToAdd.IsSelected));

            Assert.Equal(expectedFirstTarget, updatedTargets.FirstOrDefault());
            monTargetChangedEvent.Verify(e => e.Publish(It.IsAny<List<MonitoringTarget>>()), Times.Exactly(2));
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
            Mock<DataManager> dataManager = ComputerMonitoringTestHelper.GivenDataManagerMock();
            ProcessWatchDog watchdog = ComputerMonitoringTestHelper.GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            SettingsDialogViewModel settingsVm = new SettingsDialogViewModel(ComputerMonitoringTestHelper.EXPECTED_PROCESS,
                                                                             eventManager.Object,
                                                                             _currentTargets,
                                                                             dataManager.Object,
                                                                             watchdog, reporter);
            updatedTargets = null;
            MonitoringTargetViewModel elementToUpdate = settingsVm.MonitoringOptionsCollection.SingleOrDefault(mo => mo.Type == _currentTargets.LastOrDefault());
            elementToUpdate.IsSelected = false;

            elementToUpdate.PublishMonitoringOptionStatusCommand.Execute(new KeyValuePair<MonitoringTarget, bool>(elementToUpdate.Type, elementToUpdate.IsSelected));

            Assert.Null(updatedTargets);
            monTargetChangedEvent.Verify(e => e.Publish(It.IsAny<List<MonitoringTarget>>()), Times.Once);
        }

        [Fact]
        public void GivenCurrentlyWatchedProcesses_WhenEnableRemove_ThenCanRemoveAllProcesses()
        {
            SettingsDialogViewModel settingsVm = GivenSettingsViewModelSubject();

            Assert.True(settingsVm.RemoveFromWatchdogCollectionCommand.CanExecute(null));
            settingsVm.RemoveFromWatchdogCollectionCommand.Execute(null);
            
            Assert.True(settingsVm.CanRemoveWatchdogTargets);
            Assert.All(settingsVm.ProcessesUnderWatch, p => Assert.True(p.CanRemoveProcessWatch));
        }

        [Fact]
        public void GivenCurrentlyWatchedProcessesWithEnabledRemove_WhenDisableRemove_ThenCantRemoveAllProcesses()
        {
            SettingsDialogViewModel settingsVm = GivenSettingsViewModelSubject();
            settingsVm.RemoveFromWatchdogCollectionCommand.Execute(null);

            settingsVm.StopRemovingWatchdogProcessCommand.Execute(null);

            Assert.False(settingsVm.CanRemoveWatchdogTargets);
            Assert.All(settingsVm.ProcessesUnderWatch, p => Assert.False(p.CanRemoveProcessWatch));
        }


        [Fact]
        public void GivenCurrentlyWatchProcesses_WhenAddProcessToWatch_ThenItAddsProper()
        {
            SettingsDialogViewModel settingsVm = GivenSettingsViewModelSubject();
            settingsVm.RemoveFromWatchdogCollectionCommand.Execute(null);
            settingsVm.CanRemoveWatchdogTargets = false;

            Assert.True(settingsVm.AddToWatchdogCollectionCommand.CanExecute(null));
            settingsVm.AddToWatchdogCollectionCommand.Execute(null);

            Assert.Equal(ComputerMonitoringTestHelper.EXPECTED_PROCESS.Count() + 1, settingsVm.ProcessesUnderWatch.Count());
            Assert.NotEqual(ComputerMonitoringTestHelper.EXPECTED_PROCESS.Last(), settingsVm.ProcessesUnderWatch.Last());
            Assert.True(settingsVm.ProcessesUnderWatch.Last().Check4PacketExchange);
        }

        [Fact]
        public void GivenCurrentlyWatchedProcesses_WhenRemoveProcess_ThenProperEventRaisesAndCollectionIsUpdated()
        {
            ObservableCollection<ProcessViewModel> updatedProcessWatch = null;
            int wantedElementCount = ComputerMonitoringTestHelper.EXPECTED_PROCESS.Count() - 1;
            IEnumerable<ProcessViewModel> expectedProcessWatch = ComputerMonitoringTestHelper.EXPECTED_PROCESS.Take(wantedElementCount);

            Mock<IEventAggregator> eventManager = new Mock<IEventAggregator>();
            Mock<OnWatchdogTargetChangedEvent> watchdogTargetChangedEvent = new Mock<OnWatchdogTargetChangedEvent>();
            Mock<OnMonitoringTargetsChangedEvent> monTargetChangedEvent = new Mock<OnMonitoringTargetsChangedEvent>();
            watchdogTargetChangedEvent.Setup(e => e.Publish(It.IsAny<ObservableCollection<ProcessViewModel>>())).Verifiable();
            watchdogTargetChangedEvent.Setup(e => e.Publish(It.IsAny<ObservableCollection<ProcessViewModel>>()))
                                      .Callback<ObservableCollection<ProcessViewModel>>(ptw => updatedProcessWatch = ptw);
            ComputerMonitoringTestHelper.SetupEventAggMockBehaviour(eventManager, 
                                                                    watchdogTargetChangedEvent.Object, 
                                                                    monTargetChangedEvent.Object);
            
            Mock<DataManager> dataManager = ComputerMonitoringTestHelper.GivenDataManagerMock();
            ProcessWatchDog watchdog = ComputerMonitoringTestHelper.GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            SettingsDialogViewModel settingsVm = new SettingsDialogViewModel(ComputerMonitoringTestHelper.EXPECTED_PROCESS,
                                                                             eventManager.Object,
                                                                             _currentTargets,
                                                                             dataManager.Object,
                                                                             watchdog, reporter);
            settingsVm.RemoveFromWatchdogCollectionCommand.Execute(null);

            settingsVm.ProcessesUnderWatch.Last().RemoveProcessWatchCommand.Execute(null);

            Assert.Equal(expectedProcessWatch, updatedProcessWatch);
            Assert.Equal(wantedElementCount, updatedProcessWatch.Count());
            Assert.NotEqual(ComputerMonitoringTestHelper.EXPECTED_PROCESS.Count(), updatedProcessWatch.Count());
        }

        [Fact]
        public void GivenCurrentlyWatchedProcesses_WhenChangeLastProcess_ThenProperEventRaisesAndCollectionIsUpdated()
        {
            ObservableCollection<ProcessViewModel> updatedProcessWatch = null;
            int wantedElementCount = ComputerMonitoringTestHelper.EXPECTED_PROCESS.Count() - 1;
            IEnumerable<ProcessViewModel> expectedProcessWatch = ComputerMonitoringTestHelper.EXPECTED_PROCESS.Take(wantedElementCount);
            Process noteExpectedProcess = ComputerMonitoringTestHelper.EXPECTED_PROCESS.Last().Process;
            Process expectedProcess = WatchDogTestHelper.GivenSecondRunningProcess();

            Mock<IEventAggregator> eventManager = new Mock<IEventAggregator>();
            Mock<OnWatchdogTargetChangedEvent> watchdogTargetChangedEvent = new Mock<OnWatchdogTargetChangedEvent>();
            Mock<OnMonitoringTargetsChangedEvent> monTargetChangedEvent = new Mock<OnMonitoringTargetsChangedEvent>();
            watchdogTargetChangedEvent.Setup(e => e.Publish(It.IsAny<ObservableCollection<ProcessViewModel>>())).Verifiable();
            watchdogTargetChangedEvent.Setup(e => e.Publish(It.IsAny<ObservableCollection<ProcessViewModel>>()))
                                      .Callback<ObservableCollection<ProcessViewModel>>(ptw => updatedProcessWatch = ptw);
            ComputerMonitoringTestHelper.SetupEventAggMockBehaviour(eventManager,
                                                                    watchdogTargetChangedEvent.Object,
                                                                    monTargetChangedEvent.Object);

            Mock<ProcessWatchDog> watchDog = new Mock<ProcessWatchDog>();
            watchDog.Setup(w => w.GetProcessesByName(ComputerMonitoringTestHelper.EXPECTED_PROCESS.Last().ProcessName))
                    .Returns(new List<Process>() { expectedProcess });


            Mock<DataManager> dataManager = ComputerMonitoringTestHelper.GivenDataManagerMock();            
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            SettingsDialogViewModel settingsVm = new SettingsDialogViewModel(ComputerMonitoringTestHelper.EXPECTED_PROCESS,
                                                                             eventManager.Object,
                                                                             _currentTargets,
                                                                             dataManager.Object,
                                                                             watchDog.Object, reporter);

            settingsVm.ProcessesUnderWatch.Last().ChangeWatchdogTargetCommand.Execute(null);

            Assert.Equal(expectedProcess, updatedProcessWatch.Last().Process);
            Assert.NotEqual(noteExpectedProcess, updatedProcessWatch.Last().Process);
            Assert.Equal(ComputerMonitoringTestHelper.EXPECTED_PROCESS.Count(), updatedProcessWatch.Count());
        }









        #region Private Methods

        private SettingsDialogViewModel GivenSettingsViewModelSubject()
        {
            Mock<IEventAggregator> eventManager = new Mock<IEventAggregator>();
            Mock<OnWatchdogTargetChangedEvent> watchdogTargetChangedEvent = new Mock<OnWatchdogTargetChangedEvent>();
            Mock<OnMonitoringTargetsChangedEvent> monTargetChangedEvent = new Mock<OnMonitoringTargetsChangedEvent>();
            ComputerMonitoringTestHelper.SetupEventAggMockBehaviour(eventManager, 
                                                                    watchdogTargetChangedEvent.Object, 
                                                                    monTargetChangedEvent.Object);

            Mock<DataManager> dataManager = ComputerMonitoringTestHelper.GivenDataManagerMock();
            ProcessWatchDog watchdog = ComputerMonitoringTestHelper.GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            SettingsDialogViewModel settingsVm = new SettingsDialogViewModel(ComputerMonitoringTestHelper.EXPECTED_PROCESS,
                                                                             eventManager.Object,
                                                                             _currentTargets,
                                                                             dataManager.Object,
                                                                             watchdog, reporter);
            return settingsVm;
        }

        #endregion
    }
}

using Common.MailClient;
using Common.Reports;
using ComputerMonitoringTests.ComputerResourcesMonitoringTests.Helper;
using ComputerMonitoringTests.ProcessMonitoringTests.Helpers;
using ComputerResourcesMonitoring.Models;
using ComputerRessourcesMonitoring.Events;
using ComputerRessourcesMonitoring.Models;
using ComputerRessourcesMonitoring.ViewModels;
using HardwareAccess.Enums;
using HardwareAccess.Models;
using HardwareManipulation;
using Moq;
using Prism.Events;
using ProcessMonitoring;
using ProcessMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Xunit;
using static ComputerResourcesMonitoring.Models.ComputerMonitoringManagerModel;
using static ProcessMonitoring.ProcessWatchDog;

namespace ComputerMonitoringTests.ComputerResourcesMonitoringTests.Models
{
    public class ComputerMonitoringManagerModelTest
    {
        private const string PROCESS_NAME = "TEST_PROCESS";
        private const string REFRESH_ERROR_MESSAGE = "refreshErrorOccured";
        private const int EXPECTED_TIMER_DELAY = 900;
        private bool _watchDogJobSetted = false;
        private bool _watchDogThreadStarted = false;
        private bool _timerStarted = false;
        private bool _timerElapsed = false;
        private int _refreshPolldelay;
        private bool _processWatchWasInitialized;
        private bool _packetExchangedEventRegistered = false;

        private List<MonitoringTarget> _expectedTargets = new List<MonitoringTarget>() { MonitoringTarget.CPU_Load, MonitoringTarget.CPU_Temp };
        private List<HardwareInformation> _expectedHardwareInfo = new List<HardwareInformation>()
        {
            new HardwareInformation() { MainValue = 10, ShortName = "TEST1", UnitSymbol = "#"},
            new HardwareInformation() { MainValue = 20, ShortName = "TEST2", UnitSymbol = "Ø"}
        };

        private int _eventAggSubsribeCount = 0;

        [Fact]
        public void GivenValidWatchdog_WhenInstantiateManager_ThenItInitWatchdogProper()
        {
            _processWatchWasInitialized = false;
            _watchDogJobSetted = false;
            _watchDogJobSetted = false;
            _packetExchangedEventRegistered = false;
            ComputerMonitoringManagerModel managerSubject = GivenComputerManagerModel();

            Assert.True(managerSubject.ProcessesUnderWatch.Count() == 1);
            Assert.True(_processWatchWasInitialized);
            Assert.True(_watchDogThreadStarted);
            Assert.True(_watchDogJobSetted);
            Assert.True(_packetExchangedEventRegistered);
        }

        [Fact]
        public void GivenInstantiateManagerModel_WhenFailsInInitWatchDog_ThenItAbortWatchdogProper()
        {
            _processWatchWasInitialized = false;
            _watchDogJobSetted = false;
            _watchDogThreadStarted = true;
            _packetExchangedEventRegistered = false;
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            DataManager dataManager = GivenValidDataManager();
            ProcessWatchDog watchdog = GivenInvalidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            IThread watchdogThread = GivenWatchdigThread();
            ITimer timer = GivenRefreshTimer();

            ComputerMonitoringManagerModel managerSubject =
                new ComputerMonitoringManagerModel(eventManager, dataManager, watchdog, reporter, timer, watchdogThread);

            Assert.Empty(managerSubject.ProcessesUnderWatch);
            Assert.True(_processWatchWasInitialized);
            Assert.False(_watchDogThreadStarted);
            Assert.False(_watchDogJobSetted);
            Assert.False(_packetExchangedEventRegistered);
        }

        [Fact]
        public void GivenInstantiateManagerModel_WhenRefreshMonitoringInfo_ThenItAssignMonitoringValues()
        {
            ComputerMonitoringManagerModel managerSubject = GivenComputerManagerModel();

            Assert.True(managerSubject.HardwareValues.Count() == 2);
            Assert.True(_expectedHardwareInfo.All(hi => managerSubject.HardwareValues.Contains(hi)));
        }

        [Fact]
        public void GivenInstantiateManagerModel_WhenRefreshMonitoringInfoFails_ThenItLogsException()
        {
            _processWatchWasInitialized = false;
            _watchDogJobSetted = false;
            _watchDogThreadStarted = true;
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            DataManager dataManager = GivenInvalidDataManager();
            ProcessWatchDog watchdog = GivenValidWatchDog();
            Reporter reporter = GivenReporter();
            IThread watchdogThread = GivenWatchdigThread();
            ITimer timer = GivenRefreshTimer();
            string currentDirectory = Directory.GetCurrentDirectory();
            string directoryPath = Path.Combine(currentDirectory, "Exception_Logs");
            ComputerMonitoringTestHelper.GivenDeletedDirectoryIfAlreadyExists(directoryPath);
            Assert.False(Directory.Exists(directoryPath));

            ComputerMonitoringManagerModel managerSubject =
                new ComputerMonitoringManagerModel(eventManager, dataManager, watchdog, reporter, timer, watchdogThread);


            Assert.True(Directory.Exists(directoryPath));
        }

        [Fact]
        public void GivenInstantiateManagerModel_WhenInitTimer_ThenItInitProper()
        {
            ComputerMonitoringManagerModel managerSubject = GivenComputerManagerModel();

            Assert.Equal(EXPECTED_TIMER_DELAY, _refreshPolldelay);
            Assert.True(_timerStarted);
        }

        [Fact]
        public void GivenInstantiateManagerModel_WhenSubscribeToEvents_ThenItSubscriveToProperEvents()
        {
            _eventAggSubsribeCount = 0;

            ComputerMonitoringManagerModel managerSubject = GivenComputerManagerModel();

            Assert.Equal(2, _eventAggSubsribeCount);
        }

        [Fact]
        public void GivenManagerModel_WhenDispose_ThenItDisposeEverythingProper()
        {
            _timerStarted = true;
            ComputerMonitoringManagerModel managerSubject = GivenComputerManagerModel();

            managerSubject.Dispose();

            Assert.False(_timerStarted);
        }

        [Fact]
        public void GivenPassedInWatchdog_WhenGetWatchdog_ThenItReturnsSameInstance()
        {
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            DataManager dataManager = GivenValidDataManager();
            ProcessWatchDog watchdog = GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            IThread watchdogThread = GivenWatchdigThread();
            ITimer timer = GivenRefreshTimer();

            ComputerMonitoringManagerModel managerSubject =
                new ComputerMonitoringManagerModel(eventManager, dataManager, watchdog, reporter, timer, watchdogThread);

            Assert.Equal(watchdog.GetHashCode(), managerSubject.GetWatchDog().GetHashCode());
        }

        [Fact]
        public void GivenPassedDataManager_WhenGetDataManager_ThenItReturnsSameInstance()
        {
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            DataManager dataManager = GivenValidDataManager();
            ProcessWatchDog watchdog = GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            IThread watchdogThread = GivenWatchdigThread();
            ITimer timer = GivenRefreshTimer();

            ComputerMonitoringManagerModel managerSubject =
                new ComputerMonitoringManagerModel(eventManager, dataManager, watchdog, reporter, timer, watchdogThread);

            Assert.Equal(dataManager.GetHashCode(), managerSubject.GetHardwareManager().GetHashCode());
        }

        [Fact]
        public void GivenInitialTargets_WhenGetMonitoringQueue_ThenItReturnsInitialTargetsOnly()
        {
            ComputerMonitoringManagerModel managerSubject = GivenComputerManagerModel();
            List<MonitoringTarget> targets = managerSubject.GetMonitoringQueue();

            Assert.True(targets.Count() == 2);
            Assert.True(_expectedTargets.All(t => targets.Contains(t)));
        }

        [Fact]
        public void GivenRefreshCounter_WhenStopWatchElapsedRaises_ThenItRefreshMonitoring()
        {
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            DataManager dataManager = GivenValidDataManager();
            ProcessWatchDog watchdog = GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            IThread watchdogThread = GivenWatchdigThread();
            Mock<ITimer> timer = GivenRefreshTimerMock();
            ComputerMonitoringManagerModel managerSubject =
                new ComputerMonitoringManagerModel(eventManager, dataManager, watchdog, reporter, timer.Object, watchdogThread);
            managerSubject.HardwareValues.Clear();
            Assert.Empty(managerSubject.HardwareValues);

            timer.Raise(t => t.Elapsed += null, new EventArgs() as ElapsedEventArgs);

            Assert.True(managerSubject.HardwareValues.Count() == 2);
            Assert.True(_expectedHardwareInfo.All(hi => managerSubject.HardwareValues.Contains(hi)));
        }

        [Fact]
        public void GivenStopWatchElapsedRaised_WhenRefreshMonitoringHasErrors_ThenItInvokeMonitoringErrorOccuredEvent()
        {
            string exMessage = String.Empty;
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            DataManager dataManager = GivenInvalidDataManager();
            ProcessWatchDog watchdog = GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            IThread watchdogThread = GivenWatchdigThread();
            Mock<ITimer> timer = GivenRefreshTimerMock();
            ComputerMonitoringManagerModel managerSubject =
                new ComputerMonitoringManagerModel(eventManager, dataManager, watchdog, reporter, timer.Object, watchdogThread);
            managerSubject.OnMonitoringErrorOccured += new MonitoringErrorOccuredEventHandler(e => exMessage = e.Message);

            timer.Raise(t => t.Elapsed += null, new EventArgs() as ElapsedEventArgs);

            Assert.Null(managerSubject.HardwareValues);
            Assert.Equal(REFRESH_ERROR_MESSAGE, exMessage);

            managerSubject.OnMonitoringErrorOccured -= e => exMessage = e.Message;
        }

        [Fact]
        public void GivenActualProcessUnderWatch_WhenEventRaiseWithProcessTargetChangedToEmpty_ThenItChangesProcessTargets()
        {
            DataManager dataManager = GivenValidDataManager();
            ProcessWatchDog watchdog = GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            IThread watchdogThread = GivenWatchdigThread();
            ITimer timer = GivenRefreshTimer();

            Mock<IEventAggregator> eventManager = new Mock<IEventAggregator>();
            Mock<OnWatchdogTargetChangedEvent> watchdogTargetChangedEvent = new Mock<OnWatchdogTargetChangedEvent>();
            Mock<OnMonitoringTargetsChangedEvent> monTargetChangedEvent = new Mock<OnMonitoringTargetsChangedEvent>();
            Action<ObservableCollection<ProcessViewModel>> callback = null;
            watchdogTargetChangedEvent.Setup(
                p =>
                p.Subscribe(
                    It.IsAny<Action<ObservableCollection<ProcessViewModel>>>(),
                    It.IsAny<ThreadOption>(),
                    It.IsAny<bool>(),
                    It.IsAny<Predicate<ObservableCollection<ProcessViewModel>>>()))
                    .Callback<Action<ObservableCollection<ProcessViewModel>>, ThreadOption, bool, Predicate<ObservableCollection<ProcessViewModel>>>(
                    (e, t, b, a) => callback = e);
            SetupEventAggMockBehaviour(eventManager, watchdogTargetChangedEvent.Object, monTargetChangedEvent.Object);

            ComputerMonitoringManagerModel managerSubject =
                new ComputerMonitoringManagerModel(eventManager.Object, dataManager, watchdog, reporter, timer, watchdogThread);
            Assert.True(managerSubject.ProcessesUnderWatch.Count() == 1);

            callback.Invoke(new ObservableCollection<ProcessViewModel>());

            Assert.Empty(managerSubject.ProcessesUnderWatch);
        }

        [Fact]
        public void GivenActualMonTargets_WhenTargetChangeEventRaises_ThenItChangesTargetsAndRefreshInfo()
        {
            List <MonitoringTarget> changedTargets = new List<MonitoringTarget>() { MonitoringTarget.GPU_Memory_Clock, MonitoringTarget.GPU_Temp };
            DataManager dataManager = GivenValidDataManager();
            ProcessWatchDog watchdog = GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            IThread watchdogThread = GivenWatchdigThread();
            ITimer timer = GivenRefreshTimer();

            Mock<IEventAggregator> eventManager = new Mock<IEventAggregator>();
            Mock<OnWatchdogTargetChangedEvent> watchdogTargetChangedEvent = new Mock<OnWatchdogTargetChangedEvent>();
            Mock<OnMonitoringTargetsChangedEvent> monTargetChangedEvent = new Mock<OnMonitoringTargetsChangedEvent>();
            Action<List<MonitoringTarget>> callback = null;
            monTargetChangedEvent.Setup(
                p =>
                p.Subscribe(
                    It.IsAny<Action<List<MonitoringTarget>>>(),
                    It.IsAny<ThreadOption>(),
                    It.IsAny<bool>(),
                    It.IsAny<Predicate<List<MonitoringTarget>>>()))
                    .Callback<Action<List<MonitoringTarget>>, ThreadOption, bool, Predicate<List<MonitoringTarget>>>(
                    (e, t, b, a) => callback = e);
            SetupEventAggMockBehaviour(eventManager, watchdogTargetChangedEvent.Object, monTargetChangedEvent.Object);

            ComputerMonitoringManagerModel managerSubject =
                new ComputerMonitoringManagerModel(eventManager.Object, dataManager, watchdog, reporter, timer, watchdogThread);
            Assert.True(_expectedTargets.All( t => managerSubject.GetMonitoringQueue().Contains(t)));

            callback.Invoke(changedTargets);

            Assert.False(_expectedTargets.All(t => managerSubject.GetMonitoringQueue().Contains(t)));
            Assert.True(changedTargets.All(t => managerSubject.GetMonitoringQueue().Contains(t)));
        }

        #region Private methods
        

        private ProcessWatchDog GivenValidWatchDog()
        {
            Mock<ProcessWatchDog> watchDog = new Mock<ProcessWatchDog>();
            Process initialProcess = WatchDogTestHelper.GivenFirstRunningProcess();
            
            watchDog.Setup(w => w.GetInitialProcesses2Watch()).Returns(new List<string>() { PROCESS_NAME });
            watchDog.Setup(w => w.GetProcessesByName(It.IsAny<string>())).Returns(new List<Process>() { initialProcess });
            watchDog.Setup(w => w.IsProcessCurrentlyRunning(PROCESS_NAME)).Returns(true);
            watchDog.Setup(w => w.InitializeWatchdogForProcess(It.IsAny<PacketCaptureProcessInfo>())).Callback(() => _processWatchWasInitialized = true);
            watchDog.SetupAdd(w => w.PacketsExchangedEvent += It.IsAny<PacketsExchanged>()).Callback(() => _packetExchangedEventRegistered = true);
            watchDog.SetupRemove(w => w.PacketsExchangedEvent -= It.IsAny<PacketsExchanged>()).Callback(() => _packetExchangedEventRegistered = false);
            return watchDog.Object;
        }

        private ProcessWatchDog GivenInvalidWatchDog()
        {
            Mock<ProcessWatchDog> watchDog = new Mock<ProcessWatchDog>();

            watchDog.Setup(w => w.GetInitialProcesses2Watch()).Throws(new Exception());
            return watchDog.Object;
        }

        private IThread GivenWatchdigThread()
        {
            Mock<IThread> thread = new Mock<IThread>();
            thread.Setup(t => t.SetJob(It.IsAny<Action>())).Callback(() => _watchDogJobSetted = true);
            thread.Setup(t => t.Start()).Callback(() => _watchDogThreadStarted = true);
            thread.Setup(t => t.Abort()).Callback(() => _watchDogThreadStarted = false);
            return thread.Object;
        }

        private ITimer GivenRefreshTimer()
        {
            return GivenRefreshTimerMock().Object;
        }

        private Mock<ITimer> GivenRefreshTimerMock()
        {
            Mock<ITimer> timer = new Mock<ITimer>();
            timer.Setup(t => t.Start()).Callback(() => _timerStarted = true);
            timer.Setup(t => t.Stop()).Callback(() => _timerStarted = false);
            timer.Setup(t => t.Init(It.IsAny<int>())).Callback<int>(i => _refreshPolldelay = i);
            timer.SetupAdd(t => t.Elapsed += It.IsAny<ElapsedEventHandler>()).Callback(() => _processWatchWasInitialized = true);
            return timer;
        }

        private ComputerMonitoringManagerModel GivenComputerManagerModel()
        {
            _processWatchWasInitialized = false;
            _watchDogJobSetted = false;
            _watchDogThreadStarted = false;
            IEventAggregator eventManager = GivenEventAggregator();
            DataManager dataManager = GivenValidDataManager();
            ProcessWatchDog watchdog = GivenValidWatchDog();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            IThread watchdogThread = GivenWatchdigThread();
            ITimer timer = GivenRefreshTimer();

            ComputerMonitoringManagerModel managerSubject =
                new ComputerMonitoringManagerModel(eventManager, dataManager, watchdog, reporter, timer, watchdogThread);
            return managerSubject;
        }

        private DataManager GivenInvalidDataManager()
        {
            Mock<DataManager> dataManager = new Mock<DataManager>();
            dataManager.Setup(d => d.GetInitialTargets())
                       .Returns(_expectedTargets);
            dataManager.Setup(d => d.GetCalculatedValues(It.IsAny<ICollection<MonitoringTarget>>()))
                       .Throws(new Exception(REFRESH_ERROR_MESSAGE));

            return dataManager.Object;
        }

        private DataManager GivenValidDataManager()
        {
            Mock<DataManager> dataManager = new Mock<DataManager>();
            dataManager.Setup(d => d.GetInitialTargets())
                       .Returns(_expectedTargets);
            dataManager.Setup(d => d.GetCalculatedValues(It.IsAny<ICollection<MonitoringTarget>>()))
                       .Returns(_expectedHardwareInfo);

            return dataManager.Object;
        }

        private Reporter GivenReporter()
        {
            Mock<IMailClient> mailClient = new Mock<IMailClient>();
            Reporter reporter = new Reporter(mailClient.Object);

            return reporter;
        }

        private IEventAggregator GivenEventAggregator()
        {
            Mock<IEventAggregator> eventAgg = new Mock<IEventAggregator>();
            SetupEventAggMockBehaviour(eventAgg, new OnWatchdogTargetChangedEvent(), new OnMonitoringTargetsChangedEvent());
            return eventAgg.Object;
        }

        private void SetupEventAggMockBehaviour(Mock<IEventAggregator> eventManager, OnWatchdogTargetChangedEvent watchdogEvent, 
            OnMonitoringTargetsChangedEvent monitoringEvent)
        {
            eventManager.Setup(e => e.GetEvent<OnWatchdogTargetChangedEvent>()).Returns(watchdogEvent).Callback(() => _eventAggSubsribeCount += 1);
            eventManager.Setup(e => e.GetEvent<OnMonitoringTargetsChangedEvent>()).Returns(monitoringEvent).Callback(() => _eventAggSubsribeCount += 1);
        }

        #endregion
    }
}

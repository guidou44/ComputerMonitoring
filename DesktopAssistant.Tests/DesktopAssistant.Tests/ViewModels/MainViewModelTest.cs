using Common.Reports;
using Common.UI.WindowProperty;
using DesktopAssistantTests.ComputerResourcesMonitoringTests.Helper;
using DesktopAssistantTests.ComputerResourcesMonitoringTests.ViewModels.Fixtures;
using ComputerResourcesMonitoring.Models;
using DesktopAssistant.Models;
using DesktopAssistant.ViewModels;
using Hardware.Enums;
using Hardware.Models;
using Hardware;
using Moq;
using Prism.Events;
using ProcessMonitoring;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static ComputerResourcesMonitoring.Models.ComputerMonitoringManagerModel;

namespace DesktopAssistantTests.ComputerResourcesMonitoringTests.ViewModels
{
    public class MainViewModelTest
    {
        [Fact]
        public void GivenInstantiateViewModel_ThenItRegisterEventHandlers()
        {
            bool propertyChangedRegistered = false;
            bool ErrorOccuredRegistered = false;
            Mock<ComputerMonitoringManagerModel> appManager = GivenAppManager();
            appManager.SetupAdd(am => am.PropertyChanged += It.IsAny<PropertyChangedEventHandler>()).Callback(() => propertyChangedRegistered = true);
            appManager.SetupAdd(am => am.OnMonitoringErrorOccured += It.IsAny<MonitoringErrorOccuredEventHandler>()).Callback(() => ErrorOccuredRegistered = true);

            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, reporter);

            Assert.True(propertyChangedRegistered);
            Assert.True(ErrorOccuredRegistered);
        }

        [Fact]
        public void GivenAppManagerHardwareValuePropChanged_WhenEventRaise_ThenItUpdatesVmCorrespondingProperty()
        {
            Mock<ComputerMonitoringManagerModel> appManager = GivenAppManager();
            appManager.SetupGet(am => am.HardwareValues).Returns(ComputerMonitoringTestHelper.EXPECTED_VALUES);
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, reporter);

            appManager.Raise(am => am.PropertyChanged += null, new PropertyChangedEventArgs("HardwareValues"));

            Assert.Equal(ComputerMonitoringTestHelper.EXPECTED_VALUES, mainViewModel.HardwareValues);
        }

        [Fact]
        public void GivenAppManagerProcessUnderWatchPropChanged_WhenEventRaise_ThenItUpdatesVmCorrespondingProperty()
        {
            Mock<ComputerMonitoringManagerModel> appManager = GivenAppManager();
            appManager.SetupGet(am => am.ProcessesUnderWatch).Returns(ComputerMonitoringTestHelper.EXPECTED_PROCESS);
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, reporter);

            appManager.Raise(am => am.PropertyChanged += null, new PropertyChangedEventArgs("ProcessesUnderWatch"));

            Assert.Equal(ComputerMonitoringTestHelper.EXPECTED_PROCESS, mainViewModel.ProcessesUnderWatch);
        }

        [Fact]
        public void GivenErrorWithAppManager_WhenErrorOccuredEventIsRaised_ThenItHandleEventProper()
        {
            Exception managerError = null;
            Exception expectedException = new Exception("TEST");
            Mock<ComputerMonitoringManagerModel> appManager = GivenAppManager();
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            dialogService.Setup(ds => ds.ShowException(It.IsAny<Exception>())).Callback<Exception>(e => managerError = e);
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, reporter);

            appManager.Raise(am => am.OnMonitoringErrorOccured += null, expectedException);

            Assert.Equal(expectedException.GetHashCode(), managerError.GetHashCode());
            Assert.Equal(expectedException.Message, managerError.Message);
        }

        [Fact]
        public void GivenVisibleApp_WhenHideApp_ThenHideAppCommandBehavesProper()
        {
            MainViewModel mainVmSubject = GivenMainViewModelTestSubject();

            Assert.False(mainVmSubject.ShowApplicationCommand.CanExecute(null));
            Assert.True(mainVmSubject.HideApplicationCommand.CanExecute(null));
            mainVmSubject.HideApplicationCommand.Execute(null);
            Assert.False(mainVmSubject.HideApplicationCommand.CanExecute(null));
            Assert.True(mainVmSubject.ShowApplicationCommand.CanExecute(null));
        }

        [Fact]
        public void GivenVisibleApp_WhenShowApp_ThenShowAppCommandBehavesProper()
        {
            MainViewModel mainVmSubject = GivenMainViewModelTestSubject();

            mainVmSubject.HideApplicationCommand.Execute(null);

            Assert.True(mainVmSubject.ShowApplicationCommand.CanExecute(null));
            Assert.False(mainVmSubject.HideApplicationCommand.CanExecute(null));
            mainVmSubject.ShowApplicationCommand.Execute(null);
            Assert.True(mainVmSubject.HideApplicationCommand.CanExecute(null));
            Assert.False(mainVmSubject.ShowApplicationCommand.CanExecute(null));
        }

        [Fact]
        public void GivenRunningApp_WhenKillAppCommand_ThenItKillsAppProper()
        {
            bool propertyChangedRegistered = true;
            bool ErrorOccuredRegistered = true;
            bool appDisposed = false;
            bool windowClosed = false;
            Mock<IClosable> closable = new Mock<IClosable>();
            closable.Setup(c => c.Close()).Callback(() => windowClosed = true);
            Mock<ComputerMonitoringManagerModel> appManager = GivenAppManager();
            appManager.SetupRemove(am => am.PropertyChanged -= It.IsAny<PropertyChangedEventHandler>()).Callback(() => propertyChangedRegistered = false);
            appManager.SetupRemove(am => am.OnMonitoringErrorOccured -= It.IsAny<MonitoringErrorOccuredEventHandler>()).Callback(() => ErrorOccuredRegistered = false);
            appManager.Setup(am => am.Dispose()).Callback(() => appDisposed = true);
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, reporter);

            mainViewModel.KillAppCommand.Execute(closable.Object);

            Assert.True(windowClosed);
            Assert.True(appDisposed);
            Assert.False(propertyChangedRegistered);
            Assert.False(ErrorOccuredRegistered);
        }

        [Fact]
        public void GivenOpenSettingsDialogCommand_WhenInvoked_ThenItInstantiateSettingsDialogProper()
        {
            Mock<ComputerMonitoringManagerModel> appManager = GivenAppManager();
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            dialogService.Setup(ds => ds.Instantiate(It.IsAny<SettingsDialogViewModel>())).Verifiable();
            dialogService.Setup(ds => ds.ShowDialog(It.IsAny<SettingsDialogViewModel>())).Verifiable();
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, reporter);
            mainViewModel.ProcessesUnderWatch = ComputerMonitoringTestHelper.EXPECTED_PROCESS;


            mainViewModel.OpenSettingsWindowCommand.Execute(null);

            dialogService.Verify(ds => ds.Instantiate(It.IsAny<SettingsDialogViewModel>()), Times.Once);
            dialogService.Verify(ds => ds.ShowDialog(It.IsAny<SettingsDialogViewModel>()), Times.Once);
        }

        [Fact]
        public void GivenOpenSettingsDialogCommand_WhenInvokedAndErrorOccurs_ThenItManageErrorProper()
        {
            Exception managerError = null;
            Exception expectedException = new Exception("TEST");
            Mock<ComputerMonitoringManagerModel> appManager = GivenAppManager();
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            dialogService.Setup(ds => ds.ShowException(It.IsAny<Exception>())).Callback<Exception>(e => managerError = e);
            dialogService.Setup(ds => ds.Instantiate(It.IsAny<SettingsDialogViewModel>())).Verifiable();
            dialogService.Setup(ds => ds.ShowDialog(It.IsAny<SettingsDialogViewModel>())).Throws(expectedException);
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, reporter);
            mainViewModel.ProcessesUnderWatch = ComputerMonitoringTestHelper.EXPECTED_PROCESS;

            mainViewModel.OpenSettingsWindowCommand.Execute(null);

            dialogService.Verify(ds => ds.Instantiate(It.IsAny<SettingsDialogViewModel>()), Times.Once);
            dialogService.Verify(ds => ds.ShowDialog(It.IsAny<SettingsDialogViewModel>()), Times.Once);
            Assert.Equal(expectedException.GetHashCode(), managerError.GetHashCode());
            Assert.Equal(expectedException.Message, managerError.Message);
        }

        [Fact]
        public void GivenWindow_WhenResizeCommandExecutes_ThenItResizeWindowProper()
        {
            MainViewModel mainVmSubject = GivenMainViewModelTestSubject();
            WindowFixture window = new WindowFixture();
            window.ActualHeight = 50;
            window.ActualWidth = 100;
            double desktopWorkingAreaRight = 300.0;
            double desktopWorkingAreaBottom = 200.0;
            object[] commandParams = new object[] { $"%,%,{desktopWorkingAreaRight},{desktopWorkingAreaBottom}", window };

            mainVmSubject.ResizeWindowCommand.Execute(commandParams);

            Assert.Equal(desktopWorkingAreaRight - window.ActualWidth , window.Left);
            Assert.Equal(desktopWorkingAreaBottom - window.ActualHeight, window.Top);
        }

        [Fact]
        public void GivenNotOnTopWindow_WhenInvokeSetOnTopCommand_ThenItSetsWindowOnTop()
        {
            MainViewModel mainVmSubject = GivenMainViewModelTestSubject();
            WindowFixture window = new WindowFixture() { Topmost = false };

            mainVmSubject.SetWindowOnTopCommand.Execute(window);

            Assert.True(window.Topmost);
        }

        #region Private methods

        private Mock<ComputerMonitoringManagerModel> GivenAppManager()
        {
            ProcessWatchDog watchdog = ComputerMonitoringTestHelper.GivenValidWatchDog();            
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            Mock<ITimer> timer = new Mock<ITimer>();
            Mock<IThread> thread = new Mock<IThread>();

            Mock<DataManager> dataManager = ComputerMonitoringTestHelper.GivenDataManagerMock();

            Mock<ComputerMonitoringManagerModel> appManager = 
                new Mock<ComputerMonitoringManagerModel>(eventManager, dataManager.Object, watchdog, reporter, timer.Object, thread.Object);
            appManager.Setup(am => am.GetMonitoringQueue()).Returns(ComputerMonitoringTestHelper.EXPECTED_TARGETS);
            appManager.Setup(am => am.GetHardwareManager()).Returns(dataManager.Object);
            appManager.Setup(am => am.GetWatchDog()).Returns(watchdog);
            return appManager;
        }

        private MainViewModel GivenMainViewModelTestSubject()
        {
            Mock<ComputerMonitoringManagerModel> appManager = GivenAppManager();
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, reporter);
            return mainViewModel;
        }

        #endregion
    }
}

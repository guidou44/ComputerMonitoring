using System;
using System.Collections.Generic;
using Common.Reports;
using Common.UI.WindowProperty;
using DesktopAssistant.BL;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.BL.Persistence;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.Tests.DesktopAssistant.Tests.Helper;
using DesktopAssistant.Tests.DesktopAssistant.Tests.ViewModels.Fixtures;
using DesktopAssistant.UI;
using DesktopAssistant.ViewModels;
using Hardware.Models;
using Moq;
using Prism.Events;
using ProcessMonitoring;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.Tests.ViewModels
{
    public class MainViewModelTest
    {
        private Mock<IAppManager> GivenAppManager()
        {
            var targetMocks = new List<MonitoringTarget>() {MonitoringTarget.CPU_Load};
            Mock<IAppManager> appManager = 
                new Mock<IAppManager>();
            appManager.Setup(am => am.GetMonitoringQueue()).Returns(ComputerMonitoringTestHelper.EXPECTED_TARGETS);
            appManager.Setup(am => am.GetMonitoringQueue()).Returns(targetMocks);
            appManager.Setup(am => am.GetCalculatedValue(It.IsAny<MonitoringTarget>()))
                .Returns(new HardwareInformation());
            appManager.Setup(am => am.GetAllTargets()).Returns(targetMocks);
            
            return appManager;
        }

        private MainViewModel GivenMainViewModelTestSubject()
        {
            Mock<IAppManager> appManager = GivenAppManager();
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            IRepository repository = ComputerMonitoringTestHelper.GivenRepository();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            Mock<IUiSettings> ui = new Mock<IUiSettings>();
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, repository, ui.Object);
            return mainViewModel;
        }

        [Fact]
        public void GivenNotOnTopWindow_WhenInvokeSetOnTopCommand_ThenItSetsWindowOnTop()
        {
            MainViewModel mainVmSubject = GivenMainViewModelTestSubject();
            WindowFixture window = new WindowFixture() { Topmost = false };

            mainVmSubject.SetWindowOnTopCommand.Execute(window);

            Assert.True(window.Topmost);
        }

        [Fact]
        public void GivenOpenHardwareSettingsDialogCommand_WhenInvoked_ThenItInstantiateSettingsDialogProper()
        {
            Mock<IAppManager> appManager = GivenAppManager();
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            IRepository repository = ComputerMonitoringTestHelper.GivenRepository();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            dialogService.Setup(ds => ds.Instantiate(It.IsAny<HardwareSettingsViewModel>())).Verifiable();
            dialogService.Setup(ds => ds.ShowDialog(It.IsAny<HardwareSettingsViewModel>())).Verifiable();
            Mock<IUiSettings> ui = new Mock<IUiSettings>();
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, repository, ui.Object);


            mainViewModel.OpenHardwareSettingsWindowCommand.Execute(null);

            dialogService.Verify(ds => ds.Instantiate(It.IsAny<HardwareSettingsViewModel>()), Times.Once);
            dialogService.Verify(ds => ds.ShowDialog(It.IsAny<HardwareSettingsViewModel>()), Times.Once);
        }

        [Fact]
        public void GivenOpenSettingsDialogCommand_WhenInvokedAndErrorOccurs_ThenItManageErrorProper()
        {
            Exception managerError = null;
            Exception expectedException = new Exception("TEST");
            Mock<IAppManager> appManager = GivenAppManager();
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            IRepository repository = ComputerMonitoringTestHelper.GivenRepository();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            dialogService.Setup(ds => ds.ShowException(It.IsAny<Exception>())).Callback<Exception>(e => managerError = e);
            dialogService.Setup(ds => ds.Instantiate(It.IsAny<HardwareSettingsViewModel>())).Verifiable();
            dialogService.Setup(ds => ds.ShowDialog(It.IsAny<HardwareSettingsViewModel>())).Throws(expectedException);
            Mock<IUiSettings> ui = new Mock<IUiSettings>();
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, repository, ui.Object);

            mainViewModel.OpenHardwareSettingsWindowCommand.Execute(null);

            dialogService.Verify(ds => ds.Instantiate(It.IsAny<HardwareSettingsViewModel>()), Times.Once);
            dialogService.Verify(ds => ds.ShowDialog(It.IsAny<HardwareSettingsViewModel>()), Times.Once);
            Assert.Equal(expectedException.GetHashCode(), managerError.GetHashCode());
            Assert.Equal(expectedException.Message, managerError.Message);
        }

        [Fact]
        public void GivenRunningApp_WhenKillAppCommand_ThenItKillsAppProper()
        {
            bool appDisposed = false;
            bool windowClosed = false;
            Mock<IClosable> closable = new Mock<IClosable>();
            closable.Setup(c => c.Close()).Callback(() => windowClosed = true);
            Mock<IAppManager> appManager = GivenAppManager();
            appManager.Setup(am => am.Dispose()).Callback(() => appDisposed = true);
            IEventAggregator eventManager = ComputerMonitoringTestHelper.GivenEventAggregator();
            IRepository repository = ComputerMonitoringTestHelper.GivenRepository();
            Mock<IDialogService> dialogService = ComputerMonitoringTestHelper.GivenDialogServiceMock();
            Mock<IUiSettings> ui = new Mock<IUiSettings>();
            MainViewModel mainViewModel = new MainViewModel(dialogService.Object, appManager.Object, eventManager, repository, ui.Object);

            mainViewModel.KillAppCommand.Execute(closable.Object);

            Assert.True(windowClosed);
            Assert.True(appDisposed);
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
    }
}

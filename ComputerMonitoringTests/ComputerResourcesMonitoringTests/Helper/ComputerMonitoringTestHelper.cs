using Common.MailClient;
using Common.Reports;
using Common.UI.WindowProperty;
using ComputerMonitoringTests.ProcessMonitoringTests.Helpers;
using ComputerRessourcesMonitoring.Events;
using ComputerRessourcesMonitoring.ViewModels;
using HardwareAccess.Enums;
using HardwareAccess.Models;
using HardwareManipulation;
using Moq;
using Prism.Events;
using ProcessMonitoring;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ComputerMonitoringTests.ComputerResourcesMonitoringTests.Helper
{
    public static class ComputerMonitoringTestHelper
    {
        public static List<MonitoringTarget> EXPECTED_TARGETS = new List<MonitoringTarget>() { MonitoringTarget.CPU_Load,
                                                                                               MonitoringTarget.CPU_Temp,
                                                                                               MonitoringTarget.RAM_Usage,
                                                                                               MonitoringTarget.GPU_Clock,
                                                                                               MonitoringTarget.FAN_Speed
        };

        public static List<HardwareInformation> EXPECTED_VALUES = new List<HardwareInformation>()
        {
            new HardwareInformation() { MainValue = 2.0, ShortName = "CPU_TEST", UnitSymbol = "%"},
            new HardwareInformation() { MainValue = 3.0, ShortName = "TEMP_TEST", UnitSymbol = "°C"},
            new HardwareInformation() { MainValue = 5.0, ShortName = "RAM_TEST", UnitSymbol = "%"},
        };

        public static ObservableCollection<ProcessViewModel> EXPECTED_PROCESS = new ObservableCollection<ProcessViewModel>()
        {
            new ProcessViewModel(false, "TEST1", GivenReporter()) { Process = WatchDogTestHelper.GivenFirstRunningProcess()},
            new ProcessViewModel(false, "TEST2", GivenReporter()) { Process = WatchDogTestHelper.GivenFirstRunningProcess()},
            new ProcessViewModel(false, "TEST3", GivenReporter()) { Process = WatchDogTestHelper.GivenFirstRunningProcess()},
        };

        public static HardwareInformation MotherboardMake = new HardwareInformation() { MainValue = "MotherBoardTest", ShortName = "Motherboard-make" };
        public static HardwareInformation CpuMake = new HardwareInformation() { MainValue = "CpuTest", ShortName = "CPU-make" };
        public static HardwareInformation GpuMake = new HardwareInformation() { MainValue = "GpuTest", ShortName = "GPU-make" };

        public static int EVENT_AGG_SUBSCRIBTION_COUNT = 0;

        public static Reporter GivenReporter()
        {
            Mock<Reporter> reporter = new Mock<Reporter>();
            return reporter.Object;
        }

        public static IEventAggregator GivenEventAggregator()
        {
            Mock<IEventAggregator> eventAgg = new Mock<IEventAggregator>();
            SetupEventAggMockBehaviour(eventAgg, new OnWatchdogTargetChangedEvent(), new OnMonitoringTargetsChangedEvent());

            return eventAgg.Object;
        }

        public static void GivenDeletedDirectoryIfAlreadyExists(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, recursive: true);
        }

        public static Mock<IMailClient> ProvideSmtpClient()
        {
            Mock<IMailClient> smtpClient = new Mock<IMailClient>();
            smtpClient.Setup(s => s.Send(It.IsAny<MailMessage>())).Verifiable();
            smtpClient.SetupGet(e => e.Credentials).Returns(new System.Net.NetworkCredential("TEST@gmail.com", "Password"));
            return smtpClient;
        }

        public static Mock<IDialogService> GivenDialogServiceMock()
        {
            Mock<IDialogService> dialogService = new Mock<IDialogService>();
            dialogService.Setup(ds => ds.ShowDialog(It.IsAny<SettingsDialogViewModel>())).Returns(true);
            dialogService.Setup(ds => ds.Instantiate(It.IsAny<SettingsDialogViewModel>())).Verifiable();
            return dialogService;
        }

        public static ProcessWatchDog GivenValidWatchDog()
        {
            Mock<ProcessWatchDog> watchDog = new Mock<ProcessWatchDog>();
            return watchDog.Object;
        }

        public static Mock<DataManager> GivenDataManagerMock()
        {
            Mock<DataManager> dataManager = new Mock<DataManager>();
            dataManager.Setup(dm => dm.GetCalculatedValue(MonitoringTarget.Mother_Board_Make))
                       .Returns(MotherboardMake);
            dataManager.Setup(dm => dm.GetCalculatedValue(MonitoringTarget.CPU_Make))
                       .Returns(CpuMake);
            dataManager.Setup(dm => dm.GetCalculatedValue(MonitoringTarget.GPU_Make))
                       .Returns(GpuMake);
            dataManager.Setup(dm => dm.GetAllTargets()).Returns(EXPECTED_TARGETS);
            dataManager.Setup(dm => dm.GetLocalTargets()).Returns(EXPECTED_TARGETS);
            dataManager.Setup(dm => dm.IsRemoteMonitoringEnabled()).Returns(false);

            return dataManager;
        }

        public static void SetupEventAggMockBehaviour(Mock<IEventAggregator> eventManager, OnWatchdogTargetChangedEvent watchdogEvent,
                                                      OnMonitoringTargetsChangedEvent monitoringEvent)
        {
            eventManager.Setup(e => e.GetEvent<OnWatchdogTargetChangedEvent>()).Returns(watchdogEvent).Callback(() => EVENT_AGG_SUBSCRIBTION_COUNT += 1);
            eventManager.Setup(e => e.GetEvent<OnMonitoringTargetsChangedEvent>()).Returns(monitoringEvent).Callback(() => EVENT_AGG_SUBSCRIBTION_COUNT += 1);
        }

    }
}

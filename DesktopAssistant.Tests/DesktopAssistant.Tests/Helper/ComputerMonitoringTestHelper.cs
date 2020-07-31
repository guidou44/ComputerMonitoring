using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using Common.MailClient;
using Common.Reports;
using Common.UI.WindowProperty;
using DesktopAssistant.BL.Events;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.Tests.ProcessMonitoring.Tests.Helpers;
using DesktopAssistant.ViewModels;
using Hardware;
using Hardware.Models;
using Moq;
using Prism.Events;
using ProcessMonitoring;
using ProcessMonitoring.Models;

namespace DesktopAssistant.Tests.DesktopAssistant.Tests.Helper
{
    public static class ComputerMonitoringTestHelper
    {
        public static List<MonitoringTarget> EXPECTED_TARGETS = new List<MonitoringTarget>() { MonitoringTarget.CPU_Load,
                                                                                               MonitoringTarget.CPU_Temp,
                                                                                               MonitoringTarget.RAM_Usage,
                                                                                               MonitoringTarget.GPU_Clock,
                                                                                               MonitoringTarget.FAN_Speed
        };

        public static List<IHardwareInfo> EXPECTED_VALUES = new List<IHardwareInfo>()
        {
            new HardwareInformation() { MainValue = 2.0, ShortName = "CPU_TEST", UnitSymbol = "%"},
            new HardwareInformation() { MainValue = 3.0, ShortName = "TEMP_TEST", UnitSymbol = "°C"},
            new HardwareInformation() { MainValue = 5.0, ShortName = "RAM_TEST", UnitSymbol = "%"},
        };

        public static List<IProcessWatch> EXPECTED_PROCESS = new List<IProcessWatch>()
        {
            new ProcessWatch("TEST1", false),
            new ProcessWatch("TEST2",false),
            new ProcessWatch("TEST3", false),
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
            smtpClient.SetupGet(e => e.Credentials).Returns(new NetworkCredential("TEST@gmail.com", "Password"));
            return smtpClient;
        }

        public static Mock<IDialogService> GivenDialogServiceMock()
        {
            Mock<IDialogService> dialogService = new Mock<IDialogService>();
            dialogService.Setup(ds => ds.ShowDialog(It.IsAny<HardwareSettingsViewModel>())).Returns(true);
            dialogService.Setup(ds => ds.Instantiate(It.IsAny<HardwareSettingsViewModel>())).Verifiable();
            return dialogService;
        }

        public static Mock<IProcessWatcher> GivenValidWatchDog()
        {
            Mock<IProcessWatcher> watchDog = new Mock<IProcessWatcher>();
            return watchDog;
        }

        public static Mock<IHardwareManager> GivenDataManagerMock()
        {
            Mock<IHardwareManager> dataManager = new Mock<IHardwareManager>();
            dataManager.Setup(dm => dm.GetCalculatedValue(MonitoringTarget.Mother_Board_Make))
                       .Returns(MotherboardMake);
            dataManager.Setup(dm => dm.GetCalculatedValue(MonitoringTarget.CPU_Make))
                       .Returns(CpuMake);
            dataManager.Setup(dm => dm.GetCalculatedValue(MonitoringTarget.GPU_Make))
                       .Returns(GpuMake);
            dataManager.Setup(dm => dm.GetAllTargets()).Returns(EXPECTED_TARGETS);

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

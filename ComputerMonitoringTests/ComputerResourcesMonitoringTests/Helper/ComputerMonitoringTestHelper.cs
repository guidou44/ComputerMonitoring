using Common.Reports;
using ComputerRessourcesMonitoring.Events;
using HardwareAccess.Enums;
using HardwareAccess.Models;
using HardwareManipulation;
using Moq;
using Prism.Events;
using ProcessMonitoring;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerMonitoringTests.ComputerResourcesMonitoringTests.Helper
{
    public static class ComputerMonitoringTestHelper
    {
        public static Reporter GivenReporter()
        {
            Mock<Reporter> reporter = new Mock<Reporter>();
            return reporter.Object;
        }

        public static IEventAggregator GivenEventAggregator()
        {
            Mock<IEventAggregator> eventAgg = new Mock<IEventAggregator>();
            eventAgg.Setup(e => e.GetEvent<OnWatchdogTargetChangedEvent>()).Returns(new OnWatchdogTargetChangedEvent());
            eventAgg.Setup(e => e.GetEvent<OnMonitoringTargetsChangedEvent>()).Returns(new OnMonitoringTargetsChangedEvent());

            return eventAgg.Object;
        }

        public static void GivenDeletedDirectoryIfAlreadyExists(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, recursive: true);
        }
    }
}

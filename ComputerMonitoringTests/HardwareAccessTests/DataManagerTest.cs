using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareAccess.Enums;
using HardwareManipulation;
using Xunit;

namespace ComputerMonitoringTests.HardwareAccessTests
{
    public class DataManagerTest
    {
        [Fact]
        public void GivenConfigFile_WhenAskingForInitialTarget_ThenItReturnsProperTargets()
        {
            const string alternateConfigFile = @"..\..\Configuration\MonitoringConfiguration.cfg";
            DataManager managerSubject = new DataManager(alternateConfigFile);
            IEnumerable<MonitoringTarget> initialTarget = managerSubject.GetInitialTargets();
            Assert.True(initialTarget.Count() == 3);
            Assert.Contains(MonitoringTarget.CPU_Load, initialTarget);
            Assert.Contains(MonitoringTarget.Primary_HDD_Used_Space, initialTarget);
        }
    }
}

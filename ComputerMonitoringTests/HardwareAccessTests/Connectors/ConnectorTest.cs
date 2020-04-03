using HardwareAccess.Connectors;
using HardwareAccess.Enums;
using HardwareAccess.Helpers;
using HardwareAccess.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ComputerMonitoringTests.HardwareAccessTests.Connectors
{
    public class ConnectorTest
    {

        [Theory]
        [MemberData(nameof(GetConnectorParameters), parameters:3)]
        public static async void GivenValidConnector_WhenGettingValueFromAcceptedResource_ThenItReturnsValue(ConnectorBase connectorSubject,
            List<MonitoringTarget> targets)
        {
            HardwareInformation[] results = await Task.WhenAll(targets.Select(async target =>
            {
                return connectorSubject.GetValue(target);
            }));
            Assert.All(results, r => Assert.NotNull(r.MainValue));
        }

        public static IEnumerable<object[]> GetConnectorParameters(int numTest)
        {
            List<object[]> parameters = new List<object[]>()
            {
                new object[] { new WMI_Connector(new WmiHelper(), new PerformanceCounter("Processor", "% Idle Time", "_Total")), new List<MonitoringTarget>() { MonitoringTarget.RAM_Usage,
                                                                                   MonitoringTarget.CPU_Load,
                                                                                   MonitoringTarget.CPU_Make} },

                new object[] { new SystemIO_Connector(), new List<MonitoringTarget>() { MonitoringTarget.Primary_HDD_Used_Space} },

            };

            return parameters.Take(numTest);
        }

    }
}

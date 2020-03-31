using HardwareAccess.Connectors;
using HardwareAccess.Enums;
using HardwareAccess.Models;
using System;
using System.Collections.Generic;
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
        public static async void GivenValidController_WhenGettingValueFromAcceptedResource_ThenItReturnsValue(ConnectorBase connectorSubject,
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
                new object[] { new WMI_Connector(), new List<MonitoringTarget>() { MonitoringTarget.RAM_Usage,
                                                                                   MonitoringTarget.CPU_Clock_Speed,
                                                                                   MonitoringTarget.CPU_Load,
                                                                                   MonitoringTarget.CPU_Thread_Count,
                                                                                   MonitoringTarget.CPU_Make} },

                new object[] { new SystemIO_Connector(), new List<MonitoringTarget>() { MonitoringTarget.Primary_HDD_Used_Space,
                                                                                        MonitoringTarget.Secondary_HDD_Used_Space } },

            };

            return parameters.Take(numTest);
        }

    }
}

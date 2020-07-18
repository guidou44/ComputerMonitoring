using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardware.Connectors;
using Hardware.Enums;
using Hardware.Helpers;
using Hardware.Components;
using Moq;

namespace DesktopAssistantTests.Hardware.Connectors
{
    public class WMI_ConnectorTest : ConnectorBaseTest
    {
        const double EXPECTED_DOUBLE = 69.0;
        const uint EXPECTED_UINT = 96;
        const string EXPECTED_STRING = "SOME_STRING";
        const double EXPECTED_DOUBLE_WITH_SCOPE = 55.0;
        const double EXPECTED_MEMORY_SIZE = 50.0;
        const double EXPECTED_TOTAL_MEMORY_SIZE = 100.0;
        const float EXPECTED_FLOAT = 12.2f;

        protected override KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>> ProvideConnectorTargetsAndExpected()
        {
            WMI_Connector connector = GetConnector();

            IDictionary<MonitoringTarget, object> targetAndExpected = new Dictionary<MonitoringTarget, object>()
            {
                { MonitoringTarget.CPU_Core_Count, (double) EXPECTED_UINT },
                { MonitoringTarget.CPU_Clock_Speed, (double) (EXPECTED_UINT * 001f)/1000 },
                { MonitoringTarget.CPU_Make, EXPECTED_STRING },
                { MonitoringTarget.CPU_Temp, (EXPECTED_DOUBLE_WITH_SCOPE - 2732) / 10.0 },
                { MonitoringTarget.CPU_Thread_Count, (double) EXPECTED_UINT },
                { MonitoringTarget.CPU_Load, Math.Round(100.0 - EXPECTED_FLOAT, 2) },
                { MonitoringTarget.RAM_Usage, 100 * Math.Round(((EXPECTED_TOTAL_MEMORY_SIZE - EXPECTED_MEMORY_SIZE) / EXPECTED_TOTAL_MEMORY_SIZE), 2) },
            };

            return new KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>>(connector, targetAndExpected);
        }

        protected override KeyValuePair<ConnectorBase, MonitoringTarget> ProvideConnectorWithTargetThatThrows()
        {
            WMI_Connector connector = GetConnector();
            return new KeyValuePair<ConnectorBase, MonitoringTarget>(connector, MonitoringTarget.GPU_Memory_Controller);
        }

        private WMI_Connector GetConnector()
        {
            Mock<WmiHelper> wmiHelper = new Mock<WmiHelper>();
            wmiHelper.Setup(wmi => wmi.GetWmiValue<double>(It.IsAny<string>(), It.IsAny<string>())).Returns(EXPECTED_DOUBLE);
            wmiHelper.Setup(wmi => wmi.GetWmiValue<uint>(It.IsAny<string>(), It.IsAny<string>())).Returns(EXPECTED_UINT);
            wmiHelper.Setup(wmi => wmi.GetWmiValue<string>(It.IsAny<string>(), It.IsAny<string>())).Returns(EXPECTED_STRING);
            wmiHelper.Setup(wmi => wmi.GetWmiValue<double>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(EXPECTED_DOUBLE_WITH_SCOPE);
            wmiHelper.Setup(wmi => wmi.GetWmiValue<double>("Win32_OperatingSystem", "TotalVisibleMemorySize")).Returns(EXPECTED_TOTAL_MEMORY_SIZE);
            wmiHelper.Setup(wmi => wmi.GetWmiValue<double>("Win32_OperatingSystem", "FreePhysicalMemory")).Returns(EXPECTED_MEMORY_SIZE);

            Mock<IPerformanceCounter> perfCounter = new Mock<IPerformanceCounter>();
            perfCounter.Setup(p => p.NextValue()).Returns(EXPECTED_FLOAT);

            return new WMI_Connector(wmiHelper.Object, perfCounter.Object);
        }
    }
}

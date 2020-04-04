using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareAccess.Connectors;
using HardwareAccess.Enums;
using HardwareManipulation.Components;
using HardwareManipulation.Wrappers;
using Moq;
using Xunit;

namespace ComputerMonitoringTests.HardwareAccessTests.Connectors
{
    public class NVDIA_API_ConnectorTest : ConnectorBaseTest
    {
        const string GPU_MAKE = "N_V_D_I_A";
        const double GPU_LOAD = 100.0;
        const double GPU_TEMP = 77.0;

        [Fact]
        public void GivenWrapper_WhenInstantiateConnector_ThenItCallsWrapperInitialize()
        {
            Mock<INvidiaComponent> component = new Mock<INvidiaComponent>();
            component.Setup(c => c.Initialize()).Verifiable();
            NVDIA_API_Connector connector = new NVDIA_API_Connector(component.Object);

            component.Verify(c => c.Initialize(), Times.Once);
        }

        protected override KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>> ProvideConnectorTargetsAndExpected()
        {
            NVDIA_API_Connector connector = GetConnector();
            IDictionary<MonitoringTarget, object> targetsAndExpected = new Dictionary<MonitoringTarget, object>()
            {
                { MonitoringTarget.GPU_Make, GPU_MAKE},
                { MonitoringTarget.GPU_Load, GPU_LOAD},
                { MonitoringTarget.GPU_Temp, GPU_TEMP}
            };
            return new KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>>(connector, targetsAndExpected);
        }

        protected override KeyValuePair<ConnectorBase, MonitoringTarget> ProvideConnectorWithTargetThatThrows()
        {
            NVDIA_API_Connector connector = GetConnector();
            return new KeyValuePair<ConnectorBase, MonitoringTarget>(connector, MonitoringTarget.Mother_Board_Make);
        }

        public NVDIA_API_Connector GetConnector()
        {
            Mock<NvidiaGpuWrapper> gpu = new Mock<NvidiaGpuWrapper>();
            Mock<INvidiaComponent> component = new Mock<INvidiaComponent>();

            gpu.SetupGet(g => g.FullName).Returns(GPU_MAKE);
            gpu.SetupGet(g => g.Percentage).Returns(GPU_LOAD);
            gpu.SetupGet(g => g.CurrentTemperature).Returns(GPU_TEMP);

            component.Setup(c => c.GetPhysicalGPUs()).Returns(new List<NvidiaGpuWrapper>() { gpu.Object });

            return new NVDIA_API_Connector(component.Object);
        }
    }
}

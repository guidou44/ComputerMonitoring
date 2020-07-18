using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardware.Connectors;
using Hardware.Enums;
using Hardware.Components;
using Moq;

namespace DesktopAssistantTests.Hardware.Connectors
{
    public class SystemIO_ConnectorTest : ConnectorBaseTest
    {
        private const long TOTAL_FREE_SPACE_LOCAL1 = 50;
        private const long TOTAL_SPACE_LOCAL1 = 100;
        private const long TOTAL_FREE_SPACE_LOCAL2 = 60;
        private const long TOTAL_SPACE_LOCAL2 = 200;
        private const long TOTAL_FREE_SPACE_NETWORK1 = 2;
        private const long TOTAL_SPACE_NETWORK1 = 50;
        private const long TOTAL_FREE_SPACE_NETWORK2 = 17;
        private const long TOTAL_SPACE_NETWORK2 = 300;
        protected override KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>> ProvideConnectorTargetsAndExpected()
        {
            SystemIO_Connector connector = GetConnector();
            IDictionary<MonitoringTarget, object> targetsAndExpected = new Dictionary<MonitoringTarget, object>()
            {
                { MonitoringTarget.Primary_HDD_Used_Space, 100 * (1 - Math.Round((((double )TOTAL_FREE_SPACE_LOCAL1) / ((double)TOTAL_SPACE_LOCAL1)), 2))},
                { MonitoringTarget.Secondary_HDD_Used_Space, 100 * (1 - Math.Round((((double )TOTAL_FREE_SPACE_LOCAL2) / ((double)TOTAL_SPACE_LOCAL2)), 2))},
                { MonitoringTarget.Primary_Network_HDD_Used_Space, 100 * (1 - Math.Round((((double )TOTAL_FREE_SPACE_NETWORK1) / ((double)TOTAL_SPACE_NETWORK1)), 2))},
                { MonitoringTarget.Secondary_Network_HDD_Used_Space, 100 * (1 - Math.Round((((double )TOTAL_FREE_SPACE_NETWORK2) / ((double)TOTAL_SPACE_NETWORK2)), 2))},
            };

            return new KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>>(connector, targetsAndExpected);
        }

        protected override KeyValuePair<ConnectorBase, MonitoringTarget> ProvideConnectorWithTargetThatThrows()
        {
            SystemIO_Connector connector = GetConnector();
            return new KeyValuePair<ConnectorBase, MonitoringTarget>(connector, MonitoringTarget.GPU_Clock);
        }

        private SystemIO_Connector GetConnector()
        {
            Mock<IDriveInfoProvider> provider = new Mock<IDriveInfoProvider>();

            Mock<IDriveInfo> localDrive1 = new Mock<IDriveInfo>();
            Mock<IDriveInfo> localDrive2 = new Mock<IDriveInfo>();
            Mock<IDriveInfo> networkDrive1 = new Mock<IDriveInfo>();
            Mock<IDriveInfo> networkDrive2 = new Mock<IDriveInfo>();

            localDrive1.SetupGet(d => d.TotalFreeSpace).Returns(TOTAL_FREE_SPACE_LOCAL1);
            localDrive1.SetupGet(d => d.TotalSize).Returns(TOTAL_SPACE_LOCAL1);
            localDrive2.SetupGet(d => d.TotalFreeSpace).Returns(TOTAL_FREE_SPACE_LOCAL2);
            localDrive2.SetupGet(d => d.TotalSize).Returns(TOTAL_SPACE_LOCAL2);
            networkDrive1.SetupGet(d => d.TotalFreeSpace).Returns(TOTAL_FREE_SPACE_NETWORK1);
            networkDrive1.SetupGet(d => d.TotalSize).Returns(TOTAL_SPACE_NETWORK1);
            networkDrive2.SetupGet(d => d.TotalFreeSpace).Returns(TOTAL_FREE_SPACE_NETWORK2);
            networkDrive2.SetupGet(d => d.TotalSize).Returns(TOTAL_SPACE_NETWORK2);

            localDrive1.SetupGet(d => d.Name).Returns("C");
            localDrive2.SetupGet(d => d.Name).Returns("D");
            networkDrive1.SetupGet(d => d.Name).Returns("Y");
            networkDrive2.SetupGet(d => d.Name).Returns("Z");

            provider.Setup(p => p.GetLocalDrive()).Returns(new List<IDriveInfo>(){localDrive1.Object, localDrive2.Object});
            provider.Setup(p => p.GetNetworkDrive()).Returns(new List<IDriveInfo>() { networkDrive1.Object, networkDrive2.Object });

            return new SystemIO_Connector(provider.Object);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.Tests.DesktopAssistant.BL.Tests.Hardware;
using Hardware;
using Hardware.Connectors;
using Hardware.Exceptions;
using Hardware.Models;
using Moq;

namespace DesktopAssistant.Tests.Hardware.Tests
{
    public class HardwareManagerTest : IHardwareManagerTest
    {
        private readonly MonitoringTarget _notSupportedTarget = MonitoringTarget.FAN_Speed;
        private ResourceCollection _testConfig;
        
        protected override IEnumerable<MonitoringTarget> GetConfigurationInitialTargets()
        {
            return _testConfig?.InitialTargets;
        }

        protected override IEnumerable<MonitoringTarget> GetConfigurationAllTargets()
        {
            return _testConfig?.Resources.Select(r => r.TargetType);
        }

        protected override IHardwareManager GivenHardwareManager()
        {
            Mock<XmlHelper> xmlHelper = GivenXmlHelperMock();
            Mock<IFactory<ConnectorBase>> factory = GivenConnectorFactoryMock();

            return new HardwareManager(factory.Object, xmlHelper.Object);
        }

        protected override MonitoringTarget GetNotSupportedTarget()
        {
            return _notSupportedTarget;
        }

        private Mock<IFactory<ConnectorBase>> GivenConnectorFactoryMock()
        {
            Mock<IFactory<ConnectorBase>> factoryMock = new Mock<IFactory<ConnectorBase>>();
            Mock<ConnectorBase> connectorMock = new Mock<ConnectorBase>();
            HardwareInformation hardwareInfoGCpuLoad = new HardwareInformation() { MainValue = 30.0, ShortName = "CPU", UnitSymbol = "%" };
            HardwareInformation hardwareInfoGpuTemp = new HardwareInformation() { MainValue = 30.0, ShortName = "GPU", UnitSymbol = "°C" };
            HardwareInformation hardwareInfoRamUsage = new HardwareInformation() { MainValue = 30.0, ShortName = "RAM", UnitSymbol = "%" };
            HardwareInformation hardwareInfoGpuLoad = new HardwareInformation() { MainValue = 30.0, ShortName = "GPU", UnitSymbol = "%" };
            HardwareInformation hardwareInfoMotherBoard = new HardwareInformation() { MainValue = "TEST_MOTHER_BOARD", ShortName = "MB" };

            factoryMock.Setup(fac => fac.CreateInstance(It.IsAny<string>())).Returns(connectorMock.Object);
            connectorMock.Setup(con => con.GetValue(MonitoringTarget.CPU_Load)).Returns(hardwareInfoGCpuLoad);
            connectorMock.Setup(con => con.GetValue(MonitoringTarget.RAM_Usage)).Returns(hardwareInfoRamUsage);
            connectorMock.Setup(con => con.GetValue(MonitoringTarget.GPU_Temp)).Returns(hardwareInfoGpuTemp);
            connectorMock.Setup(con => con.GetValue(MonitoringTarget.Mother_Board_Make)).Returns(hardwareInfoMotherBoard);
            connectorMock.Setup(con => con.GetValue(_notSupportedTarget))
                .Throws(new HardwareCommunicationException(_notSupportedTarget));

            return factoryMock;
        }
        
        private Mock<XmlHelper> GivenXmlHelperMock()
        {
            BuildTestConfiguration();
            BuildComputerResources();
            Mock<XmlHelper> xmlHelper = new Mock<XmlHelper>();
            xmlHelper.Setup(x => x.DeserializeConfiguration<ResourceCollection>(It.IsAny<string>())).Returns(_testConfig);
            return xmlHelper;
        }
        
        private void BuildTestConfiguration()
        {
            _testConfig = new ResourceCollection();
            _testConfig.InitialTargets = new List<MonitoringTarget>()
            {
                MonitoringTarget.CPU_Load,
                MonitoringTarget.GPU_Temp,
                MonitoringTarget.RAM_Usage
            };
            
        }

        private void BuildComputerResources()
        {
            _testConfig.Resources = new List<ComputerResource>();
            _testConfig.Resources.Add(new ComputerResource()
            {
                TargetType = MonitoringTarget.CPU_Load,
                ConnectorName = "WMI"
            });
            _testConfig.Resources.Add(new ComputerResource()
            {
                TargetType = MonitoringTarget.RAM_Usage,
                ConnectorName = "WMI"
            });
            _testConfig.Resources.Add(new ComputerResource()
            {
                TargetType = MonitoringTarget.GPU_Temp,
                ConnectorName = "OpenHardware"
            });
            _testConfig.Resources.Add(new ComputerResource()
            {
                TargetType = MonitoringTarget.Mother_Board_Make,
                ConnectorName = "OpenHardware",
                ExcludeFromMonitoring = true
            });
            _testConfig.Resources.Add(new ComputerResource()
            {
                TargetType = MonitoringTarget.GPU_Load,
                ConnectorName = "OpenHardware"
            });
            _testConfig.Resources.Add(new ComputerResource()
            {
                TargetType = MonitoringTarget.FAN_Speed,
                ConnectorName = "OpenHardware"
            });
        }
    }
}
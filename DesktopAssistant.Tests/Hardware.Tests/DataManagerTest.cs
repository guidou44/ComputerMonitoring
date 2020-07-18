using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Helpers;
using Hardware.Connectors;
using Hardware.Enums;
using Hardware.Exceptions;
using Hardware.Models;
using Hardware;
using Moq;
using Xunit;

namespace DesktopAssistantTests.Hardware
{
    public class DataManagerTest
    {
        private const string ALTERNATE_CONFIG_PATH_WITH_REMOTE = @"..\..\Configuration\MonitoringConfiguration.cfg";
        private const string ALTERNATE_CONFIG_PATH_WO_REMOTE = @"..\..\Configuration\MonitoringConfigurationNoRemote.cfg";

        [Fact]
        public void GivenCpuLoadAndHddUsageInConfigFile_WhenAskingForInitialTarget_ThenItReturnsProperTargets()
        {
            Mock<IFactory<ConnectorBase>> factoryMock = new Mock<IFactory<ConnectorBase>>();
            Mock<ConnectorBase> connectorMock = new Mock<ConnectorBase>();
            Mock<HardwareInformation> hardwreInfoMock = new Mock<HardwareInformation>();
            connectorMock.Setup(c => c.GetValue(It.IsAny<MonitoringTarget>())).Returns(hardwreInfoMock.Object);
            factoryMock.Setup(s => s.CreateInstance(It.IsAny<string>())).Returns(connectorMock.Object);

            DataManager dataManagerSubject = new DataManager(factoryMock.Object, new XmlHelper(), ALTERNATE_CONFIG_PATH_WITH_REMOTE);
            IEnumerable<MonitoringTarget> initialTarget = dataManagerSubject.GetInitialTargets();
            Assert.True(initialTarget.Count() == 3);
            Assert.Contains(MonitoringTarget.CPU_Load, initialTarget);
            Assert.Contains(MonitoringTarget.Primary_HDD_Used_Space, initialTarget);
        }

        [Fact]
        public void GivenNonReachableRemoteTargetInConfigFile_WhenCheckingIfRemote_ThenItReturnsFalse()
        {
            IFactory<ConnectorBase> factory = GetMockFactory();
            DataManager dataManagerSubject = new DataManager(factory, new XmlHelper(), ALTERNATE_CONFIG_PATH_WITH_REMOTE);

            Assert.False(dataManagerSubject.IsRemoteMonitoringEnabled());
        }

        [Fact]
        public void GivenRemoteTargetInConfigFile_WhenCheckingIfRemote_ThenItReturnsTrue()
        {
            IFactory<ConnectorBase> factory = GetMockFactory();
            XmlHelper xmlHelper = GetXmlHelperMock();
            DataManager dataManagerSubject = new DataManager(factory, xmlHelper, ALTERNATE_CONFIG_PATH_WITH_REMOTE);

            Assert.True(dataManagerSubject.IsRemoteMonitoringEnabled());
        }

        [Fact]
        public void GivenRemoteTargetInConfigFile_WhenGettingRemoteTargets_ThenItReturnsProperTargets()
        {
            IFactory<ConnectorBase> factory = GetMockFactory();
            XmlHelper xmlHelper = GetXmlHelperMock();
            DataManager dataManagerSubject = new DataManager(factory, xmlHelper, ALTERNATE_CONFIG_PATH_WITH_REMOTE);

            IEnumerable<MonitoringTarget> remoteTargets = dataManagerSubject.GetRemoteTargets();

            Assert.True(remoteTargets.Count() == 1);
            Assert.Contains(MonitoringTarget.Server_CPU_Load, remoteTargets);
        }

        [Fact]
        public void GivenNoRemoteTargetInConfigFile_WhenGettingRemoteTargets_ThenItReturnsEmptyCollection()
        {
            IFactory<ConnectorBase> factory = GetMockFactory();
            DataManager dataManagerSubject = new DataManager(factory, new XmlHelper(), ALTERNATE_CONFIG_PATH_WO_REMOTE);

            IEnumerable<MonitoringTarget> remoteTargets = dataManagerSubject.GetRemoteTargets();

            Assert.True(remoteTargets.Count() == 0);
        }

        [Fact]
        public void GivenRemoteTargetInConfigFile_WhenGettingLocalTargets_ThenItDoesNotContainsRemote()
        {
            IFactory<ConnectorBase> factory = GetMockFactory();
            XmlHelper xmlHelper = GetXmlHelperMock();
            DataManager dataManagerSubject = new DataManager(factory, xmlHelper, ALTERNATE_CONFIG_PATH_WITH_REMOTE);
            IEnumerable<MonitoringTarget> remoteTargets = dataManagerSubject.GetRemoteTargets();

            IEnumerable<MonitoringTarget> localTargets = dataManagerSubject.GetLocalTargets();

            Assert.False(localTargets.Intersect(remoteTargets).Count() > 0);
            Assert.Contains(MonitoringTarget.RAM_Usage, localTargets);
        }

        [Fact]
        public void GivenInitialTargets_WhenGettingAllTargets_ThenItReturnsMoreThanInitialTargets()
        {
            IFactory<ConnectorBase> factory = GetMockFactory();
            DataManager dataManagerSubject = new DataManager(factory, new XmlHelper(), ALTERNATE_CONFIG_PATH_WITH_REMOTE);
            IEnumerable<MonitoringTarget> initialTargets = dataManagerSubject.GetInitialTargets();

            IEnumerable<MonitoringTarget> allTargets = dataManagerSubject.GetAllTargets();
            
            Assert.True(allTargets.Count() > initialTargets.Count());
        }

        [Fact]
        public void GivenNonInitialValidTarget_WhenGettingValue_ThenItReturnsProper()
        {
            IFactory<ConnectorBase> factory = GetMockFactory();
            XmlHelper xmlHelper = GetXmlHelperMock();
            DataManager dataManagerSubject = new DataManager(factory, xmlHelper, ALTERNATE_CONFIG_PATH_WITH_REMOTE);

            HardwareInformation hardwareInfo = dataManagerSubject.GetCalculatedValue(MonitoringTarget.GPU_Temp);

            Assert.NotNull(hardwareInfo);
            Assert.NotNull(hardwareInfo.MainValue);
            Assert.False(string.IsNullOrEmpty(hardwareInfo.ShortName));
            Assert.False(string.IsNullOrEmpty(hardwareInfo.UnitSymbol));
        }

        [Fact]
        public void GivenNonInitialInvalidTargetAndValidOne_WhenGettingAllvalues_ThenItRemoveItFromCollection()
        {
            IFactory<ConnectorBase> factory = GetMockFactory();
            XmlHelper xmlHelper = GetXmlHelperMock();
            DataManager dataManagerSubject = new DataManager(factory, xmlHelper, ALTERNATE_CONFIG_PATH_WITH_REMOTE);
            List<MonitoringTarget> targets = new List<MonitoringTarget>() { MonitoringTarget.GPU_Temp, MonitoringTarget.CPU_Temp};

            IEnumerable<HardwareInformation> hardwareInfo = dataManagerSubject.GetCalculatedValues(targets);

            Assert.True(targets.Count() == 1);
            Assert.True(hardwareInfo.Count() == 2);
        }

        #region private Methods

        private IFactory<ConnectorBase> GetMockFactory()
        {
            Mock<IFactory<ConnectorBase>> factoryMock = new Mock<IFactory<ConnectorBase>>();
            Mock<ConnectorBase> connectorMock = new Mock<ConnectorBase>();
            HardwareInformation hardwareInfoGpu = new HardwareInformation() { MainValue = 30.0, ShortName = "GPU", UnitSymbol = "°C" };
            HardwareInformation hardwareInfoServerCpu = new HardwareInformation() { MainValue = 30.0, ShortName = "S.CPU", UnitSymbol = "%" };
            HardwareInformation hardwareInfoRam = new HardwareInformation() { MainValue = 30.0, ShortName = "RAM", UnitSymbol = "%" };

            factoryMock.Setup(fac => fac.CreateInstance(It.IsAny<string>())).Returns(connectorMock.Object);
            connectorMock.Setup(con => con.GetValue(MonitoringTarget.GPU_Temp)).Returns(hardwareInfoGpu);
            connectorMock.Setup(con => con.GetValue(MonitoringTarget.Server_CPU_Load)).Returns(hardwareInfoServerCpu);
            connectorMock.Setup(con => con.GetValue(MonitoringTarget.RAM_Usage)).Returns(hardwareInfoRam);
            connectorMock.Setup(con => con.GetValue(MonitoringTarget.CPU_Temp)).Throws(new HardwareCommunicationException(MonitoringTarget.CPU_Temp));

            return factoryMock.Object;
        }

        private XmlHelper GetXmlHelperMock()
        {
            Mock<XmlHelper> xmlHelperMock = new Mock<XmlHelper>();
            Mock<ResourceCollection> resourceMock = new Mock<ResourceCollection>();
            Mock<ComputerResource> computerResourceMockRemote = new Mock<ComputerResource>();
            Mock<ComputerResource> computerResourceMockLocal = new Mock<ComputerResource>();
            Mock<ComputerResource> computerResourceMockGpuTemp = new Mock<ComputerResource>();
            Mock<ComputerResource> computerResourceMockCpuTemp = new Mock<ComputerResource>();

            xmlHelperMock.Setup(elm => elm.DeserializeConfiguration<ResourceCollection>(It.IsAny<string>())).Returns(resourceMock.Object);
            resourceMock.SetupGet(rm => rm.Ressources).Returns(new List<ComputerResource>() { computerResourceMockRemote.Object, 
                computerResourceMockLocal.Object, computerResourceMockGpuTemp.Object, computerResourceMockCpuTemp.Object });
            
            computerResourceMockRemote.SetupGet(crm => crm.IsRemote).Returns(true);
            computerResourceMockRemote.SetupGet(crm => crm.ExcludeFromMonitoring).Returns(false);
            computerResourceMockRemote.SetupGet(crm => crm.TargetType).Returns(MonitoringTarget.Server_CPU_Load);
            computerResourceMockRemote.Setup(crm => crm.TryPing()).Returns(true);

            computerResourceMockLocal.SetupGet(crm => crm.IsRemote).Returns(false);
            computerResourceMockLocal.SetupGet(crm => crm.ExcludeFromMonitoring).Returns(false);
            computerResourceMockLocal.SetupGet(crm => crm.TargetType).Returns(MonitoringTarget.RAM_Usage);
            computerResourceMockLocal.Setup(crm => crm.TryPing()).Returns(false);

            computerResourceMockGpuTemp.SetupGet(crm => crm.IsRemote).Returns(false);
            computerResourceMockGpuTemp.SetupGet(crm => crm.ExcludeFromMonitoring).Returns(false);
            computerResourceMockGpuTemp.SetupGet(crm => crm.TargetType).Returns(MonitoringTarget.GPU_Temp);
            computerResourceMockGpuTemp.Setup(crm => crm.TryPing()).Returns(false);

            computerResourceMockCpuTemp.SetupGet(crm => crm.IsRemote).Returns(false);
            computerResourceMockCpuTemp.SetupGet(crm => crm.ExcludeFromMonitoring).Returns(false);
            computerResourceMockCpuTemp.SetupGet(crm => crm.TargetType).Returns(MonitoringTarget.CPU_Temp);
            computerResourceMockCpuTemp.Setup(crm => crm.TryPing()).Returns(false);

            return xmlHelperMock.Object;
        }

        #endregion
    }
}

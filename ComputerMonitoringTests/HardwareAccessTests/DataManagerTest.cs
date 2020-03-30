using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Helpers;
using HardwareAccess.Connectors;
using HardwareAccess.Enums;
using HardwareAccess.Exceptions;
using HardwareAccess.Factories;
using HardwareAccess.Models;
using HardwareManipulation;
using Moq;
using Xunit;

namespace ComputerMonitoringTests.HardwareAccessTests
{
    public class DataManagerTest
    {
        const string ALTERNATE_CONFIG_PATH_WITH_REMOTE = @"..\..\Configuration\MonitoringConfiguration.cfg";
        const string ALTERNATE_CONFIG_PATH_WO_REMOTE = @"..\..\Configuration\MonitoringConfigurationNoRemote.cfg";

        public DataManagerTest()
        {

        }

        [Fact]
        public void GivenCpuLoadAndHddUsageInConfigFile_WhenAskingForInitialTarget_ThenItReturnsProperTargets()
        {
            DataManager dataManagerSubject = new DataManager(ALTERNATE_CONFIG_PATH_WITH_REMOTE, new ConnectorFactory(), new XmlHelper());
            IEnumerable<MonitoringTarget> initialTarget = dataManagerSubject.GetInitialTargets();
            Assert.True(initialTarget.Count() == 3);
            Assert.Contains(MonitoringTarget.CPU_Load, initialTarget);
            Assert.Contains(MonitoringTarget.Primary_HDD_Used_Space, initialTarget);
        }

        [Fact]
        public void GivenNonReachableRemoteTargetInConfigFile_WhenCheckingIfRemote_ThenItReturnsFalse()
        {
            IFactory factory = GetMockFactory();
            DataManager dataManagerSubject = new DataManager(ALTERNATE_CONFIG_PATH_WITH_REMOTE, factory, new XmlHelper());

            Assert.False(dataManagerSubject.IsRemoteMonitoringEnabled());
        }

        [Fact]
        public void GivenRemoteTargetInConfigFile_WhenCheckingIfRemote_ThenItReturnsTrue()
        {
            IFactory factory = GetMockFactory();
            XmlHelper xmlHelper = GetXmlHelperMock();
            DataManager dataManagerSubject = new DataManager(ALTERNATE_CONFIG_PATH_WITH_REMOTE, factory, xmlHelper);

            Assert.True(dataManagerSubject.IsRemoteMonitoringEnabled());
        }

        [Fact]
        public void GivenRemoteTargetInConfigFile_WhenGettingRemoteTargets_ThenItReturnsProperTargets()
        {
            IFactory factory = GetMockFactory();
            XmlHelper xmlHelper = GetXmlHelperMock();
            DataManager dataManagerSubject = new DataManager(ALTERNATE_CONFIG_PATH_WITH_REMOTE, factory, xmlHelper);

            IEnumerable<MonitoringTarget> remoteTargets = dataManagerSubject.GetRemoteTargets();

            Assert.True(remoteTargets.Count() == 1);
            Assert.Contains(MonitoringTarget.Server_CPU_Load, remoteTargets);
        }

        [Fact]
        public void GivenNoRemoteTargetInConfigFile_WhenGettingRemoteTargets_ThenItReturnsEmptyCollection()
        {
            IFactory factory = GetMockFactory();
            DataManager dataManagerSubject = new DataManager(ALTERNATE_CONFIG_PATH_WO_REMOTE, factory, new XmlHelper());

            IEnumerable<MonitoringTarget> remoteTargets = dataManagerSubject.GetRemoteTargets();

            Assert.True(remoteTargets.Count() == 0);
        }

        [Fact]
        public void GivenRemoteTargetInConfigFile_WhenGettingLocalTargets_ThenItDoesNotContainsRemote()
        {
            IFactory factory = GetMockFactory();
            XmlHelper xmlHelper = GetXmlHelperMock();
            DataManager dataManagerSubject = new DataManager(ALTERNATE_CONFIG_PATH_WITH_REMOTE, factory, xmlHelper);
            IEnumerable<MonitoringTarget> remoteTargets = dataManagerSubject.GetRemoteTargets();

            IEnumerable<MonitoringTarget> localTargets = dataManagerSubject.GetLocalTargets();

            Assert.False(localTargets.Intersect(remoteTargets).Count() > 0);
            Assert.Contains(MonitoringTarget.RAM_Usage, localTargets);
        }

        [Fact]
        public void GivenInitialTargets_WhenGettingAllTargets_ThenItReturnsMoreThanInitialTargets()
        {
            IFactory factory = GetMockFactory();
            DataManager dataManagerSubject = new DataManager(ALTERNATE_CONFIG_PATH_WITH_REMOTE, factory, new XmlHelper());
            IEnumerable<MonitoringTarget> initialTargets = dataManagerSubject.GetInitialTargets();

            IEnumerable<MonitoringTarget> allTargets = dataManagerSubject.GetAllTargets();
            
            Assert.True(allTargets.Count() > initialTargets.Count());
        }

        [Fact]
        public void GivenNonInitialValidTarget_WhenGettingValue_ThenItReturnsProper()
        {
            IFactory factory = GetMockFactory();
            XmlHelper xmlHelper = GetXmlHelperMock();
            DataManager dataManagerSubject = new DataManager(ALTERNATE_CONFIG_PATH_WITH_REMOTE, factory, xmlHelper);

            HardwareInformation hardwareInfo = dataManagerSubject.GetCalculatedValue(MonitoringTarget.GPU_Temp);

            Assert.NotNull(hardwareInfo);
            Assert.NotNull(hardwareInfo.MainValue);
            Assert.False(string.IsNullOrEmpty(hardwareInfo.ShortName));
            Assert.False(string.IsNullOrEmpty(hardwareInfo.UnitSymbol));
        }

        [Fact]
        public void GivenNonInitialInvalidTargetAndValidOne_WhenGettingAllvalues_ThenItRemoveItFromCollection()
        {
            IFactory factory = GetMockFactory();
            XmlHelper xmlHelper = GetXmlHelperMock();
            DataManager dataManagerSubject = new DataManager(ALTERNATE_CONFIG_PATH_WITH_REMOTE, factory, xmlHelper);
            List<MonitoringTarget> targets = new List<MonitoringTarget>() { MonitoringTarget.GPU_Temp, MonitoringTarget.CPU_Temp};

            IEnumerable<HardwareInformation> hardwareInfo = dataManagerSubject.GetCalculatedValues(targets);

            Assert.True(targets.Count() == 1);
            Assert.True(hardwareInfo.Count() == 2);
        }

        #region private Methods

        private IFactory GetMockFactory()
        {
            Mock<IFactory> factoryMock = new Mock<IFactory>();
            Mock<ConnectorBase> connectorMock = new Mock<ConnectorBase>();
            HardwareInformation hardwareInfoGpu = new HardwareInformation() { MainValue = 30.0, ShortName = "GPU", UnitSymbol = "°C" };
            HardwareInformation hardwareInfoServerCpu = new HardwareInformation() { MainValue = 30.0, ShortName = "S.CPU", UnitSymbol = "%" };
            HardwareInformation hardwareInfoRam = new HardwareInformation() { MainValue = 30.0, ShortName = "RAM", UnitSymbol = "%" };

            factoryMock.Setup(fac => fac.CreateInstance<ConnectorBase>(It.IsAny<string>())).Returns(connectorMock.Object);
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

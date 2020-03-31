using HardwareAccess.Connectors;
using HardwareManipulation;
using HardwareManipulation.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ComputerMonitoringTests.HardwareAccessTests
{
    public abstract class IFactoryTest
    {
        private IFactory _factorySubject;

        public IFactoryTest()
        {
            _factorySubject = ProvideFactory();
        }

        protected abstract IFactory ProvideFactory();

        [Fact]
        public void GivenConnectorFactory_WhenProvidingValidName_ThenItInstantiateProperConnector()
        {
            ConnectorBase wmiConnector = _factorySubject.CreateInstance<ConnectorBase>("WMI");
            ConnectorBase ioConnector = _factorySubject.CreateInstance<ConnectorBase>("SystemIO");

            Assert.IsType<WMI_Connector>(wmiConnector);
            Assert.IsType<SystemIO_Connector>(ioConnector);
        }

        [Fact]
        public void GivenConnectorFactory_WhenProvidingInvalidName_ThenItThrowsProper()
        {
            Assert.Throws<InvalidConnectorException>(() => _factorySubject.CreateInstance<ConnectorBase>("BULLSHIT"));
        }
    }
}

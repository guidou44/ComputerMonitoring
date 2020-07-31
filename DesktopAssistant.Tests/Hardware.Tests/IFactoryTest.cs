using Hardware.Connectors;
using Hardware;
using Hardware.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesktopAssistant.Tests.Hardware.Tests
{
    public abstract class IFactoryTest
    {
        protected abstract IFactory<ConnectorBase> ProvideFactory();

        [Theory]
        [InlineData(typeof(WMI_Connector), "WMI")]
        [InlineData(typeof(SystemIO_Connector), "SystemIO")]
        [InlineData(typeof(NVDIA_API_Connector), "NVDIA_API")]
        [InlineData(typeof(ASPNET_API_Connector), "ASPNET_API")]
        [InlineData(typeof(OpenHardware_Connector), "OpenHardware")]
        public void GivenConnectorFactory_WhenProvidingValidName_ThenItInstantiateProperConnector(Type expected, string name)
        {
            IFactory<ConnectorBase> factorySubject = ProvideFactory();
            ConnectorBase connector = factorySubject.CreateInstance(name);

            Assert.Equal(expected.FullName, connector.GetType().FullName);
        }

        [Fact]
        public void GivenConnectorFactory_WhenProvidingInvalidName_ThenItThrowsProper()
        {
            IFactory<ConnectorBase> factorySubject = ProvideFactory();
            Assert.Throws<InvalidConnectorException>(() => factorySubject.CreateInstance("BULLSHIT"));
        }
    }
}

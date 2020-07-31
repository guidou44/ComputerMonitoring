using Hardware;
using Hardware.Connectors;
using Hardware.Factories;

namespace DesktopAssistant.Tests.Hardware.Tests.Factories
{
    public class ConnectorFactoryTest : IFactoryTest
    {
        protected override IFactory<ConnectorBase> ProvideFactory()
        {

            return new ConnectorFactory(ConnectorFactoryHelper.ProvideConnectorFactoryDelegateMock());
        }
    }
}
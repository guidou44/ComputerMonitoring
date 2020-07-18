using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DesktopAssistant.Factories;
using Hardware.Connectors;
using Hardware.Helpers;
using Hardware;
using Hardware.Components;
using Hardware.Exceptions;
using Moq;

namespace DesktopAssistantTests.Hardware.Factories
{
    public class ConnectorFactoryTest : IFactoryTest
    {
        protected override IFactory<ConnectorBase> ProvideFactory()
        {

            return new ConnectorFactory(ConnectorFactoryHelper.ProvideConnectorFactoryDelegateMock());
        }
    }
}

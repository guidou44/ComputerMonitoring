using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using ComputerRessourcesMonitoring.Factories;
using HardwareAccess.Connectors;
using HardwareAccess.Helpers;
using HardwareManipulation;
using HardwareManipulation.Components;
using HardwareManipulation.Exceptions;
using Moq;

namespace ComputerMonitoringTests.HardwareAccessTests.Factories
{
    public class ConnectorFactoryTest : IFactoryTest
    {
        protected override IFactory<ConnectorBase> ProvideFactory()
        {

            return new ConnectorFactory(ConnectorFactoryHelper.ProvideConnectorFactoryDelegateMock());
        }
    }
}

using HardwareAccess.Connectors;
using HardwareAccess.Helpers;
using HardwareManipulation.Components;
using HardwareManipulation.Exceptions;
using HardwareManipulation.Wrappers;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerMonitoringTests.HardwareAccessTests.Factories
{
    public static class ConnectorFactoryHelper
    {
        public static Func<Type, ConnectorBase> ProvideConnectorFactoryDelegateMock()
        {
            Mock<ServerResourceApiClient> apiClientMock = new Mock<ServerResourceApiClient>();
            Mock<OpenHardwareWrapper> ohComputerMock = new Mock<OpenHardwareWrapper>();
            ohComputerMock.Setup(C => C.Open()).Verifiable();
            Mock<WmiHelper> wmiHelperMock = new Mock<WmiHelper>();
            Mock <INvidiaComponent> nvidiaComponentMock = new Mock<INvidiaComponent>();
            nvidiaComponentMock.Setup(N => N.Initialize()).Verifiable();

            Func<Type, ConnectorBase> delegatObject = T =>
            {
                if (T == typeof(NVDIA_API_Connector)) { return new NVDIA_API_Connector(nvidiaComponentMock.Object); }
                if (T == typeof(ASPNET_API_Connector)) { return new ASPNET_API_Connector(apiClientMock.Object); }
                if (T == typeof(OpenHardware_Connector)) { return new OpenHardware_Connector(ohComputerMock.Object); }
                if (T == typeof(SystemIO_Connector)) { return new SystemIO_Connector(); }
                if (T == typeof(WMI_Connector)) { return new WMI_Connector(wmiHelperMock.Object, new PerformanceCounter()); }
                throw new InvalidConnectorException("Invalid connector");
            };


            return delegatObject;
        }
    }
}

using Hardware.Connectors;
using Hardware.Helpers;
using Hardware.Components;
using Hardware.Exceptions;
using Hardware.Wrappers;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistantTests.Hardware.Factories
{
    public static class ConnectorFactoryHelper
    {
        public static Func<Type, ConnectorBase> ProvideConnectorFactoryDelegateMock()
        {
            Mock<ServerResourceApiClientWrapper> apiClientMock = new Mock<ServerResourceApiClientWrapper>();
            Mock<OpenHardwareComputerWrapper> ohComputerMock = new Mock<OpenHardwareComputerWrapper>();
            ohComputerMock.Setup(C => C.Open()).Verifiable();
            Mock<WmiHelper> wmiHelperMock = new Mock<WmiHelper>();
            Mock <INvidiaComponent> nvidiaComponentMock = new Mock<INvidiaComponent>();
            Mock<IPerformanceCounter> perfCounter = new Mock<IPerformanceCounter>();
            Mock<IDriveInfoProvider> provider = new Mock<IDriveInfoProvider>();
            nvidiaComponentMock.Setup(N => N.Initialize()).Verifiable();

            Func<Type, ConnectorBase> delegatObject = T =>
            {
                if (T == typeof(NVDIA_API_Connector)) { return new NVDIA_API_Connector(nvidiaComponentMock.Object); }
                if (T == typeof(ASPNET_API_Connector)) { return new ASPNET_API_Connector(apiClientMock.Object); }
                if (T == typeof(OpenHardware_Connector)) { return new OpenHardware_Connector(ohComputerMock.Object); }
                if (T == typeof(SystemIO_Connector)) { return new SystemIO_Connector(provider.Object); }
                if (T == typeof(WMI_Connector)) { return new WMI_Connector(wmiHelperMock.Object, perfCounter.Object); }
                throw new InvalidConnectorException("Invalid connector");
            };


            return delegatObject;
        }
    }
}

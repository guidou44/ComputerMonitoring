using HardwareManipulation.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ComputerMonitoringTests.HardwareAccessTests.Components
{
    public class ServerResourceApiClientTest
    {
        [Fact]
        public void GivenApiClient_WhenInstantiateWithEmptyConstructor_ThenItUsesParentCtorWithArguments()
        {
            ServerResourceApiClient apiClientSubject = new ServerResourceApiClient();


        }
    }
}

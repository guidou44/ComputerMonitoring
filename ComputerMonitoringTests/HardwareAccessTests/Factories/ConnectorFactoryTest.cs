using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareAccess.Factories;
using HardwareManipulation;

namespace ComputerMonitoringTests.HardwareAccessTests.Factories
{
    public class ConnectorFactoryTest : IFactoryTest
    {
        protected override IFactory ProvideFactory()
        {
            return new ConnectorFactory();
        }
    }
}

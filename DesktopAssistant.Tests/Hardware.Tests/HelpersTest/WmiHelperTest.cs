using Hardware.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesktopAssistantTests.Hardware.HelpersTest
{
    public class WmiHelperTest
    {
        private WmiHelper wmiHelperSubject;

        public WmiHelperTest()
        {
            wmiHelperSubject = new WmiHelper();
        }

        [Fact]
        public void GivenValidWmiPathAndKey_WhenRequestingData_ThenItReturnsProper()
        {
            double loadPercentage = wmiHelperSubject.GetWmiValue<double>("Win32_Processor", "LoadPercentage");
            Assert.True(loadPercentage > 0);
        }
    }
}

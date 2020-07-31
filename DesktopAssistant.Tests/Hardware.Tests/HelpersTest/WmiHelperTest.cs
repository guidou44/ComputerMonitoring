using Hardware.Helpers;
using Xunit;

namespace DesktopAssistant.Tests.Hardware.Tests.HelpersTest
{
    public class WmiHelperTest
    {
        public WmiHelperTest()
        {
            wmiHelperSubject = new WmiHelper();
        }

        private WmiHelper wmiHelperSubject;

        [Fact]
        public void GivenValidWmiPathAndKey_WhenRequestingData_ThenItReturnsProper()
        {
            double loadPercentage = wmiHelperSubject.GetWmiValue<double>("Win32_Processor", "LoadPercentage");
            Assert.True(loadPercentage > 0);
        }
    }
}

using DesktopAssistant.ViewModels;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.Tests.ViewModels
{
    public class HardwareViewModelTest
    {
        [Fact]
        public void GivenHardwareInfo_WhenConvertToString_ThenItFormatsProper()
        {
            HardwareViewModel subject = new HardwareViewModel()
            {
                MainValue = "TEST",
                ShortName = "T",
                UnitSymbol = "PASSED"
            };

            Assert.Equal("TEST PASSED", subject.ToString()); ;
        }
    }
}
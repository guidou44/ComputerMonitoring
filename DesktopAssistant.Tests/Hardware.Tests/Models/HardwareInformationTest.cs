using Hardware.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesktopAssistantTests.HardwareAccessTests.Models
{
    public class HardwareInformationTest
    {
        [Fact]
        public void GivenHardwareInfo_WhenConvertToString_ThenItFormatsProper()
        {
            HardwareInformation subject = new HardwareInformation()
            {
                MainValue = "TEST",
                ShortName = "T",
                UnitSymbol = "PASSED"
            };

            Assert.Equal("TEST PASSED", subject.ToString()); ;
        }
    }
}

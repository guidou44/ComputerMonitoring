using Common.UI.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesktopAssistantTests.Common.UI.Tests.Converters
{
    public class CommandParameterConverterTest
    {
        private CommandParameterConverter converterSubject;
        public CommandParameterConverterTest()
        {
            converterSubject = new CommandParameterConverter();
        }

        [Fact]
        public void GivenMultipleCommandParameters_WhenConverting_ThenItReturnsShallowCopy()
        {
            object[] parameters = new object[] {3, "string", 14.5M };

            object convertedParams = converterSubject.Convert(parameters, typeof(object), 2, CultureInfo.InvariantCulture);

            Assert.NotEqual(parameters.GetHashCode(), convertedParams.GetHashCode());
        }

        [Fact]
        public void GivenNotImplementedConvertBack_WhenConvertingBack_ThenItThrowsProper()
        {
            object[] parameters = new object[] { 3, "string", 14.5M };

            Assert.Throws<NotImplementedException>(() => converterSubject.ConvertBack(parameters, new Type[] { typeof(object) }, 2, CultureInfo.InvariantCulture));
        }
    }
}

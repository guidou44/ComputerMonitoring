using Common.Exceptions;
using Common.Helpers;
using Hardware.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesktopAssistant.Tests.Common.Tests.Helpers
{
    public class XmlHelperTest
    {
        const string ALTERNATE_CONFIG_PATH_WO_REMOTE = @"..\..\Configuration\MonitoringConfigurationNoRemote.cfg";
        private XmlHelper xmlHelperSubject;

        public XmlHelperTest()
        {

            xmlHelperSubject = new XmlHelper();
        }

        [Fact]
        public void GivenValidConfigFile_WhenDeserializing_ThenItReturnsProper()
        {
            ResourceCollection deserialized = xmlHelperSubject.DeserializeConfiguration<ResourceCollection>(ALTERNATE_CONFIG_PATH_WO_REMOTE);

            Assert.NotNull(deserialized);
            Assert.NotEmpty(deserialized.Resources);
        }

        [Fact]
        public void GivenInvalidConfigFile_WhenDeserializing_ThenItThrowsProper()
        {
            const string invalidLogFile = "........";

            Assert.Throws<XmlDeserializationException>(() => xmlHelperSubject.DeserializeConfiguration<ResourceCollection>(invalidLogFile));
        }
    }
}

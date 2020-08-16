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
        private const string AlternateConfigPathWoRemote = @"..\..\..\Configuration\MonitoringConfigurationNoRemote.cfg";
        private const string RandomFilePath = @"..\..\..\Configuration\TEST.xml";
        private readonly XmlHelper _xmlHelperSubject;

        public XmlHelperTest()
        {

            _xmlHelperSubject = new XmlHelper();
        }

        [Fact]
        public void GivenValidConfigFile_WhenDeserializing_ThenItReturnsProper()
        {
            ResourceCollection deserialized = _xmlHelperSubject.Deserialize<ResourceCollection>(AlternateConfigPathWoRemote);

            Assert.NotNull(deserialized);
            Assert.NotEmpty(deserialized.Resources);
        }

        [Fact]
        public void GivenInvalidConfigFile_WhenDeserializing_ThenItThrowsProper()
        {
            const string invalidLogFile = "........";

            Assert.Throws<XmlDeserializationException>(() => _xmlHelperSubject.Deserialize<ResourceCollection>(invalidLogFile));
        }

        [Fact]
        public void GivenInvalidSerializePath_WhenSerialize_ThenItThrowsProperException()
        {
            const string invalidLogFile = "........";
            object obj = new object();

            Assert.Throws<XmlSerializationException>(() => _xmlHelperSubject.SerializeOverwrite(obj, invalidLogFile));
        }
        
        [Fact]
        public void GivenSerializable_WhenSerialize_ThenItOverwritesFileProperly()
        {
            Serializable serializable = new Serializable();
            serializable.Name = "TEST";
            serializable.Age = 3;
            
            _xmlHelperSubject.SerializeOverwrite(serializable, RandomFilePath);
            Serializable deserialized = _xmlHelperSubject.Deserialize<Serializable>(RandomFilePath);
            
            Assert.Equal(serializable.Age, deserialized.Age);
            Assert.Equal(serializable.Name, deserialized.Name);

            deserialized.Age = 4;
            deserialized.Name = "TEST_OVERWRITTEN";
            
            _xmlHelperSubject.SerializeOverwrite(deserialized, RandomFilePath);
            Serializable deserializedOverwritten = _xmlHelperSubject.Deserialize<Serializable>(RandomFilePath);
            
            Assert.Equal(deserialized.Age, deserializedOverwritten.Age);
            Assert.Equal(deserialized.Name, deserializedOverwritten.Name);
        }
        
        public class Serializable
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}

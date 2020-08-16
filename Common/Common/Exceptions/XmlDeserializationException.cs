using System;

namespace Common.Exceptions
{
    public class XmlDeserializationException : Exception
    {
        public XmlDeserializationException(string message) : base($"{nameof(XmlDeserializationException)}:\n" + message) { }
    }
    
    public class XmlSerializationException : Exception
    {
        public XmlSerializationException(string message) : base($"{nameof(XmlSerializationException)}:\n" + message) { }
    }
}
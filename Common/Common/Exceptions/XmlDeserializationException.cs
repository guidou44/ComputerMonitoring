using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class XmlDeserializationException : Exception
    {
        public XmlDeserializationException(string message) : base($"{nameof(XmlDeserializationException)}:\n" + message) { }
    }
}

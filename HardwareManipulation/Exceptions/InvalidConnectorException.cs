using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Exceptions
{
    public class InvalidConnectorException : Exception
    {
        public InvalidConnectorException(string message) : base($"{nameof(InvalidConnectorException)}:\n")
        {

        }
    }
}

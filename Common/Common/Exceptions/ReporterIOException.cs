using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class ReporterIOException : Exception
    {
        public ReporterIOException(string message) : base($"{nameof(ReporterIOException)}:\n" + message) { }
    }
}

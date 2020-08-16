using System;

namespace Common.Exceptions
{
    public class ReporterIOException : Exception
    {
        public ReporterIOException(string message) : base($"{nameof(ReporterIOException)}:\n" + message) { }
    }
}
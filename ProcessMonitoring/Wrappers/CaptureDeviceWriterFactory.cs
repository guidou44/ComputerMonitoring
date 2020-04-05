using ProcessMonitoring.Models;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Wrappers
{
    public class CaptureDeviceWriterFactory : ICaptureFactory<ICaptureFileWriter>
    {
        public ICaptureFileWriter CreateInstance(string reference)
        {
            return new CaptureFileWriterWrapper(new CaptureFileWriterDevice(reference));
        }
    }
}

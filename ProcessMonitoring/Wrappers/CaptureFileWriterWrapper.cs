using ProcessMonitoring.Models;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Wrappers
{
    public class CaptureFileWriterWrapper : ICaptureFileWriter
    {
        private CaptureFileWriterDevice _captureDevice;
        public CaptureFileWriterWrapper(CaptureFileWriterDevice captureDevice)
        {
            _captureDevice = captureDevice;
        }

        public void Write(CaptureEventWrapperArgs args)
        {
            _captureDevice.Write(args.Packet);
        }
    }
}

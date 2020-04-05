using ProcessMonitoring.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Models
{
    public interface ICaptureFileWriter
    {
        void Write(CaptureEventWrapperArgs args);
    }
}

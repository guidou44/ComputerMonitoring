using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Wrappers
{
    public delegate void PacketCaptureEventHandlerWrapper(object sender, CaptureEventWrapperArgs e);
}

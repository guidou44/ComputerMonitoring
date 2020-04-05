using ProcessMonitoring.Models;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Wrappers
{
    public class CaptureDeviceWrapper : IPacketCaptureDevice
    {
        private ICaptureDevice captureDevice;


        public event PacketCaptureEventHandlerWrapper OnPacketArrival
        {
            add
            {
                captureDevice.OnPacketArrival += (sender, e) => value.Invoke(sender, e);
            }
            remove
            {
                captureDevice.OnPacketArrival -= (sender, e) => value.Invoke(sender, e);
            }
        }

        public CaptureDeviceWrapper(ICaptureDevice captureDevice)
        {
            this.captureDevice = captureDevice;
        }

        public void Open(int readTimeOutMilliseconds)
        {
            captureDevice.Open(DeviceMode.Promiscuous, readTimeOutMilliseconds);
        }

        public void StartCapture()
        {
            captureDevice.StartCapture();
        }

        public string Filter
        {
            get { return captureDevice.Filter; }
            set { captureDevice.Filter = value; }
        }

    }
}

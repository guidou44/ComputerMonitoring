using System;

namespace DesktopAssistant.BL.ProcessWatch
{
    public class PacketData
    {
        public int Length;
        public DateTime Time;
        public string SourceAddress;
        public string DestinationAddress;
        public byte[] RawData;
        public string Summary;
    }
}
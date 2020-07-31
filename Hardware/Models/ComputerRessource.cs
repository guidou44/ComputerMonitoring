using System;
using System.Net.NetworkInformation;
using System.Xml.Serialization;
using DesktopAssistant.BL.Hardware;

namespace Hardware.Models
{ 
    public class ComputerResource
    {
        public ComputerResource()
        {
            IsRemote = false;
        }
        
        [XmlElement("Connector")]
        public string ConnectorName { get; set; }

        public virtual bool IsRemote { get; set; }

        public virtual bool? ExcludeFromMonitoring { get; set; }

        [XmlElement("TargetName")]
        public virtual MonitoringTarget TargetType { get; set; }

        [XmlElement("IP")]
        public string RemoteIp { get; set; }

        [XmlIgnore]
        public Exception CommunicationError { get; set; }

        public virtual bool TryPing()
        {
            if (!IsRemote)
                return false;

            try
            {
                var pingHost = new Ping();
                PingReply reply = pingHost.Send(RemoteIp);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}

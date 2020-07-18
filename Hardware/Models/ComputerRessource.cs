using Common.Reports;
using Hardware.Connectors;
using Hardware.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Hardware.Models
{ 
    public class ComputerResource
    {
        [XmlElement("Connector")]
        public string ConnectorName { get; set; }

        public virtual bool IsRemote { get; set; }

        public virtual bool? ExcludeFromMonitoring { get; set; }

        [XmlElement("TargetName")]
        public virtual MonitoringTarget TargetType { get; set; }

        [XmlElement("IP")]
        public string RemoteIp { get; set; }

        [XmlIgnore]
        public Exception Com_Error { get; set; }

        public virtual bool TryPing()
        {
            if (!IsRemote)
                return false;

            Ping pingHost;
            try
            {
                pingHost = new Ping();
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

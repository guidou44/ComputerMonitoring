using HardwareAccess.Connectors;
using HardwareAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HardwareAccess.Models
{ 
    public class ComputerRessource
    {
        [XmlElement("Connector")]
        public string ConnectorName { get; set; }

        public bool IsRemote { get; set; }

        public bool? ExcludeFromMonitoring { get; set; }

        [XmlElement("TargetName")]
        public MonitoringTarget TargetType { get; set; }

        [XmlElement("IP")]
        public string RemoteIp { get; set; }

        [XmlIgnore]
        public Exception Com_Error { get; set; }
    }
}

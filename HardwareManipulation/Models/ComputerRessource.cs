using HardwareManipulation.Connectors;
using HardwareManipulation.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HardwareManipulation.Models
{ 
    public class ComputerRessource
    {
        [XmlElement("Connector")]
        public string ConnectorName { get; set; }

        [XmlElement("IsRemote")]
        public bool IsRemote { get; set; }

        [XmlElement("TargetName")]
        public MonitoringTarget TargetName { get; set; }

        [XmlElement("IP")]
        public string RemoteIp { get; set; }
    }
}

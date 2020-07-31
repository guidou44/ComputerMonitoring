using System.Collections.Generic;
using System.Xml.Serialization;
using DesktopAssistant.BL.Hardware;

namespace Hardware.Models
{
    [XmlRoot("Configuration")]
    public class ResourceCollection
    {
        [XmlElement("InitialTarget")]
        public List<MonitoringTarget> InitialTargets { get; set; }

        [XmlElement("ComputerResource")]
        public virtual List<ComputerResource> Resources { get; set; }
    }
}

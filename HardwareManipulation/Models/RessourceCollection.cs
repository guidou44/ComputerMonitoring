using HardwareAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HardwareAccess.Models
{
    [XmlRoot("Configuration")]
    public class ResourceCollection
    {
        [XmlElement("InitialTarget")]
        public List<MonitoringTarget> InitialTargets { get; set; }

        [XmlElement("ComputerRessource")]
        public virtual List<ComputerResource> Ressources { get; set; }
    }
}

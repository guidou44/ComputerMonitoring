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
    public class RessourceCollection
    {
        [XmlElement("InitialTarget")]
        public List<MonitoringTarget> InitialTargets { get; set; }

        [XmlElement("ComputerRessource")]
        public List<ComputerRessource> Ressources { get; set; }
    }
}

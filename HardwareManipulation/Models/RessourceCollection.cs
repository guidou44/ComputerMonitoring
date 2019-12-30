using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HardwareManipulation.Models
{
    [XmlRoot("Configuration")]
    public class RessourceCollection
    {
        [XmlElement("ComputerRessource")]
        public List<ComputerRessource> Ressources { get; set; }
    }
}

using ComputerRessourcesMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerRessourcesMonitoring.ViewModels
{
    public class MonitoringTargetViewModel
    {
        public bool IsSelected { get; set; }
        public string DisplayName { get; set; }
        public MonitoringTarget Name { get; set; }
    }
}

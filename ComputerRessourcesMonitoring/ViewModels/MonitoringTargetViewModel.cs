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
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set 
            { 
                _isSelected = value; 
            }
        }

        public string displayName { get; set; }
        public MonitoringTarget type { get; set; }
        //Console.WriteLine();
    }
}

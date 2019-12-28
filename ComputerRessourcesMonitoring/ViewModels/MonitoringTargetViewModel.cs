using Common.UI.Infrastructure;
using ComputerRessourcesMonitoring.Events;
using ComputerRessourcesMonitoring.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ComputerRessourcesMonitoring.ViewModels
{
    public class MonitoringTargetViewModel : NotifyPropertyChanged
    {
        private IEventAggregator _eventHub;
        public MonitoringTargetViewModel(IEventAggregator eventHub, MonitoringTarget type)
        {
            _eventHub = eventHub;
            Type = type;
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set 
            { 
                _isSelected = value;
                RaisePropertyChanged(nameof(IsSelected));
            }
        }

        public string DisplayName { get; set; }
        public MonitoringTarget Type { get; set; }

        public ICommand PublishMonitoringOptionStatusCommand
        {
            get { return new RelayCommand(PublishMonitoringOptionStatusCommandExecute); }
        }

        public void PublishMonitoringOptionStatusCommandExecute()
        {
            _eventHub.GetEvent<OnMonitoringTargetSelectedEvent>().Publish(new KeyValuePair<MonitoringTarget, bool>(this.Type, this.IsSelected));
        }      
    }
}

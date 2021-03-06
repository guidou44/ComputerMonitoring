﻿using System.Collections.Generic;
using System.Windows.Input;
using Common.UI.Infrastructure;
using DesktopAssistant.BL.Hardware;

namespace DesktopAssistant.ViewModels
{
    public class MonitoringTargetViewModel : NotifyPropertyChanged
    {
        public delegate void SelectionChanged(KeyValuePair<MonitoringTarget, bool> kvp);
        public event SelectionChanged SelectionChangedEvent;

        public MonitoringTargetViewModel(MonitoringTarget type)
        {
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
            var kvp = new KeyValuePair<MonitoringTarget, bool>(this.Type, this.IsSelected);
            SelectionChangedEvent?.Invoke(kvp);
        }      
    }
}
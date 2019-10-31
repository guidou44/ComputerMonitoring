using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using ComputerRessourcesMonitoring.Events;
using ComputerRessourcesMonitoring.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace ComputerRessourcesMonitoring.ViewModels
{
    public class WatchdogSettingsDialogViewModel : ComputerMonitoringViewModelBase, IDialogRequestClose
    {
        #region Constructor

        private IEventAggregator _eventsHub;
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        public WatchdogSettingsDialogViewModel(string message, IEventAggregator eventsHub, bool usePerformanceCounter) : this(eventsHub, usePerformanceCounter)
        {
            WatchdogTargetName = message;
        }

        public WatchdogSettingsDialogViewModel(IEventAggregator eventsHub, bool usePerformanceCounter) 
        {
            var globalCpuUsage = PerformanceInfo.GetGlobalCpuUsage();
            CpuMake = globalCpuUsage.Cpu_Name + $" - {globalCpuUsage.Number_of_cores} cores";
            _eventsHub = eventsHub;
            UsePerformanceCounterForCpuUsage = usePerformanceCounter;
            RefreshMonitoring();
            SetMonitoringCounter(900);
        }

        #endregion


        #region Methods

        protected override void OnCounterCompletionEvent(Object source, ElapsedEventArgs e)
        {
            RefreshMonitoring();
        }

        protected override async void RefreshMonitoring()
        {
            CpuUsageCollection = new ObservableCollection<CpuUsage>(await Task.Run(() => PerformanceInfo.GetEachCpuUsage()));
        }

        #endregion


        #region Properties

        private ObservableCollection<CpuUsage> _cpuUsageCollection;

        public ObservableCollection<CpuUsage> CpuUsageCollection
        {
            get { return _cpuUsageCollection; }
            set 
            {
                _cpuUsageCollection = value;
                RaisePropertyChanged(nameof(CpuUsageCollection));
            }
        }

        private string _cpuMake;

        public string CpuMake
        {
            get { return _cpuMake; }
            set 
            { 
                _cpuMake = value;
                RaisePropertyChanged(nameof(CpuMake));
            }
        }

        private bool _usePerformanceCounterForCpuUsage;

        public bool UsePerformanceCounterForCpuUsage
        {
            get { return _usePerformanceCounterForCpuUsage; }
            set 
            { 
                _usePerformanceCounterForCpuUsage = value;
                RaisePropertyChanged(nameof(UsePerformanceCounterForCpuUsage));
                _eventsHub.GetEvent<OnUsePerformanceCounterChangedEvent>().Publish(_usePerformanceCounterForCpuUsage);
            }
        }


        private string _watchdogTargetName;

        public string WatchdogTargetName
        {
            get { return _watchdogTargetName; }
            set 
            { 
                _watchdogTargetName = value;
                RaisePropertyChanged(nameof(WatchdogTargetName));
            }
        }

        #endregion


        #region Commands

        public ICommand ChangeWatchdogTargetCommand
        {
            get { return new RelayCommand(ChangeWatchdogTargetCommandExecute, CanChangeWatchdogTargetCommandExecute); }
        }

        public void ChangeWatchdogTargetCommandExecute()
        {
            _eventsHub.GetEvent<OnWatchdogTargetChangedEvent>().Publish(WatchdogTargetName);
        }

        public bool CanChangeWatchdogTargetCommandExecute()
        {
            if (WatchdogTargetName?.Length > 0) return true;
            return false;
        }

        public ICommand DragWindowCommand
        {
            get { return new RelayCommand<IDragable>(DragWindowCommandExecute); }
        }

        public void DragWindowCommandExecute(IDragable dialog)
        {
            dialog.DragMove();
        }

        public ICommand OkCommand 
        {
            get { return new RelayCommand<WatchdogSettingsDialogViewModel>(p => CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true))); }
        }

        public ICommand CancelCommand
        {
            get { return new RelayCommand<WatchdogSettingsDialogViewModel>(p => CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(false))); }
        }

        #endregion

    }
}

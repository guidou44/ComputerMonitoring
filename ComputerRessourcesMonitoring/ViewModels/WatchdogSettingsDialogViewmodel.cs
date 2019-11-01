using Common.Reports;
using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using ComputerRessourcesMonitoring.Events;
using HardwareManipulation.HardwareInformation;
using HardwareManipulation.Models;
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
            var globalCpuUsage = CPUPerformanceInfo.GetGlobalCpuUsage();
            CpuMake = globalCpuUsage.Cpu_Name + $" - {globalCpuUsage.Number_of_cores} cores";
            GPUPerformanceInfo.InitializeGpuWatcher();

            _eventsHub = eventsHub;
            UsePerformanceCounterForCpuUsage = usePerformanceCounter;
            RefreshMonitoring();
            SetMonitoringCounter(1000);
        }
        ~WatchdogSettingsDialogViewModel()
        {
            GPUPerformanceInfo.ResetGpuWatcher();
            _monitoringRefreshCounter.Elapsed -= OnCounterCompletionEvent;
        }

        #endregion

        #region Events

        protected override void OnCounterCompletionEvent(Object source, ElapsedEventArgs e)
        {
            RefreshMonitoring();
        }

        #endregion


        #region Methods

        protected override async void RefreshMonitoring()
        {
                CpuUsageCollection = new ObservableCollection<CpuUsage>(await Task.Run(() => CPUPerformanceInfo.GetEachCpuUsage()));
                var gpuRequestResult = await Task.Run(() => GPUPerformanceInfo.GetGpuTemperature());
                if (gpuRequestResult != null)
                {
                    GpuUsageCollection = new ObservableCollection<GpuUsage>(gpuRequestResult);
                    GpuMake = GpuUsageCollection.FirstOrDefault().Name;
                }

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

        private string _gpuMake;

        public string GpuMake
        {
            get { return _gpuMake; }
            set 
            { 
                _gpuMake = value;
                RaisePropertyChanged(nameof(GpuMake));
            }
        }

        private ObservableCollection<GpuUsage> _gpuUsageCollection;

        public ObservableCollection<GpuUsage> GpuUsageCollection
        {
            get { return _gpuUsageCollection; }
            set
            {
                _gpuUsageCollection = value;
                RaisePropertyChanged(nameof(GpuUsageCollection));
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

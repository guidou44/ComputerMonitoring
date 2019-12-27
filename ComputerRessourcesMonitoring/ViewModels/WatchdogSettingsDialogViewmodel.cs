using Common.Reports;
using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using ComputerRessourcesMonitoring.Events;
using ComputerRessourcesMonitoring.Models;
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

        private Queue<MonitoringTarget> targetQueue;
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        public WatchdogSettingsDialogViewModel(string message, IEventAggregator eventsHub, 
                                               MonitoringTarget firstTarget,
                                               MonitoringTarget secondTarget) : this(eventsHub, firstTarget, secondTarget)
        {
            WatchdogTargetName = message;
        }

        public WatchdogSettingsDialogViewModel(IEventAggregator eventsHub, 
            MonitoringTarget firstTarget, MonitoringTarget secondTarget) : base()
        {
            var globalCpuUsage = CPUPerformanceInfo.GetCurrentGlobalCpuUsage();
            CpuMake = globalCpuUsage.Name + $" - {(globalCpuUsage as CpuUsage).Number_of_cores} cores";
            _eventHub = eventsHub;
            targetQueue = new Queue<MonitoringTarget>(2);
            targetQueue.Enqueue(firstTarget);
            targetQueue.Enqueue(secondTarget);

            GPUPerformanceInfo.InitializeGpuWatcher();
            SetCheckboxValues();
            RefreshMonitoring();
            SetMonitoringCounter(1500);
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

        private void AddTargetToQueue(MonitoringTarget target)
        {
            if (targetQueue.Contains(target)) return;
            if (targetQueue.Count() == 2) targetQueue.Dequeue();
            targetQueue.Enqueue(target);
        }

        private void RemoveTargetFromQueue(MonitoringTarget target)
        {
            if (!targetQueue.Contains(target)) return;
            var firstElementInQueue = targetQueue.Dequeue();
            if (firstElementInQueue == target) return;
            targetQueue.Dequeue();
            targetQueue.Enqueue(firstElementInQueue);
        }

        private async void SetCheckboxValues()
        {
            await Task.Run(() =>
            {
                _isMonitoringCpuUsage = targetQueue.Contains(MonitoringTarget.CPU_Usage) || targetQueue.Contains(MonitoringTarget.CPU_Usage_PC); RaisePropertyChanged(nameof(IsMonitoringCpuUsage));
                _isUsingPerfCountersForCpuUsage = IsMonitoringCpuUsage && targetQueue.Contains(MonitoringTarget.CPU_Usage_PC); RaisePropertyChanged(nameof(IsUsingPerfCountersForCpuUsage));
                _isMonitoringCpuTemp = targetQueue.Contains(MonitoringTarget.CPU_Temp); RaisePropertyChanged(nameof(IsMonitoringCpuTemp));
                _isMonitoringRamUsage = targetQueue.Contains(MonitoringTarget.RAM_Usage); RaisePropertyChanged(nameof(IsMonitoringRamUsage));
                _isMonitoringGpuUsage = targetQueue.Contains(MonitoringTarget.GPU_Usage); RaisePropertyChanged(nameof(IsMonitoringGpuUsage));
                _isMonitoringGpuTemp = targetQueue.Contains(MonitoringTarget.GPU_Temp); RaisePropertyChanged(nameof(IsMonitoringGpuTemp));
            });
        }

        private void SetTargetQueue(MonitoringTarget target, bool addToQueue)
        {
            if (addToQueue) AddTargetToQueue(target);
            else RemoveTargetFromQueue(target);
            SetCheckboxValues();
            if (targetQueue.Count() == 2) _eventHub.GetEvent<OnMonitoringTargetsChangedEvent>().Publish(new Queue<MonitoringTarget>(targetQueue));
            Console.WriteLine();
        }

        protected override async void RefreshMonitoring()
        {
            CpuUsageCollection = new ObservableCollection<CpuUsage>(await Task.Run(() => CPUPerformanceInfo.GetEachCpuUsage()));
            CpuTemperature = await Task.Run(() => CPUPerformanceInfo.GetCpuTemperature().Main_Value);

            var gpuRequestResult = await Task.Run(() => GPUPerformanceInfo.GetFirstGpuInformation());
            if (gpuRequestResult != null)
            {
                GpuUsageCollection = new ObservableCollection<GpuUsage>() { gpuRequestResult as GpuUsage };
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

        private double _cpuTemperature;

        public double CpuTemperature
        {
            get { return _cpuTemperature; }
            set 
            { 
                _cpuTemperature = value;
                RaisePropertyChanged(nameof(CpuTemperature));
            }
        }

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
            _eventHub.GetEvent<OnWatchdogTargetChangedEvent>().Publish(WatchdogTargetName);
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

        #endregion

        #region Checkbox_properties

        private bool _isMonitoringCpuUsage;

        public bool IsMonitoringCpuUsage
        {
            get { return _isMonitoringCpuUsage; }
            set 
            { 
                _isMonitoringCpuUsage = value;
                SetTargetQueue(MonitoringTarget.CPU_Usage, _isMonitoringCpuUsage);
                RaisePropertyChanged(nameof(IsMonitoringCpuUsage));
            }
        }

        private bool _isUsingPerfCountersForCpuUsage;

        public bool IsUsingPerfCountersForCpuUsage
        {
            get { return _isUsingPerfCountersForCpuUsage; }
            set
            {
                _isUsingPerfCountersForCpuUsage = value;
                SetTargetQueue(MonitoringTarget.CPU_Usage_PC, _isUsingPerfCountersForCpuUsage);
                RaisePropertyChanged(nameof(IsUsingPerfCountersForCpuUsage));
            }
        }

        private bool _isMonitoringCpuTemp;

        public bool IsMonitoringCpuTemp
        {
            get { return _isMonitoringCpuTemp; }
            set
            {
                _isMonitoringCpuTemp = value;
                SetTargetQueue(MonitoringTarget.CPU_Temp, _isMonitoringCpuTemp);
                RaisePropertyChanged(nameof(IsMonitoringCpuTemp));
            }
        }


        private bool _isMonitoringRamUsage;

        public bool IsMonitoringRamUsage
        {
            get { return _isMonitoringRamUsage; }
            set
            {
                _isMonitoringRamUsage = value;
                SetTargetQueue(MonitoringTarget.RAM_Usage, _isMonitoringRamUsage);
                RaisePropertyChanged(nameof(IsMonitoringRamUsage));
            }
        }

        private bool _isMonitoringGpuUsage;

        public bool IsMonitoringGpuUsage
        {
            get { return _isMonitoringGpuUsage; }
            set
            {
                _isMonitoringGpuUsage = value;
                SetTargetQueue(MonitoringTarget.GPU_Usage, _isMonitoringGpuUsage);
                RaisePropertyChanged(nameof(IsMonitoringGpuUsage));
            }
        }

        private bool _isMonitoringGpuTemp;

        public bool IsMonitoringGpuTemp
        {
            get { return _isMonitoringGpuTemp; }
            set
            {
                _isMonitoringGpuTemp = value;
                SetTargetQueue(MonitoringTarget.GPU_Temp, _isMonitoringGpuTemp);
                RaisePropertyChanged(nameof(IsMonitoringGpuTemp));
            }
        }

        #endregion
    }
}

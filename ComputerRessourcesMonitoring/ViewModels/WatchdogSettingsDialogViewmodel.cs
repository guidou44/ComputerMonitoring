using Common.Reports;
using Common.UI.CustomControls;
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
            InitializeMonitoringOptions();

            targetQueue = new Queue<MonitoringTarget>(2);
            targetQueue.Enqueue(firstTarget);
            targetQueue.Enqueue(secondTarget);
            _eventHub = eventsHub;
            _eventHub.GetEvent<OnMonitoringTargetSelectedEvent>().Subscribe(SetTargetQueue, true);

            SetCheckboxValues();
            RefreshMonitoring();
            SetMonitoringCounter(1500);
        }

        ~WatchdogSettingsDialogViewModel()
        {
            GPUPerformanceInfo.ResetGpuWatcher();
            foreach (var mvm in MonitoringOptionsCollection) mvm.SelectionChangedEvent -= SetTargetQueue;
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

        private void InitializeMonitoringOptions()
        {
            GPUPerformanceInfo.InitializeGpuWatcher();

            MonitoringOptionsCollection = new ObservableCollection<MonitoringTargetViewModel>();
            foreach (var option in Enum.GetValues(typeof(MonitoringTarget)).Cast<MonitoringTarget>())
            {
                if (option == MonitoringTarget.None) continue;
                var mvm = new MonitoringTargetViewModel(option) { DisplayName = option.ToString()};
                mvm.SelectionChangedEvent += SetTargetQueue;
                MonitoringOptionsCollection.Add(mvm);
            }
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
            await Task.Run(() => {
                foreach (var mvm in MonitoringOptionsCollection)
                {
                    mvm.IsSelected = targetQueue.Contains(mvm.Type);
                }
            });
        }

        private void SetTargetQueue(KeyValuePair<MonitoringTarget, bool> optionChanged)
        {
            if (optionChanged.Value) AddTargetToQueue(optionChanged.Key);
            else RemoveTargetFromQueue(optionChanged.Key);
            SetCheckboxValues();
            if (targetQueue.Count() == 2) _eventHub.GetEvent<OnMonitoringTargetsChangedEvent>().Publish(new Queue<MonitoringTarget>(targetQueue));
        }

        protected override async void RefreshMonitoring()
        {
            var gpuRequestResult = await Task.Run(() => GPUPerformanceInfo.GetFirstGpuInformation());
            if (gpuRequestResult != null)
            {
                GpuUsageCollection = new ObservableCollection<GpuUsage>() { gpuRequestResult as GpuUsage };
                GpuMake = GpuUsageCollection.FirstOrDefault().Name;
            }

        }


        #endregion


        #region Properties

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

        private ObservableCollection<MonitoringTargetViewModel> _monitoringOptionsCollection;
        public ObservableCollection<MonitoringTargetViewModel> MonitoringOptionsCollection
        {
            get { return _monitoringOptionsCollection; }
            set 
            { 
                _monitoringOptionsCollection = value;
                RaisePropertyChanged(nameof(MonitoringOptionsCollection));
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
            get { return new RelayCommand<WatchdogSettingsDialogViewModel>(OkCommandExecute); }
        }

        public void OkCommandExecute(WatchdogSettingsDialogViewModel wsdvm)
        {
            CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true));
            GPUPerformanceInfo.ResetGpuWatcher();
            foreach (var mvm in MonitoringOptionsCollection) mvm.SelectionChangedEvent -= SetTargetQueue;
            _monitoringRefreshCounter.Elapsed -= OnCounterCompletionEvent;
        }


        #endregion

    }
}

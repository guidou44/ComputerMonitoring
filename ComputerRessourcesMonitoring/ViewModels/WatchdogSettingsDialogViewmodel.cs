using Common.Reports;
using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using Common.UI.ViewModels;
using ComputerRessourcesMonitoring.Events;
using HardwareManipulation.Connectors;
using HardwareManipulation.Enums;
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
    public class WatchdogSettingsDialogViewModel : DialogViewModelBase
    {
        #region Constructor

        private IDictionary<MonitoringTarget, bool> targetDict;
        private MonitoringTarget _lruTarget;
        private IEventAggregator _eventHub;

        public WatchdogSettingsDialogViewModel(string watchdogTargetName, IEventAggregator eventsHub, MonitoringTarget firstMonTarget, MonitoringTarget secondMonTarget)
        {
            WatchdogTargetName = watchdogTargetName;
            _lruTarget = firstMonTarget;
            _eventHub = eventsHub;
            InitializeMonitoringOptions();
            SetMonitoringDictionary(new KeyValuePair<MonitoringTarget, bool>(firstMonTarget, true));
            SetMonitoringDictionary(new KeyValuePair<MonitoringTarget, bool>(secondMonTarget, true));
        }

        ~WatchdogSettingsDialogViewModel()
        {
            GPU_Connector.ResetGpuWatcher();
            foreach (var mvm in MonitoringOptionsCollection) mvm.SelectionChangedEvent -= SetMonitoringDictionary;
        }

        #endregion

        #region Methods

        private void InitializeMonitoringOptions()
        {
            GPU_Connector.InitializeGpuWatcher();
            MonitoringOptionsCollection = new ObservableCollection<MonitoringTargetViewModel>();
            targetDict = new Dictionary<MonitoringTarget, bool>();
            foreach (var option in Enum.GetValues(typeof(MonitoringTarget)).Cast<MonitoringTarget>())
            {
                if (option == MonitoringTarget.None) continue;
                targetDict.Add(option, false);
                var mvm = new MonitoringTargetViewModel(option) { DisplayName = option.ToString()};
                mvm.SelectionChangedEvent += SetMonitoringDictionary;
                MonitoringOptionsCollection.Add(mvm);
            }
        }

        private void SetCheckboxValues()
        {
            foreach (var monOption in MonitoringOptionsCollection)
            {
                monOption.IsSelected = targetDict[monOption.Type];
            }
        }

        private void SetMonitoringDictionary(KeyValuePair<MonitoringTarget, bool> target)
        {
            targetDict[target.Key] = target.Value;
            var currentTrueTargets = targetDict.Where(KVP => KVP.Value == true);
            if (currentTrueTargets.Count() > 2)
            {
                targetDict[_lruTarget] = false;
                _lruTarget = targetDict.Where(KVP => KVP.Value == true && KVP.Key != target.Key).SingleOrDefault().Key;
            }
            if (targetDict.Where(KVP => KVP.Value == true).Count() == 2)
            {
                var transfertQueue = new Queue<MonitoringTarget>();
                transfertQueue.Enqueue(_lruTarget);
                transfertQueue.Enqueue(targetDict.Where(KVP => KVP.Value == true && KVP.Key != _lruTarget).SingleOrDefault().Key);
                _eventHub.GetEvent<OnMonitoringTargetsChangedEvent>().Publish(new Queue<MonitoringTarget>(transfertQueue));
            }
            SetCheckboxValues();
        }

        #endregion


        #region Properties

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

        #endregion

    }
}

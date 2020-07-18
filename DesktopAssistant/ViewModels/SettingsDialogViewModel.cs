using Common.Reports;
using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.WindowProperty;
using Common.UI.ViewModels;
using DesktopAssistant.Events;
using Hardware;
using Hardware.Enums;
using Prism.Events;
using ProcessMonitoring;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Hardware;

namespace DesktopAssistant.ViewModels
{
    public class SettingsDialogViewModel : DialogViewModelBase
    {
        #region Constructor

        private DataManager _manager;
        private IEventAggregator _eventHub;
        private List<MonitoringTarget> _lruTargets;
        private IDictionary<MonitoringTarget, bool> targetDict;
        private ProcessWatchDog _watchdog;
        private Reporter _reporter;

        public SettingsDialogViewModel(ObservableCollection<ProcessViewModel> watchdogProcesses, IEventAggregator eventsHub, 
                                               List<MonitoringTarget> monTargets,
                                               DataManager manager, ProcessWatchDog watchdog, Reporter reporter)
        {
            _reporter = reporter;
            _watchdog = watchdog;
            ProcessesUnderWatch = new ObservableCollection<ProcessViewModel>(watchdogProcesses);
            foreach (ProcessViewModel processUnderWatch in ProcessesUnderWatch)
            {
                processUnderWatch.OnProcessNameChangedEvent += OnWatchdogTargetChanged;
                processUnderWatch.OnProcessWatchRemoveEvent += OnWatchdogRemoveTarget;
            }


            MaxAllowedMonTargets = monTargets.Count();
            _eventHub = eventsHub;
            _manager = manager;
            _lruTargets = new List<MonitoringTarget>();
            InitializeComponents(monTargets);
        }

        #endregion

        #region Monitoring Methods

        private void InitializeComponents(List<MonitoringTarget> monTargets)
        {
            MotherBoardMake = (string)_manager.GetCalculatedValue(MonitoringTarget.Mother_Board_Make).MainValue;
            CpuMake = (string) _manager.GetCalculatedValue(MonitoringTarget.CPU_Make).MainValue;
            GpuMake = (string)_manager.GetCalculatedValue(MonitoringTarget.GPU_Make).MainValue;

            MonitoringOptionsCollection = new ObservableCollection<MonitoringTargetViewModel>();
            targetDict = new Dictionary<MonitoringTarget, bool>();

            IEnumerable<MonitoringTarget> targetOptions;
            if (_manager.IsRemoteMonitoringEnabled()) targetOptions = _manager.GetAllTargets();
            else targetOptions = _manager.GetLocalTargets();

            foreach (var targetOption in targetOptions)
            {
                if (targetOption == MonitoringTarget.None) continue;
                targetDict.Add(targetOption, false);
                var mvm = new MonitoringTargetViewModel(targetOption) { DisplayName = targetOption.ToString().Replace("_", " ")};
                mvm.SelectionChangedEvent += SetMonitoringDictionary;
                MonitoringOptionsCollection.Add(mvm);
            }

            foreach (var target in monTargets)
            {
                SetMonitoringDictionary(new KeyValuePair<MonitoringTarget, bool>(target, true));
            }
        }

        private void PublishQueue(List<MonitoringTarget> lruTargets)
        {
            _eventHub.GetEvent<OnMonitoringTargetsChangedEvent>().Publish(lruTargets);
        }

        private void SetCheckboxValues()
        {
            foreach(var monOption in MonitoringOptionsCollection)
            {
                monOption.IsSelected = targetDict[monOption.Type];
            }
        }

        private void SetMonitoringDictionary(KeyValuePair<MonitoringTarget, bool> target)
        {
            targetDict[target.Key] = target.Value;
            if (target.Value) _lruTargets.Add(target.Key);
            else if (!target.Value && _lruTargets.Contains(target.Key))
            {
                _lruTargets.Remove(target.Key);
            }

            while (_lruTargets.Count() > MaxAllowedMonTargets)
            {
                MonitoringTarget lruTarget = _lruTargets.First();
                if (_lruTargets.Count() >= 1)
                    _lruTargets.RemoveAt(0);
                targetDict[lruTarget] = false;
            }
            SetCheckboxValues();
            if (_lruTargets.Count() == MaxAllowedMonTargets) PublishQueue(_lruTargets);
        }

        #endregion

        #region Watchdog Methods

        private void OnWatchdogRemoveTarget(object sender, EventArgs e)
        {
            var processVM = ProcessesUnderWatch.Where(PUW => PUW == (ProcessViewModel)sender).SingleOrDefault();
            processVM.OnProcessWatchRemoveEvent -= OnWatchdogRemoveTarget;
            ProcessesUnderWatch.Remove(processVM);
            _eventHub.GetEvent<OnWatchdogTargetChangedEvent>().Publish(ProcessesUnderWatch);
        }

        private void OnWatchdogTargetChanged(object sender, EventArgs e)
        {
            var processVM = ProcessesUnderWatch.Where(PUW => PUW == (ProcessViewModel)sender).SingleOrDefault();
            processVM.Process = _watchdog.GetProcessesByName(processVM.ProcessName).FirstOrDefault();
            _eventHub.GetEvent<OnWatchdogTargetChangedEvent>().Publish(ProcessesUnderWatch);
        }

        #endregion

        #region Properties

        private bool _canRemoveWatchdogTargets;
        public bool CanRemoveWatchdogTargets
        {
            get { return _canRemoveWatchdogTargets; }
            set 
            { 
                _canRemoveWatchdogTargets = value;
                RaisePropertyChanged(nameof(CanRemoveWatchdogTargets));
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

        private string _motherBoardMake;

        public string MotherBoardMake
        {
            get { return _motherBoardMake; }
            set 
            { 
                _motherBoardMake = value;
                RaisePropertyChanged(nameof(MotherBoardMake));
            }
        }


        private int _maxAllowedMonTargets;
        public int MaxAllowedMonTargets
        {
            get { return _maxAllowedMonTargets; }
            set
            { 
                _maxAllowedMonTargets = value;
                RaisePropertyChanged(nameof(MaxAllowedMonTargets));
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

        private ObservableCollection<ProcessViewModel> _processesUnderWatch;
        public ObservableCollection<ProcessViewModel> ProcessesUnderWatch
        {
            get { return _processesUnderWatch; }
            set 
            { 
                _processesUnderWatch = value;
                RaisePropertyChanged(nameof(ProcessesUnderWatch));
            }
        }

        #endregion

        #region Commands

        public ICommand AddToWatchdogCollectionCommand
        {
            get { return new RelayCommand((() => 
            {
                var newProcessVm = new ProcessViewModel(true, "#NAME# ENTER2APPLY", _reporter);
                newProcessVm.OnProcessNameChangedEvent += OnWatchdogTargetChanged;
                newProcessVm.OnProcessWatchRemoveEvent += OnWatchdogRemoveTarget;
                ProcessesUnderWatch.Add(newProcessVm); 
            }), 
            (() => { return ProcessesUnderWatch.Count() < 7 && !CanRemoveWatchdogTargets; })); }        
        }

        public ICommand RemoveFromWatchdogCollectionCommand
        {
            get { return new RelayCommand((() => 
            { 
                ProcessesUnderWatch.ToList().ForEach(PUW => PUW.CanRemoveProcessWatch = true);
                CanRemoveWatchdogTargets = true;
            }), 
                (() => { return ProcessesUnderWatch.Count() > 0; })); }
        }

        public ICommand StopRemovingWatchdogProcessCommand
        {
            get { return new RelayCommand(() =>
            {
                ProcessesUnderWatch.ToList().ForEach(PUW => PUW.CanRemoveProcessWatch = false);
                CanRemoveWatchdogTargets = false;
            }); }
        }
        
        #endregion

    }
}

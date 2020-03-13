using Common.Reports;
using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using Common.UI.ViewModels;
using ComputerRessourcesMonitoring.Events;
using HardwareAccess;
using HardwareAccess.Enums;
using Prism.Events;
using ProcessMonitoring;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HardwareManipulation;

namespace ComputerRessourcesMonitoring.ViewModels
{
    public class SettingsDialogViewModel : DialogViewModelBase
    {
        #region Constructor

        private DataManager _manager;
        private IEventAggregator _eventHub;
        private Queue<MonitoringTarget> _lruTargets;
        private IDictionary<MonitoringTarget, bool> targetDict;
        private ProcessWatchDog _watchdog;

        public SettingsDialogViewModel(ObservableCollection<ProcessViewModel> watchdogProcesses, IEventAggregator eventsHub, 
                                               Queue<MonitoringTarget> monTargets,
                                               DataManager manager, ProcessWatchDog watchdog)
        {
            //watchdog---------
            _watchdog = watchdog;
            ProcessesUnderWatch = new ObservableCollection<ProcessViewModel>(watchdogProcesses);
            ProcessesUnderWatch.ToList().ForEach(PUW => 
            { 
                PUW.OnProcessNameChangedEvent += OnWatchdogTargetChanged;
                PUW.OnProcessWatchRemoveEvent += OnWatchdogRemoveTarget;
            });

            //Monitoring-------
            MaxAllowedMonTargets = monTargets.Count();
            _eventHub = eventsHub;
            _lruTargets = monTargets;
            _manager = manager;
            InitializeComponents(monTargets);
        }

        ~SettingsDialogViewModel()
        {
            MonitoringOptionsCollection.ToList().ForEach(MVM => MVM.SelectionChangedEvent -= SetMonitoringDictionary);
            ProcessesUnderWatch.ToList().ForEach(PUW => PUW.OnProcessNameChangedEvent -= OnWatchdogTargetChanged);
        }

        #endregion

        #region Monitoring Methods

        private async void InitializeComponents(Queue<MonitoringTarget> monTargets)
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

            for (int i = 0; i < _lruTargets.Count(); i++)
            {
                SetMonitoringDictionary(new KeyValuePair<MonitoringTarget, bool>(monTargets.Dequeue(), true));
            }
        }

        private async void PublishQueue(Queue<MonitoringTarget> lruTargets)
        {
            await Task.Run(() =>_eventHub.GetEvent<OnMonitoringTargetsChangedEvent>().Publish(lruTargets));
        }

        private async void SetCheckboxValues()
        {
            foreach(var monOption in MonitoringOptionsCollection)
            {
                await Task.Run(() => monOption.IsSelected = targetDict[monOption.Type]);
            }
        }

        private void SetMonitoringDictionary(KeyValuePair<MonitoringTarget, bool> target)
        {
            targetDict[target.Key] = target.Value;
            if (target.Value) _lruTargets.Enqueue(target.Key);
            else if (!target.Value && _lruTargets.Contains(target.Key))
            {
                var lruTargetsList = _lruTargets.ToList();
                lruTargetsList.Remove(target.Key);
                _lruTargets = new Queue<MonitoringTarget>(lruTargetsList);
            }

            while (_lruTargets.Count() > MaxAllowedMonTargets)
            {
                var lruTarget = _lruTargets.Dequeue();
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
                var newProcessVm = new ProcessViewModel(true, "#NAME# ENTER2APPLY");
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

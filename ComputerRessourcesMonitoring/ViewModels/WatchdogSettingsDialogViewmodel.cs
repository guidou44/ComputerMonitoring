using Common.Reports;
using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using Common.UI.ViewModels;
using ComputerRessourcesMonitoring.Events;
using HardwareManipulation;
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

        private DataManager _manager;
        private IEventAggregator _eventHub;
        private Queue<MonitoringTarget> _lruTargets;
        private IDictionary<MonitoringTarget, bool> targetDict;

        private IEnumerable<ProcessViewModel> _watchdogProcesses;

        public WatchdogSettingsDialogViewModel(IEnumerable<ProcessViewModel> watchdogProcesses, IEventAggregator eventsHub, 
                                               Queue<MonitoringTarget> monTargets,
                                               DataManager manager)
        {
            //watchdog---------
            _watchdogProcesses = watchdogProcesses;
            ProcessesUnderWatchNames = new ObservableCollection<ProcessChangerViewModel>();
            foreach (var process in watchdogProcesses)
            {
                var processChanger = new ProcessChangerViewModel(process.Process.ProcessName);
                processChanger.OnProcessNameChangedEvent += OnWatchdogTargetChanged;
                ProcessesUnderWatchNames.Add(processChanger);
            }

            //Monitoring-------
            MaxAllowedMonTargets = monTargets.Count();
            _eventHub = eventsHub;
            _lruTargets = monTargets;
            _manager = manager;
            InitializeComponents(monTargets);
        }

        ~WatchdogSettingsDialogViewModel()
        {
            foreach (var mvm in MonitoringOptionsCollection) mvm.SelectionChangedEvent -= SetMonitoringDictionary;
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

        private void OnWatchdogTargetChanged(string targetName)
        {
            var oldProcessVM = _watchdogProcesses.Where(WP => WP.Process.ProcessName == )
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

        private ObservableCollection<ProcessChangerViewModel> _processesUnderWatchNames;

        public ObservableCollection<ProcessChangerViewModel> ProcessesUnderWatchNames
        {
            get { return _processesUnderWatchNames; }
            set 
            { 
                _processesUnderWatchNames = value;
                RaisePropertyChanged(nameof(ProcessesUnderWatchNames));
            }
        }


        #endregion

    }
}

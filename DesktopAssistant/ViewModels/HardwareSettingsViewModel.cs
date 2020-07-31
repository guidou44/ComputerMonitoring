using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Common.Reports;
using Common.UI.Infrastructure;
using Common.UI.ViewModels;
using DesktopAssistant.BL;
using DesktopAssistant.BL.Events;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.BL.ProcessWatch;
using Prism.Events;

namespace DesktopAssistant.ViewModels
{
    public class HardwareSettingsViewModel : DialogViewModelBase
    {
        #region Constructor

        private readonly IAppManager _manager;
        private readonly IEventAggregator _eventHub;
        private readonly List<MonitoringTarget> _lruTargets;
        private IDictionary<MonitoringTarget, bool> _targetDict;

        public HardwareSettingsViewModel(IEventAggregator eventsHub, IAppManager manager)
        {
            _manager = manager;
            MaxAllowedMonTargets = _manager.GetMonitoringQueue().Count();
            _eventHub = eventsHub;
            _lruTargets = new List<MonitoringTarget>();
            InitializeMonitoringTargets(_manager.GetMonitoringQueue());
        }

        #endregion

        #region Monitoring Methods

        private void InitializeMonitoringTargets(List<MonitoringTarget> monTargets)
        {
            MotherBoardMake = (string)_manager.GetCalculatedValue(MonitoringTarget.Mother_Board_Make).MainValue;
            CpuMake = (string) _manager.GetCalculatedValue(MonitoringTarget.CPU_Make).MainValue;
            GpuMake = (string)_manager.GetCalculatedValue(MonitoringTarget.GPU_Make).MainValue;

            MonitoringOptionsCollection = new ObservableCollection<MonitoringTargetViewModel>();
            _targetDict = new Dictionary<MonitoringTarget, bool>();

            IEnumerable<MonitoringTarget> targetOptions = _manager.GetAllTargets();

            foreach (var targetOption in targetOptions)
            {
                if (targetOption == MonitoringTarget.None) continue;
                _targetDict.Add(targetOption, false);
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

        private void SetMonitoringDictionary(KeyValuePair<MonitoringTarget, bool> target)
        {
            _targetDict[target.Key] = target.Value;
            if (target.Value) _lruTargets.Add(target.Key);
            else if (!target.Value && _lruTargets.Contains(target.Key))
            {
                _lruTargets.Remove(target.Key);
            }

            while (_lruTargets.Count() > MaxAllowedMonTargets)
            {
                MonitoringTarget lruTarget = _lruTargets.First();
                if (_lruTargets.Any())
                    _lruTargets.RemoveAt(0);
                _targetDict[lruTarget] = false;
            }
            SetCheckboxValues();
            if (_lruTargets.Count() == MaxAllowedMonTargets) 
                PublishQueue(_lruTargets);
        }
        
        private void SetCheckboxValues()
        {
            foreach(var monOption in MonitoringOptionsCollection)
            {
                monOption.IsSelected = _targetDict[monOption.Type];
            }
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

        #endregion
        
    }
}

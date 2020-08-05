using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using Common.Reports;
using Common.UI.Models;
using DesktopAssistant.BL.Events;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.BL.Persistence;
using DesktopAssistant.BL.ProcessWatch;
using Prism.Events;

namespace DesktopAssistant.BL
{
    public sealed class ComputerMonitoringManager : AppManagerModelBase, IDisposable, IPacketObserver, IAppManager
    {
        private const int TimerRefreshRateMilli = 900;

        private readonly IHardwareManager _hardwareManager;
        private readonly ITimer _monitoringRefreshCounter;
        private readonly IRepository _repository;
        private readonly IProcessWatcher _processWatcher;

        private IManagerObserver _observer;
        private ICollection<IHardwareInfo> _hardwareValues;
        private List<MonitoringTarget> _monitoringTargets;
        private IEnumerable<IProcessWatch> _processesUnderWatch;

        public ComputerMonitoringManager(IEventAggregator eventHub, 
                                      IHardwareManager hardwareManager, 
                                      IProcessWatcher watchDog,
                                      IRepository repository,
                                      ITimer refreshCounter) : base(eventHub)
        {
            _monitoringRefreshCounter = refreshCounter;
            _hardwareManager = hardwareManager;
            _monitoringTargets = new List<MonitoringTarget>();
            _processWatcher = watchDog;
            _repository = repository;

            SetInitialMonitoringTargets();
            SubscribeToEvents();
        }

        public void Start()
        {
            RefreshMonitoring();
            SetMonitoringCounter(TimerRefreshRateMilli);
            StartProcessWatch();
        }
        
        public void RegisterManagerObserver(IManagerObserver observer)
        {
            _observer = observer;
        }

        public void Dispose()
        {
            _monitoringRefreshCounter.Stop();
            _processWatcher.StopCapture();
        }

        public void OnPacketCapture(PacketData data)
        {
            //manage the data received
        }

        private void SetInitialMonitoringTargets()
        {
            IEnumerable<MonitoringTarget> initialTargets = _hardwareManager.GetInitialTargets();
            initialTargets.ToList().ForEach(target => _monitoringTargets.Add(target));
        }

        private void StartProcessWatch()
        {
            _processWatcher.RegisterPacketCaptureObserver(this);
            ProcessesUnderWatch = _processWatcher.GetProcessUnderWatch();
            _processWatcher.StartCapture();
        }

        public List<MonitoringTarget> GetMonitoringQueue()
        {
            return _monitoringTargets;
        }

        public IHardwareInfo GetCalculatedValue(MonitoringTarget monTarget)
        {
            return _hardwareManager.GetCalculatedValue(monTarget);
        }

        public ICollection<MonitoringTarget> GetAllTargets()
        {
            return _hardwareManager.GetAllTargets();
        }

        private void OnCounterCompletionEvent(Object source, ElapsedEventArgs e)
        {
            RefreshMonitoring();
        }

        private void RefreshMonitoring()
        {
            try
            {
                IEnumerable<IHardwareInfo> valuesQueue = _hardwareManager.GetCalculatedValues(_monitoringTargets);
                HardwareValues = new ObservableCollection<IHardwareInfo>(valuesQueue);
            }
            catch (Exception e)
            {
                _repository.Update(e);
                _observer.OnError(e);
            }
        }

        private void SetMonitoringCounter(int counterTimeMilliseconds)
        {
            _monitoringRefreshCounter.Init(counterTimeMilliseconds);
            _monitoringRefreshCounter.Elapsed += OnCounterCompletionEvent;
            _monitoringRefreshCounter.AutoReset = true;
            _monitoringRefreshCounter.Start();
        }

        private void SubscribeToEvents()
        {
            EventHub.GetEvent<OnWatchdogTargetChangedEvent>().Subscribe(UpdateProcessWatch);
            EventHub.GetEvent<OnMonitoringTargetsChangedEvent>().Subscribe((targets) => { _monitoringTargets = targets; });
        }

        private void UpdateProcessWatch(IEnumerable<IProcessWatch> processWatches)
        {
            IEnumerable<IProcessWatch> processToRemove = ProcessesUnderWatch.Except(processWatches);
            IEnumerable<IProcessWatch> processToAdd = processWatches.Except(ProcessesUnderWatch);
            IEnumerable<IProcessWatch> processToUpdate = ProcessesUnderWatch.Union(processWatches);
            processToAdd.ToList().ForEach(p => _processWatcher.AddProcessToWatchList(p.ProcessName, p.DoCapture));
            processToRemove.ToList().ForEach(p => _processWatcher.RemoveProcessFromWatchList(p.ProcessName));
            processToUpdate.ToList().ForEach(p => _processWatcher.UpdateProcessCaptureInWatchList(p.ProcessName, p.DoCapture));
            ProcessesUnderWatch = _processWatcher.GetProcessUnderWatch();
        }
        
        
        
        public ICollection<IHardwareInfo> HardwareValues
        {
            get { return _hardwareValues; }
            set
            {
                _hardwareValues = value;
                _observer?.OnHardwareInfoChange();
            }
        }

        public IEnumerable<IProcessWatch> ProcessesUnderWatch
        {
            get { return _processesUnderWatch; }
            set
            {
                _processesUnderWatch = value;
                _observer?.OnProcessWatchInfoChange();
            }
        }
    }
}

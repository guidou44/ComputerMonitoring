using Common.Reports;
using Common.UI.Infrastructure;
using Common.UI.Models;
using ComputerRessourcesMonitoring.Events;
using ComputerRessourcesMonitoring.ViewModels;
using HardwareAccess;
using HardwareAccess.Enums;
using HardwareAccess.Models;
using Prism.Events;
using ProcessMonitoring;
using ProcessMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using HardwareManipulation;
using System.Diagnostics;

namespace ComputerResourcesMonitoring.Models
{
    public class ComputerMonitoringManagerModel : AppManagerModelBase, IDisposable
    {
        #region Constructor

        private DataManager _hardwareManager;
        private List<MonitoringTarget> _monitoringTargets;
        private System.Timers.Timer _monitoringRefreshCounter;
        private ProcessWatchDog _watchdog;
        private Reporter _reporter;
        private Thread _watchdogThread;

        public delegate void MonitoringErrorOccuredEventHandler(Exception e);
        public event MonitoringErrorOccuredEventHandler OnMonitoringErrorOccured;

        public ComputerMonitoringManagerModel(IEventAggregator eventHub, 
                                              DataManager hardwareManager, 
                                              ProcessWatchDog watchDog,
                                              Reporter reporter) : base(eventHub)
        {
            _hardwareManager = hardwareManager;
            _monitoringTargets = new List<MonitoringTarget>();
            _watchdog = watchDog;
            _reporter = reporter;
            var initialTargets = _hardwareManager.GetInitialTargets();
            initialTargets.ToList().ForEach(TARGET => _monitoringTargets.Add(TARGET));
            InitializeWatchdog();
            RefreshMonitoring();
            SubscribeToEvents();
            SetMonitoringCounter(900);
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            _watchdog.PacketsExchangedEvent -= ReportPacketExchange;
            _monitoringRefreshCounter.Stop();
            _monitoringRefreshCounter.Dispose();
        }

        public DataManager GetHardwareManager()
        {
            return _hardwareManager;
        }

        public ProcessWatchDog GetWatchDog()
        {
            return _watchdog;
        }

        public List<MonitoringTarget> GetMonitoringQueue()
        {
            return _monitoringTargets;
        }

        #endregion

        #region Private Methods

        private void InitializeWatchdog()
        {           
            try
            {
                _watchdog.PacketsExchangedEvent += ReportPacketExchange;
                ProcessesUnderWatch = new ObservableCollection<ProcessViewModel>();
                foreach (var initialProcess in _watchdog.GetInitialProcesses2Watch())
                {
                    var process = _watchdog.GetProcessesByName(initialProcess).FirstOrDefault();
                    var processVM = new ProcessViewModel(true, initialProcess, _reporter)
                    {
                        Process = process,
                    };
                    ProcessesUnderWatch.Add(processVM);
                }

                Thread _watchdogThread = new Thread(() =>
                {
                    while (true)
                    {
                        ManageWatchdog();
                        Thread.Sleep(1000);
                    }
                });
                _watchdogThread.Start();
            }
            catch (Exception e)
            {
                _watchdog.PacketsExchangedEvent -= ReportPacketExchange;
                _watchdogThread.Abort();
            }
        }

        private void ManageWatchdog()
        {
            foreach (var process2watch in ProcessesUnderWatch)
            {
                process2watch.IsRunning = _watchdog.IsProcessCurrentlyRunning(process2watch.ProcessName);
                if (process2watch.IsRunning && process2watch.Check4PacketExchange)
                {
                    if (process2watch.Process == null) process2watch.Process = _watchdog.GetProcessesByName(process2watch.ProcessName).FirstOrDefault();
                    if (!process2watch.WasInitialized)
                    {
                        _watchdog.InitializeWatchdogForProcess(process2watch.Process);
                        process2watch.WasInitialized = true;
                    }
                    _watchdog.RefreshInfo();
                }
            }
            RaisePropertyChanged(nameof(ProcessesUnderWatch));
        }

        private void OnCounterCompletionEvent(Object source, ElapsedEventArgs e)
        {
            RefreshMonitoring();
        }

        private void RefreshMonitoring()
        {
            try
            {
                var valuesQueue = _hardwareManager.GetCalculatedValues(_monitoringTargets);
                HardwareValues = new ObservableCollection<HardwareInformation>(valuesQueue);
            }
            catch (Exception e)
            {
                _reporter.LogException(e);
                OnMonitoringErrorOccured?.Invoke(e);
            }
        }

        private async void ReportPacketExchange(PacketCaptureProcessInfo guiltyProcessInformation)
        {
            await Task.Run(() => _reporter.SendEmailReport(
                            subject: $"ALARM: Detected Activity for {guiltyProcessInformation.Process.ProcessName}",
                            message: $"Activity detected report:\n" +
                                        $"----------------{guiltyProcessInformation.Process.ProcessName}---------------\n\n" +
                                        "DateTime: " + DateTime.Now.ToString("dd/MM/yyyy H:mm:ss") + "\n\nContent:\n" +
                                        $"Net send bytes : {guiltyProcessInformation.NetSendBytes}\n" +
                                        $"Net Received bytes : {guiltyProcessInformation.NetRecvBytes}\n" +
                                        $"Net Total bytes: {guiltyProcessInformation.NetTotalBytes}\n"
                            ));
        }

        private void SetMonitoringCounter(int counterTimeMilliseconds)
        {
            _monitoringRefreshCounter = new System.Timers.Timer(counterTimeMilliseconds);
            _monitoringRefreshCounter.Elapsed += OnCounterCompletionEvent;
            _monitoringRefreshCounter.AutoReset = true;
            _monitoringRefreshCounter.Enabled = true;
        }

        private void SubscribeToEvents()
        {
            _eventHub.GetEvent<OnWatchdogTargetChangedEvent>().Subscribe((processesToWatch) => { ProcessesUnderWatch = new ObservableCollection<ProcessViewModel>(processesToWatch); });
            _eventHub.GetEvent<OnMonitoringTargetsChangedEvent>().Subscribe((targets) => { _monitoringTargets = targets; RefreshMonitoring(); });
        }

        #endregion

        #region Properties

        private ICollection<HardwareInformation> _hardwareValues;
        public ICollection<HardwareInformation> HardwareValues
        {
            get { return _hardwareValues; }
            set
            {
                _hardwareValues = value;
                RaisePropertyChanged(nameof(HardwareValues));
            }
        }

        private ICollection<ProcessViewModel> _processesUnderWatch;
        public ICollection<ProcessViewModel> ProcessesUnderWatch
        {
            get { return _processesUnderWatch; }
            set
            {
                _processesUnderWatch = value;
                RaisePropertyChanged(nameof(ProcessesUnderWatch));
            }
        }

        #endregion
    }
}

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
using ComputerRessourcesMonitoring.Models;
using static ProcessMonitoring.ProcessWatchDog;

namespace ComputerResourcesMonitoring.Models
{
    public class ComputerMonitoringManagerModel : AppManagerModelBase, IDisposable
    {
        private const int TIMER_REFRESH_RATE_MILLI = 900;

        private DataManager _hardwareManager;
        private List<MonitoringTarget> _monitoringTargets;
        private ITimer _monitoringRefreshCounter;
        private ProcessWatchDog _watchdog;
        private Reporter _reporter;
        private IThread _watchdogThread;

        public bool IsWatchdogRunning = true;

        public delegate void MonitoringErrorOccuredEventHandler(Exception e);
        public virtual event MonitoringErrorOccuredEventHandler OnMonitoringErrorOccured;

        public ComputerMonitoringManagerModel(IEventAggregator eventHub, 
                                              DataManager hardwareManager, 
                                              ProcessWatchDog watchDog,
                                              Reporter reporter,
                                              ITimer refreshCounter,
                                              IThread watchdogThread) : base(eventHub)
        {
            _watchdogThread = watchdogThread;
            _monitoringRefreshCounter = refreshCounter;
            _hardwareManager = hardwareManager;
            _monitoringTargets = new List<MonitoringTarget>();
            _watchdog = watchDog;
            _reporter = reporter;
            IEnumerable<MonitoringTarget> initialTargets = _hardwareManager.GetInitialTargets();
            initialTargets.ToList().ForEach(TARGET => _monitoringTargets.Add(TARGET));
            InitializeWatchdog();
            RefreshMonitoring();
            SubscribeToEvents();
            SetMonitoringCounter(TIMER_REFRESH_RATE_MILLI);
        }

        public virtual void Dispose()
        {
            _watchdog.PacketsExchangedEvent -= ReportPacketExchange;
            _monitoringRefreshCounter.Stop();
        }

        public virtual DataManager GetHardwareManager()
        {
            return _hardwareManager;
        }

        public virtual ProcessWatchDog GetWatchDog()
        {
            return _watchdog;
        }

        public virtual List<MonitoringTarget> GetMonitoringQueue()
        {
            return _monitoringTargets;
        }

        private void InitializeWatchdog()
        {           
            try
            {
                _watchdog.PacketsExchangedEvent += new PacketsExchanged(ReportPacketExchange);
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

                _watchdogThread.SetJob(() =>
                {
                    while (IsWatchdogRunning)
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
                        PacketCaptureProcessInfo captureInfo = new PacketCaptureProcessInfo(process2watch.Process);
                        _watchdog.InitializeWatchdogForProcess(captureInfo);
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
                IEnumerable<HardwareInformation> valuesQueue = _hardwareManager.GetCalculatedValues(_monitoringTargets);
                HardwareValues = new ObservableCollection<HardwareInformation>(valuesQueue);
            }
            catch (Exception e)
            {
                _reporter.LogException(e);
                OnMonitoringErrorOccured?.Invoke(e);
            }
        }

        private void ReportPacketExchange(PacketCaptureProcessInfo guiltyProcessInformation)
        {
            _reporter.SendEmailReport(
                            subject: $"ALARM: Detected Activity for {guiltyProcessInformation.ProcessName}",
                            message: $"Activity detected report:\n" +
                                        $"----------------{guiltyProcessInformation.ProcessName}---------------\n\n" +
                                        "DateTime: " + DateTime.Now.ToString("dd/MM/yyyy H:mm:ss") + "\n\nContent:\n" +
                                        $"Net send bytes : {guiltyProcessInformation.NetSendBytes}\n" +
                                        $"Net Received bytes : {guiltyProcessInformation.NetRecvBytes}\n" +
                                        $"Net Total bytes: {guiltyProcessInformation.NetTotalBytes}\n"
                            );
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
            _eventHub.GetEvent<OnWatchdogTargetChangedEvent>().Subscribe((processesToWatch) => { ProcessesUnderWatch = new ObservableCollection<ProcessViewModel>(processesToWatch); });
            _eventHub.GetEvent<OnMonitoringTargetsChangedEvent>().Subscribe((targets) => { _monitoringTargets = targets; RefreshMonitoring(); });
        }

        private ICollection<HardwareInformation> _hardwareValues;
        public virtual ICollection<HardwareInformation> HardwareValues
        {
            get { return _hardwareValues; }
            set
            {
                _hardwareValues = value;
                RaisePropertyChanged(nameof(HardwareValues));
            }
        }

        private ICollection<ProcessViewModel> _processesUnderWatch;
        public virtual ICollection<ProcessViewModel> ProcessesUnderWatch
        {
            get { return _processesUnderWatch; }
            set
            {
                _processesUnderWatch = value;
                RaisePropertyChanged(nameof(ProcessesUnderWatch));
            }
        }
    }
}

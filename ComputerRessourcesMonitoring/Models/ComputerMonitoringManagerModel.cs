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

namespace ComputerRessourcesMonitoring.Models
{
    public class ComputerMonitoringManagerModel : AppManagerModelBase, IDisposable
    {
        #region Constructor

        private DataManager _manager;
        private Queue<MonitoringTarget> _monitoringTargets;
        private System.Timers.Timer _monitoringRefreshCounter;
        private ProcessWatchDog _watchdog;
        private Thread _watchdogThread;

        public ComputerMonitoringManagerModel(IEventAggregator eventHub) : base(eventHub)
        {
            _manager = new DataManager();
            _monitoringTargets = new Queue<MonitoringTarget>();
            var initialTargets = _manager.GetInitialTargets();
            initialTargets.ToList().ForEach(TARGET => _monitoringTargets.Enqueue(TARGET));
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            _watchdog.PacketsExchangedEvent -= ReportPacketExchange;
            _monitoringRefreshCounter.Stop();
            _monitoringRefreshCounter.Dispose();
        }

        #endregion

        #region Private Methods

        private void InitializeWatchdog()
        {
            _watchdog = new ProcessWatchDog();
            _watchdog.PacketsExchangedEvent += ReportPacketExchange;
            ProcessesUnderWatch = new ObservableCollection<ProcessViewModel>();
            foreach (var initialProcess in _watchdog.GetInitialProcesses2Watch())
            {
                var process = _watchdog.GetProcessesByName(initialProcess).FirstOrDefault();
                var processVM = new ProcessViewModel(true, initialProcess)
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
        }

        private void OnCounterCompletionEvent(Object source, ElapsedEventArgs e)
        {
            RefreshMonitoring();
        }

        private void RefreshMonitoring()
        {
            try
            {

                var valuesQueue = _manager.GetCalculatedValues(_monitoringTargets);
                HardwareValues = new ObservableCollection<HardwareInformation>(valuesQueue);
            }
            catch (Exception e)
            {
                Reporter.LogException(e);
                _dialogService.ShowException(e);
            }
        }

        private async void ReportPacketExchange(PacketCaptureProcessInfo guiltyProcessInformation)
        {
            await Task.Run(() => Reporter.SendEmailReport(
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

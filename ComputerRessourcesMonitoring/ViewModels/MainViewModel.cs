using Common.DialogServices;
using Common.Reports;
using Common.UI;
using Common.UIInterfaces;
using ComputerRessourcesMonitoring.Events;
using ComputerRessourcesMonitoring.Models;
using Prism.Events;
using ProcessMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;


/*TODO: 
Add all Cpu information on watchdog settings page (In list box or something like this). Also add clock speed overall, voltage, usage of each, etc...
Add ram, Add network activiy, the most the better!
See how to use the dispatcher class to begin invoke the close action for the wndow in order for the app to close appropriateley
release
 */



namespace ComputerRessourcesMonitoring.ViewModels
{
    public class MainViewModel : ComputerMonitoringViewModelBase
    {
        #region constructor
     
        private readonly IDialogService _dialogService;
        private IEventAggregator _eventsHub;
        private bool _watchdogIsUnsubsribed;
        private bool _watchdogIsInitialized;
        private ProcessWatchDog _watchdog;
        private string _watchdogTargetName;

        public MainViewModel(IDialogService dialogService)
        {
            _watchdog = new ProcessWatchDog();
            _watchdog.PacketsExchangedEvent += ReportPacketExchange;

            _watchdogTargetName = "USBHelperLauncher";
            IsMonitoringVisible = true;
            IsWatchdogRunning = true;

            _dialogService = dialogService;
            _eventsHub = new EventAggregator();

            SubscribeToEvents();
            SetMonitoringCounter(900);
        }

        #endregion


        #region Methods

        private void ChangeWatchdogTarget(string newTarget)
        {
            if (IsWatchdogRunning) ToggleWatchdogRunStateCommandExecute();
            _watchdogTargetName = newTarget;
            ToggleWatchdogRunStateCommandExecute();
        }

        private void ManageWatchdog(ref bool watchdog_is_initialized)
        {
            if (!(_watchdog.IsProcessCurrentlyRunning(_watchdogTargetName))) watchdog_is_initialized = false;
            else
            {
                if (!watchdog_is_initialized)
                {
                    var pidAndPorts = _watchdog.GetOpenPortsForProcess(_watchdogTargetName);
                    _watchdog.InitializeWatchdog(pidAndPorts.Key, pidAndPorts.Value);
                    if (watchdog_is_initialized) Reporter.SendEmailReport(
                                    subject: $"ALARM: Detected process start for {_watchdogTargetName}",
                                    message: $"Activity detected report:\n" +
                                    $"----------------{_watchdogTargetName}---------------\n\n" +
                                    "DateTime: " + DateTime.Now.ToString("dd/MM/yyyy H:mm:ss") + "\n");
                    watchdog_is_initialized = true;
                }

                _watchdog.RefreshInfo();
            }
        }

        protected override void OnCounterCompletionEvent(Object source, ElapsedEventArgs e)
        {
            RefreshMonitoring();
        }

        protected override void RefreshMonitoring()
        {
            try
            {
                if (_watchdogIsUnsubsribed) _watchdogIsInitialized = false;
                RamUsage = PerformanceInfo.GetCurrentRamMemoryUsage();
                CpuUsage = PerformanceInfo.GetCurrentTotalCpuUsage();
                CpuClockSpeed = PerformanceInfo.GetCpuClockSpeed();
                if (IsWatchdogRunning) ManageWatchdog(ref _watchdogIsInitialized);
            }
            catch (Exception e)
            {
                Reporter.LogException(e);
            }
        }

        private async void ReportPacketExchange()
        {
            await Task.Run(() => Reporter.SendEmailReport(
                            subject: $"ALARM: Detected Activity for {_watchdogTargetName}",
                            message: $"Activity detected report:\n" +
                                        $"----------------{_watchdogTargetName}---------------\n\n" +
                                        "DateTime: " + DateTime.Now.ToString("dd/MM/yyyy H:mm:ss") + "\n\nContent:\n" +
                                        $"Net send bytes : {ProcessWatchDog.ProccessInfo.NetSendBytes}\n" +
                                        $"Net Received bytes : {ProcessWatchDog.ProccessInfo.NetRecvBytes}\n" +
                                        $"Net Total bytes: {ProcessWatchDog.ProccessInfo.NetTotalBytes}\n"
                            ));
        }

        private void SubscribeToEvents()
        {
            _eventsHub.GetEvent<OnWatchdogTargetChangedEvent>().Subscribe(ChangeWatchdogTarget);
        }

        #endregion


        #region porperties

        private double _cpuUsage;

        public double CpuUsage
        {
            get { return _cpuUsage; }
            set 
            { 
                _cpuUsage = value;
                RaisePropertyChanged(nameof(CpuUsage));
            }
        }


        private float _cpuClockSpeed;

        public float CpuClockSpeed
        {
            get { return _cpuClockSpeed; }
            set 
            { 
                _cpuClockSpeed = value;
                RaisePropertyChanged(nameof(CpuClockSpeed));
            }
        }


        private double _ramUsage;

        public double RamUsage
        {
            get { return _ramUsage; }
            set
            {
                _ramUsage = value;
                RaisePropertyChanged(nameof(RamUsage));
            }
        }


        private bool _isMonitoringVisible;

        public bool IsMonitoringVisible
        {
            get { return _isMonitoringVisible; }
            set 
            { 
                _isMonitoringVisible = value;
                RaisePropertyChanged(nameof(IsMonitoringVisible));
            }
        }


        private bool _isWatchdogRunning;

        public bool IsWatchdogRunning
        {
            get { return _isWatchdogRunning; }
            set 
            { 
                _isWatchdogRunning = value;
                _watchdogIsUnsubsribed = !_isWatchdogRunning;
                RaisePropertyChanged(nameof(IsWatchdogRunning));
            }
        }


        #endregion


        #region Commands

        public ICommand ToggleWatchdogRunStateCommand
        {
            get { return new RelayCommand(ToggleWatchdogRunStateCommandExecute); }
        }

        public void ToggleWatchdogRunStateCommandExecute()
        {
            IsWatchdogRunning = !IsWatchdogRunning;
            if (!IsWatchdogRunning) _watchdog.StopCapturingPackets();
        }

        public ICommand ShowComputerMonitoringCommand
        {
            get { return new RelayCommand(ChangeComputerMonitoringCommandExecute, CanShowComputerMonitoringCommandExecute); }
        }

        public bool CanShowComputerMonitoringCommandExecute()
        {
            if (!IsMonitoringVisible) return true;
            return false;
        }

        public void ChangeComputerMonitoringCommandExecute()
        {
            IsMonitoringVisible = !IsMonitoringVisible;
        }

        public ICommand OpenWatchdogManagerCommand
        {
            get { return new RelayCommand(OpenWatchdogManagerCommandExecute); }
        }

        public void OpenWatchdogManagerCommandExecute()
        {
            var viewModel = new WatchdogSettingsDialogViewModel(_watchdogTargetName, _eventsHub);

            bool? result = _dialogService.ShowDialog(viewModel);

            if (result.HasValue)
            {
                if (result.Value)
                {
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine();
                }
            }
        }
        public ICommand HideComputerMonitoringCommand
        {
            get { return new RelayCommand(ChangeComputerMonitoringCommandExecute, CanHideWatchdogCommandExecute); }
        }

        public bool CanHideWatchdogCommandExecute()
        {
            if (IsMonitoringVisible) return true;
            return false;
        }

        public ICommand KillAppCommand
        { 
            get { return new RelayCommand<IClosable>(KillAppCommandExecute); }
        }

        public void KillAppCommandExecute(IClosable window)
        {
            if (window != null) window.Close();
        }


        #endregion




    }
}

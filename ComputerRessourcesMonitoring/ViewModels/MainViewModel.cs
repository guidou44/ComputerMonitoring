using Common.Reports;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using ComputerRessourcesMonitoring.Events;
using HardwareManipulation.HardwareInformation;
using HardwareManipulation.Models;
using Prism.Events;
using ProcessMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Runtime;
using ComputerRessourcesMonitoring.Models;
using Common.UI.ViewModels;

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

        private MonitoringTarget firstTargetEnum;
        private MonitoringTarget secondTargetEnum;

        private IDictionary<MonitoringTarget, Func<HardwareUsageBase>> targetToAction;

        public MainViewModel(IDialogService dialogService)
        {
            targetToAction = InitializeResourceDictionary();
            _watchdog = new ProcessWatchDog();
            _eventsHub = new EventAggregator();
            _watchdog.PacketsExchangedEvent += ReportPacketExchange;

            firstTargetEnum = MonitoringTarget.CPU_Usage_PC;
            secondTargetEnum = MonitoringTarget.RAM_Usage;

            _watchdogTargetName = "USBHelperLauncher";
            IsMonitoringVisible = true;
            IsWatchdogRunning = true;
            _dialogService = dialogService;

            RefreshMonitoring();
            SubscribeToEvents();
            SetMonitoringCounter(900);
        }

        #endregion


        #region Methods

        private void SetWatchdogTarget(string newTarget)
        {
            if (IsWatchdogRunning) ToggleWatchdogRunStateCommandExecute();
            _watchdogTargetName = newTarget;
            ToggleWatchdogRunStateCommandExecute();
        }

        private IDictionary<MonitoringTarget, Func<HardwareUsageBase>> InitializeResourceDictionary()
        {
            return new Dictionary<MonitoringTarget, Func<HardwareUsageBase>>()
            {
                {MonitoringTarget.RAM_Usage, new Func<HardwareUsageBase>(RAMPerformanceInfo.GetCurrentRamMemoryUsage)},
                {MonitoringTarget.CPU_Usage_PC, new Func<HardwareUsageBase>(CPUPerformanceInfo.GetCurrentGlobalCpuUsageWithPerfCounter)},
                {MonitoringTarget.CPU_Usage, new Func<HardwareUsageBase>(CPUPerformanceInfo.GetCurrentGlobalCpuUsage)},
                {MonitoringTarget.GPU_Usage, new Func<HardwareUsageBase>(GPUPerformanceInfo.GetFirstGpuInformation)},
                {MonitoringTarget.GPU_Temp, new Func<HardwareUsageBase>(GPUPerformanceInfo.GetFirstGpuTempOnly)},
                {MonitoringTarget.CPU_Temp, new Func<HardwareUsageBase>(CPUPerformanceInfo.GetCpuTemperature)},
            };

        }

        private void ManageWatchdog(ref bool watchdog_is_initialized)
        {
            if (!(_watchdog.IsProcessCurrentlyRunning(_watchdogTargetName))) watchdog_is_initialized = false;
            else
            {
                if (!watchdog_is_initialized)
                {
                    var pidAndPorts = _watchdog.GetOpenPortsForProcess(_watchdogTargetName);
                    _watchdog.InitializeWatchdog(pidAndPorts.Key, pidAndPorts.Value, _watchdogTargetName);
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
                var target_1 = targetToAction[firstTargetEnum].Invoke();
                var target_2 = targetToAction[secondTargetEnum].Invoke();
                FirstMonitoringTarget = target_1.Main_Value;
                SecondMonitoringTarget = target_2.Main_Value;
                if (FirstMonitoringTargetName != target_1.ShortName) FirstMonitoringTargetName = target_1.ShortName;
                if (SecondMonitoringTargetName != target_2.ShortName) SecondMonitoringTargetName = target_2.ShortName;
                FirstMonitoringTargetDisplay = target_1.ToString();
                SecondMonitoringTargetDisplay = target_2.ToString();
                if (IsWatchdogRunning) ManageWatchdog(ref _watchdogIsInitialized);
            }
            catch (Exception e)
            {
                Reporter.LogException(e);
                ShowErrorMessage(e);
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

        private void ShowErrorMessage(Exception e)
        {
            var viewModel = new ErrorMessageViewModel(e);

            bool? result =_dialogService.ShowDialog(viewModel);
        }

        private void SetMonitoringTargets(Queue<MonitoringTarget> targets)
        {
            firstTargetEnum = targets.Dequeue();
            secondTargetEnum = targets.Dequeue();
        }

        private void SubscribeToEvents()
        {
            _eventsHub.GetEvent<OnWatchdogTargetChangedEvent>().Subscribe(SetWatchdogTarget);
            _eventsHub.GetEvent<OnMonitoringTargetsChangedEvent>().Subscribe(SetMonitoringTargets);
        }

        #endregion


        #region porperties

        private double _firstMonitoringTarget;

        public double FirstMonitoringTarget
        {
            get { return _firstMonitoringTarget; }
            set 
            { 
                _firstMonitoringTarget = value;
                RaisePropertyChanged(nameof(FirstMonitoringTarget));
            }
        }

        private string _firstMonitoringTargetName;

        public string FirstMonitoringTargetName
        {
            get { return _firstMonitoringTargetName; }
            set 
            { 
                _firstMonitoringTargetName = value;
                RaisePropertyChanged(nameof(FirstMonitoringTargetName));
            }
        }

        private string _firstMonitoringTargetDisplay;

        public string FirstMonitoringTargetDisplay
        {
            get { return _firstMonitoringTargetDisplay; }
            set
            {
                _firstMonitoringTargetDisplay = value;
                RaisePropertyChanged(nameof(FirstMonitoringTargetDisplay));
            }
        }

        private double _secondMonitoringTarget;

        public double SecondMonitoringTarget
        {
            get { return _secondMonitoringTarget; }
            set
            {
                _secondMonitoringTarget = value;
                RaisePropertyChanged(nameof(SecondMonitoringTarget));
            }
        }

        private string _secondMonitoringTargetName;

        public string SecondMonitoringTargetName
        {
            get { return _secondMonitoringTargetName; }
            set
            {
                _secondMonitoringTargetName = value;
                RaisePropertyChanged(nameof(SecondMonitoringTargetName));
            }
        }

        private string _secondMonitoringTargetDisplay;

        public string SecondMonitoringTargetDisplay
        {
            get { return _secondMonitoringTargetDisplay; }
            set 
            { 
                _secondMonitoringTargetDisplay = value;
                RaisePropertyChanged(nameof(SecondMonitoringTargetDisplay));
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
            try
            {
                var viewModel = new WatchdogSettingsDialogViewModel(_watchdogTargetName, _eventsHub, firstTargetEnum, secondTargetEnum);

                bool? result = _dialogService.ShowDialog(viewModel);
            }
            catch (Exception e)
            {
                Reporter.LogException(e);
                ShowErrorMessage(e);
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
            if (window != null)
            {
                _watchdog.PacketsExchangedEvent -= ReportPacketExchange;
                _monitoringRefreshCounter.Stop();
                _monitoringRefreshCounter.Dispose();
                window.Close();
            }
        }


        #endregion




    }
}

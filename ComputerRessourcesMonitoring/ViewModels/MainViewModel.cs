using Common.Reports;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using ComputerRessourcesMonitoring.Events;
using HardwareManipulation.Connectors;
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
using Common.UI.ViewModels;
using HardwareManipulation.Enums;
using HardwareManipulation;
using System.Collections.ObjectModel;

namespace ComputerRessourcesMonitoring.ViewModels
{
    public class MainViewModel : ComputerMonitoringViewModelBase
    {
        #region constructor

        private DataManager _manager;
        private Queue<MonitoringTarget> _monitoringTargets;
        private ProcessWatchDog _watchdog;
        private bool _watchdogIsUnsubsribed;
        private bool _watchdogIsInitialized;
        private string _watchdogTargetName;

        public MainViewModel(IDialogService dialogService) : base (dialogService)
        {
            _manager = new DataManager();
            _watchdog = new ProcessWatchDog();
            _watchdog.PacketsExchangedEvent += ReportPacketExchange;
            _watchdogTargetName = "USBHelperLauncher";
            IsApplicationVisible = true;
            IsWatchdogRunning = true;
            _dialogService = dialogService;

            _monitoringTargets = new Queue<MonitoringTarget>();
            _monitoringTargets.Enqueue(MonitoringTarget.CPU_Load);
            _monitoringTargets.Enqueue(MonitoringTarget.GPU_Temp);
            _monitoringTargets.Enqueue(MonitoringTarget.Server_CPU_Load);

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
                var valuesQueue = _manager.GetCalculatedValues(_monitoringTargets);
                HardwareValues = new ObservableCollection<HardwareInformation>(valuesQueue);

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
            _dialogService.ShowException(e);
        }

        private void SetMonitoringTargets(Queue<MonitoringTarget> targets)
        {
            _monitoringTargets = targets;
            RefreshMonitoring();
            //ResetTopValue
        }

        private void SubscribeToEvents()
        {
            _eventHub.GetEvent<OnWatchdogTargetChangedEvent>().Subscribe(SetWatchdogTarget);
            _eventHub.GetEvent<OnMonitoringTargetsChangedEvent>().Subscribe(SetMonitoringTargets);
        }

        #endregion


        #region porperties

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

        private bool _isApplicationVisible;

        public bool IsApplicationVisible
        {
            get { return _isApplicationVisible; }
            set 
            { 
                _isApplicationVisible = value;
                RaisePropertyChanged(nameof(IsApplicationVisible));
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

        public ICommand ShowApplicationCommand
        {
            get { return new RelayCommand(ChangeAppVisibilityCommandExecute, CanShowApplicationCommandExecute); }
        }

        public bool CanShowApplicationCommandExecute()
        {
            if (!IsApplicationVisible) return true;
            return false;
        }

        public void ChangeAppVisibilityCommandExecute()
        {
            IsApplicationVisible = !IsApplicationVisible;
        }

        public ICommand OpenWatchdogManagerCommand
        {
            get { return new RelayCommand(OpenWatchdogManagerCommandExecute); }
        }

        public void OpenWatchdogManagerCommandExecute()
        {
            try
            {
                var viewModel = new WatchdogSettingsDialogViewModel(_watchdogTargetName, _eventHub, _monitoringTargets, _manager);

                bool? result = _dialogService.ShowDialog(viewModel);
            }
            catch (Exception e)
            {
                Reporter.LogException(e);
                ShowErrorMessage(e);
            }
        }

        public ICommand HideApplicationCommand
        {
            get { return new RelayCommand(ChangeAppVisibilityCommandExecute, CanHideApplicationCommandExecute); }
        }

        public bool CanHideApplicationCommandExecute()
        {
            if (IsApplicationVisible) return true;
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

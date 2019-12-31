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
    public class MainViewModel : WindowViewModelBase
    {
        #region constructor

        private DataManager _manager;
        private Queue<MonitoringTarget> _monitoringTargets;
        private Timer _monitoringRefreshCounter;
        private ProcessWatchDog _watchdog;

        public MainViewModel(IDialogService dialogService) : base (dialogService)
        {
            _manager = new DataManager();

            

            IsApplicationVisible = true;
            _dialogService = dialogService;

            _monitoringTargets = new Queue<MonitoringTarget>();
            _monitoringTargets.Enqueue(MonitoringTarget.CPU_Load);
            _monitoringTargets.Enqueue(MonitoringTarget.GPU_Temp);
            _monitoringTargets.Enqueue(MonitoringTarget.Server_CPU_Load);

            InitializeWatchdog();
            RefreshMonitoring();
            SubscribeToEvents();
            SetMonitoringCounter(900);
        }

        #endregion


        #region Methods

        private void InitializeWatchdog()
        {
            _watchdog = new ProcessWatchDog();
            _watchdog.PacketsExchangedEvent += ReportPacketExchange;
            ProcessesUnderWatch = new ObservableCollection<ProcessViewModel>();
            var process = _watchdog.GetProcessesByName("USBHelperLauncher").FirstOrDefault();
            ProcessesUnderWatch.Add(new ProcessViewModel(process, true));
        }

        private void SetWatchdogTarget(IEnumerable<ProcessViewModel> processesToWatch)
        {
            ProcessesUnderWatch = new ObservableCollection<ProcessViewModel>(processesToWatch);
        }

        private void ManageWatchdog()
        {
            foreach (var process2watch in ProcessesUnderWatch)
            {
                process2watch.IsRunning = _watchdog.IsProcessCurrentlyRunning(process2watch.Process.ProcessName);
                if (process2watch.IsRunning && process2watch.Check4PacketExchange)
                {
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
                ManageWatchdog();
            }
            catch (Exception e)
            {
                Reporter.LogException(e);
                ShowErrorMessage(e);
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

        protected void SetMonitoringCounter(int counterTimeMilliseconds)
        {
            _monitoringRefreshCounter = new Timer(counterTimeMilliseconds);
            _monitoringRefreshCounter.Elapsed += OnCounterCompletionEvent;
            _monitoringRefreshCounter.AutoReset = true;
            _monitoringRefreshCounter.Enabled = true;
        }

        private void SetMonitoringTargets(Queue<MonitoringTarget> targets)
        {
            _monitoringTargets = targets;
            RefreshMonitoring();
        }

        private void ShowErrorMessage(Exception e)
        {
            _dialogService.ShowException(e);
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

        public ICommand OpenSettingsWindowCommand
        {
            get { return new RelayCommand(OpenSettingsWindowCommandExecute); }
        }

        public void OpenSettingsWindowCommandExecute()
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

        #endregion




    }
}

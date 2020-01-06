using Common.Reports;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using ComputerRessourcesMonitoring.Events;
using HardwareAccess.Models;
using ProcessMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Common.UI.ViewModels;
using HardwareAccess.Enums;
using HardwareAccess;
using System.Collections.ObjectModel;
using ProcessMonitoring;
using System.Threading;
using ComputerRessourcesMonitoring.Models;
using System.ComponentModel;
using System.Reflection;

namespace ComputerRessourcesMonitoring.ViewModels
{
    public class MainViewModel : WindowViewModelBase
    {
        #region constructor

        private ComputerMonitoringManagerModel _app_manager;
        private DataManager _hardware_manager;
        private Queue<MonitoringTarget> _monitoringTargets;
        private System.Timers.Timer _monitoringRefreshCounter;
        private ProcessWatchDog _watchdog;
        private Thread _watchdogThread;

        public MainViewModel(IDialogService dialogService) : base (dialogService)
        {
            IsApplicationVisible = true;
            _app_manager = new ComputerMonitoringManagerModel(_eventHub);
            _dialogService = dialogService;
            _hardware_manager = new DataManager();
            _monitoringTargets = new Queue<MonitoringTarget>();
            var initialTargets = _hardware_manager.GetInitialTargets();
            initialTargets.ToList().ForEach(TARGET => _monitoringTargets.Enqueue(TARGET));

            InitializeWatchdog();
            RefreshMonitoring();
            SubscribeToEvents();
            SetMonitoringCounter(900);
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

        private void OnCounterCompletionEvent(object source, ElapsedEventArgs e)
        {
            RefreshMonitoring();
        }

        private void OnAppManagerPropertyChangedEvent(object source, PropertyChangedEventArgs e)
        {
            if (e != null)
            {
                PropertyInfo prop = GetType().GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo managerProp = _app_manager.GetType().GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite && managerProp != null)
                {
                    prop.SetValue(this, managerProp.GetValue(_app_manager), null);
                }
            }
        }

        private void RefreshMonitoring()
        {
            try
            {

                var valuesQueue = _hardware_manager.GetCalculatedValues(_monitoringTargets);
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
            _eventHub.GetEvent<OnWatchdogTargetChangedEvent>().Subscribe((processesToWatch) => { ProcessesUnderWatch = new ObservableCollection<ProcessViewModel>(processesToWatch);});
            _eventHub.GetEvent<OnMonitoringTargetsChangedEvent>().Subscribe((targets) => {  _monitoringTargets = targets; RefreshMonitoring(); });
            _app_manager.PropertyChanged += OnAppManagerPropertyChangedEvent;
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
            get { return new RelayCommand((() => { IsApplicationVisible = !IsApplicationVisible; }), (() => { return !IsApplicationVisible; })); }
        }

        public ICommand HideApplicationCommand
        {
            get { return new RelayCommand((() => { IsApplicationVisible = !IsApplicationVisible; }), (() => { return IsApplicationVisible; })); }
        }

        public ICommand KillAppCommand
        { 
            get { return new RelayCommand<IClosable>(KillAppCommandExecute); }
        }

        public void KillAppCommandExecute(IClosable window)
        {
            if (window != null)
            {
                _app_manager.Dispose();
                //_watchdog.PacketsExchangedEvent -= ReportPacketExchange;
                //_monitoringRefreshCounter.Stop();
                //_monitoringRefreshCounter.Dispose();
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
                var viewModel = new SettingsDialogViewModel(ProcessesUnderWatch, _eventHub, _monitoringTargets, manager: ref _hardware_manager, watchdog: ref _watchdog);
                bool? result = _dialogService.ShowDialog(viewModel);
            }
            catch (Exception e)
            {
                Reporter.LogException(e);
                _dialogService.ShowException(e);
            }
        }

        public ICommand ResizeWindowCommand
        {
            get { return new RelayCommand<object[]>(ResizeWindowCommandExecute); }
        }

        public void ResizeWindowCommandExecute(object[] parameters)
        {
            var stringData = parameters[0].ToString().Split(',');
            var desktopWorkingAreaRight = double.Parse(stringData[2]);
            var desktopWorkingAreaBottom = double.Parse(stringData[3]);
            var actualWindow = parameters[1] as IRelocatable;
            actualWindow.Left = desktopWorkingAreaRight - actualWindow.ActualWidth;
            actualWindow.Top = desktopWorkingAreaBottom - actualWindow.ActualHeight;
        }

        public ICommand SetWindowOnTopCommand
        {
            get { return new RelayCommand<ITopMost>(W => W.Topmost = true); }
        }

        #endregion




    }
}

using Common.Reports;
using Common.UI;
using ComputerRessourcesMonitoring.Interfaces;
using ComputerRessourcesMonitoring.Models;
using ProcessMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;


/*TODO: 
        evaluates the cpu precision with thread sleep vs await delay
        Add possibility to activate/deactivate watchdog in UI
        Add possibility to change the watchdog target*/



namespace ComputerRessourcesMonitoring.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        #region constructor
     
        private bool done;
        private bool _watchdogIsUnsubsribed;
        private ProcessWatchDog _watchdog;

        public MainViewModel()
        {
            _watchdog = new ProcessWatchDog();
            ProcessWatchDog.PacketsExchangedEvent += ReportPacketExchange;

            WatchdogTargetName = "USBHelperLauncher";
            //WatchdogTargetName = "chrome";
            IsMonitoringVisible = true;
            IsWatchdogRunning = true;
            done = false;
            StartMonitoring();
        }

        #endregion


        #region Methods

        private Task Delay(int time_in_milliseconds)
        {
            return Task.Delay(time_in_milliseconds);
        }

        public void ManageWatchdog(ref bool watchdog_is_initialized)
        {
            if (!(_watchdog.IsProcessCurrentlyRunning(WatchdogTargetName))) watchdog_is_initialized = false;
            else
            {
                if (!watchdog_is_initialized)
                {
                    var pidAndPorts = _watchdog.GetOpenPortsForProcess(WatchdogTargetName);
                    _watchdog.InitializeWatchdog(pidAndPorts.Key, pidAndPorts.Value);
                    if (watchdog_is_initialized) Reporter.SendEmailReport(
                                    subject: $"ALARM: Detected process start for {WatchdogTargetName}",
                                    message: $"Activity detected report:\n" +
                                    $"----------------{WatchdogTargetName}---------------\n\n" +
                                    "DateTime: " + DateTime.Now.ToString("dd/MM/yyyy H:mm:ss") + "\n");
                    watchdog_is_initialized = true;
                }

                ProcessWatchDog.RefreshInfo();
            }

        }

        public async void ReportPacketExchange()
        {
            await Task.Run(() => Reporter.SendEmailReport(
                            subject: $"ALARM: Detected Activity for {WatchdogTargetName}",
                            message: $"Activity detected report:\n" +
                                        $"----------------{WatchdogTargetName}---------------\n\n" +
                                        "DateTime: " + DateTime.Now.ToString("dd/MM/yyyy H:mm:ss") + "\n\nContent:\n" +
                                        $"Net send bytes : {ProcessWatchDog.ProccessInfo.NetSendBytes}\n" +
                                        $"Net Received bytes : {ProcessWatchDog.ProccessInfo.NetRecvBytes}\n" +
                                        $"Net Total bytes: {ProcessWatchDog.ProccessInfo.NetTotalBytes}\n"
                            ));
        }

        private async void StartMonitoring()
        {
            var watchdog_is_initialized = false;

            while (!done)
            {
                try
                {
                    if(_watchdogIsUnsubsribed) watchdog_is_initialized = false;
                    RamUsage = PerformanceInfo.GetCurrentRamMemoryUsage();
                    CpuUsage = PerformanceInfo.GetCurrentTotalCpuUsage();
                    CpuClockSpeed = PerformanceInfo.GetCpuClockSpeed();
                    await Delay(800);
                    if (IsWatchdogRunning) await Task.Run(() => ManageWatchdog(ref watchdog_is_initialized));
                }
                catch (Exception e)
                {
                    Reporter.LogException(e);
                }
        }
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


        private string _watchdogTargetName;

        public string WatchdogTargetName
        {
            get { return _watchdogTargetName; }
            set 
            { 
                _watchdogTargetName = value;
                RaisePropertyChanged(nameof(WatchdogTargetName));
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
            ProcessWatchDog.StopCapturingPackets();
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

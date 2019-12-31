using Common.Reports;
using Common.UI.Infrastructure;
using ProcessMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ComputerRessourcesMonitoring.ViewModels
{
    public class ProcessViewModel : NotifyPropertyChanged
    {
        public delegate void OnWatchdogStopped(Process process);
        public event OnWatchdogStopped OnWatchdogStoppedEvent;

        public ProcessViewModel(Process process, bool check4PacketExchange)
        {
            Process = process;
            Check4PacketExchange = check4PacketExchange;
        }

        public override string ToString()
        {
            return Process.Id.ToString();
        }

        #region Properties

        public Process Process { get; set; }
        public bool WasInitialized { get; set; }

        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set 
            { 
                _isRunning = value;
                if (!_isRunning) WasInitialized = false;
                else Reporter.SendEmailReport(
                    subject: $"ALARM: Detected process start for {Process.ProcessName}",
                    message: $"Activity detected report:\n" +
                    $"----------------{Process.ProcessName}---------------\n\n" +
                    "DateTime: " + DateTime.Now.ToString("dd/MM/yyyy H:mm:ss") + "\n");
                RaisePropertyChanged(nameof(IsRunning)); 
            }
        }

        private bool _check4PacketExchange;
        public bool Check4PacketExchange
        {
            get { return _check4PacketExchange; }
            set 
            { 
                _check4PacketExchange = value;
                if (!_check4PacketExchange) WasInitialized = false;
                RaisePropertyChanged(nameof(Check4PacketExchange)); 
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
            Check4PacketExchange = !Check4PacketExchange;
            if (!Check4PacketExchange)
            {
                OnWatchdogStoppedEvent(Process);
            }
        }

        #endregion
    }
}

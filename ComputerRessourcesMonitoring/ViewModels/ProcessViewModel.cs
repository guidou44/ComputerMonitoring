﻿using Common.Reports;
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
        public delegate void OnProcessNameChanged(ProcessViewModel processViewModel);
        public event OnProcessNameChanged OnProcessNameChangedEvent;

        public ProcessViewModel(bool check4PacketExchange)
        {
            Check4PacketExchange = check4PacketExchange;
        }

        public override string ToString()
        {
            var upperChars = ProcessName.Where(C => char.IsUpper(C));
            string output = (upperChars.Count() > 1) ? (upperChars.FirstOrDefault().ToString() + upperChars.LastOrDefault().ToString()) :
                            (upperChars.Count() == 1) ? upperChars.SingleOrDefault().ToString() :
                            ProcessName[0].ToString();
            return (output);
        }

        public void RegisterProcessNameEventHandlerIfNotRegistered(Delegate prospectiveHandler)
        {
            if (OnProcessNameChangedEvent != null)
            {
                foreach (Delegate existingHandler in OnProcessNameChangedEvent.GetInvocationList())
                {
                    if (existingHandler == prospectiveHandler)
                    {
                        return;
                    }
                }
            }
            OnProcessNameChangedEvent += (OnProcessNameChanged) prospectiveHandler;
        }

        #region Properties

        public Process Process { get; set; }

        private string _processName;
        public string ProcessName
        {
            get { return _processName; }
            set
            {
                _processName = value;
                RaisePropertyChanged(nameof(ProcessName));
            }
        }

        public bool WasInitialized { get; set; }

        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set 
            {
#if !DEBUG
                if (!_isRunning && value) Reporter.SendEmailReport(
                    subject: $"ALARM: Detected process start for {Process.ProcessName}",
                    message: $"Activity detected report:\n" +
                    $"----------------{Process.ProcessName}---------------\n\n" +
                    "DateTime: " + DateTime.Now.ToString("dd/MM/yyyy H:mm:ss") + "\n");
#endif
                _isRunning = value;
                if (!_isRunning) WasInitialized = false;
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

        public ICommand ChangeWatchdogTargetCommand
        {
            get { return new RelayCommand(() => { OnProcessNameChangedEvent(this); }, CanChangeWatchdogTargetCommandExecute); }
        }

        public bool CanChangeWatchdogTargetCommandExecute()
        {
            if (ProcessName?.Length > 0 && ((Process == null) ? true : Process.ProcessName != ProcessName)) return true;
            return false;
        }

        public ICommand ToggleWatchdogRunStateCommand
        {
            get { return new RelayCommand(() => { Check4PacketExchange = !Check4PacketExchange; }); }
        }

        #endregion
    }
}

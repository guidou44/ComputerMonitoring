using Common.Reports;
using Common.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopAssistant.BL.ProcessWatch;

namespace DesktopAssistant.ViewModels
{
    public class ProcessViewModel : NotifyPropertyChanged
    {
        private const string DefaultEmptyName = "#NAME# ENTER2APPLY";
        public event EventHandler OnProcessNameChangedEvent;
        public event EventHandler OnProcessWatchRemoveEvent;

        public ProcessViewModel(bool doCapture, string processName = DefaultEmptyName)
        {
            DoCapture = doCapture;
            ProcessName = processName;
        }
        
        public ProcessViewModel(bool doCapture, string processName, bool isRunning)
        {
            DoCapture = doCapture;
            ProcessName = processName;
            IsRunning = isRunning;
        }

        public override string ToString()
        {
            var upperChars = ProcessName.Where(char.IsUpper);
            string output = (upperChars.Count() > 1) ? (upperChars.FirstOrDefault() + upperChars.LastOrDefault().ToString()) :
                            (upperChars.Count() == 1) ? upperChars.SingleOrDefault().ToString() :
                            ProcessName[0].ToString();
            return (output);
        }

        #region Properties

        public bool IsValidProcessViewModel => ProcessName != DefaultEmptyName;

        private bool _canRemoveProcessWatch;
        public bool CanRemoveProcessWatch
        {
            get { return _canRemoveProcessWatch; }
            set 
            { 
                _canRemoveProcessWatch = value;
                RaisePropertyChanged(nameof(CanRemoveProcessWatch));
            }
        }

        private bool _doCapture;
        public virtual bool DoCapture
        {
            get { return _doCapture; }
            set
            {
                _doCapture = value;
                RaisePropertyChanged(nameof(DoCapture));
            }
        }

        private bool _isRunning;
        public virtual bool IsRunning
        {
            get { return _isRunning; }
            set 
            {
                _isRunning = value;
                RaisePropertyChanged(nameof(IsRunning)); 
            }
        }

        private string _processName;
        public virtual string ProcessName
        {
            get { return _processName; }
            set
            {
                _processName = value;
                RaisePropertyChanged(nameof(ProcessName));
            }
        }

        #endregion

        #region Commands

        public ICommand ChangeWatchdogTargetCommand
        {
            get { return new RelayCommand(() => { OnProcessNameChangedEvent?.Invoke(this, EventArgs.Empty); }, CanChangeWatchdogTargetCommandExecute); }
        }

        public bool CanChangeWatchdogTargetCommandExecute()
        {
            return ProcessName?.Length > 0;
        }

        public ICommand RemoveProcessWatchCommand
        {
            get { return new RelayCommand(() => { OnProcessWatchRemoveEvent?.Invoke(this, EventArgs.Empty); }); }
        }
        
        public ICommand ToggleWatchdogRunStateCommand
        {
            get { return new RelayCommand(() => { DoCapture = !DoCapture; }); }
        }

        #endregion
    }
}

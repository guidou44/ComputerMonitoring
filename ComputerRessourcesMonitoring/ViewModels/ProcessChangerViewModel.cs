using Common.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ComputerRessourcesMonitoring.ViewModels
{
    public class ProcessChangerViewModel : NotifyPropertyChanged
    {
        public delegate void OnProcessNameChanged(string newProcessName);
        public event OnProcessNameChanged OnProcessNameChangedEvent;

        public ProcessChangerViewModel(string processName)
        {
            ProcessName = processName;
        }

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

        public ICommand ChangeWatchdogTargetCommand
        {
            get { return new RelayCommand(ChangeWatchdogTargetCommandExecute, CanChangeWatchdogTargetCommandExecute); }
        }

        public void ChangeWatchdogTargetCommandExecute()
        {
            OnProcessNameChangedEvent(ProcessName);
        }

        public bool CanChangeWatchdogTargetCommandExecute()
        {
            if (ProcessName?.Length > 0) return true;
            return false;
        }

    }
}

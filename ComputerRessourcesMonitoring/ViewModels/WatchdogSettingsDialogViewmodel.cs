using Common.DialogServices;
using Common.UI;
using Common.UIInterfaces;
using ComputerRessourcesMonitoring.Events;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace ComputerRessourcesMonitoring.ViewModels
{
    public class WatchdogSettingsDialogViewModel : ComputerMonitoringViewModelBase, IDialogRequestClose
    {
        #region Constructor

        private IEventAggregator _eventsHub;
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        public WatchdogSettingsDialogViewModel(string message, IEventAggregator eventsHub) : this(eventsHub)
        {
            WatchdogTargetName = message;
        }

        public WatchdogSettingsDialogViewModel(IEventAggregator eventsHub) 
        {
            _eventsHub = eventsHub;
            SetMonitoringCounter(900);
        }

        #endregion


        #region Methods

        protected override void OnCounterCompletionEvent(Object source, ElapsedEventArgs e)
        {
            RefreshMonitoring();
        }

        protected override void RefreshMonitoring()
        {
            //set monitoring in settings view refresh
        }

        #endregion


        #region Properties

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

        #endregion


        #region Commands

        public ICommand ChangeWatchdogTargetCommand
        {
            get { return new RelayCommand(ChangeWatchdogTargetCommandExecute, CanChangeWatchdogTargetCommandExecute); }
        }

        public void ChangeWatchdogTargetCommandExecute()
        {
            _eventsHub.GetEvent<OnWatchdogTargetChangedEvent>().Publish(WatchdogTargetName);
        }

        public bool CanChangeWatchdogTargetCommandExecute()
        {
            if (WatchdogTargetName?.Length > 0) return true;
            return false;
        }

        public ICommand DragWindowCommand
        {
            get { return new RelayCommand<IDragable>(DragWindowCommandExecute); }
        }

        public void DragWindowCommandExecute(IDragable dialog)
        {
            dialog.DragMove();
        }

        public ICommand OkCommand 
        {
            get { return new RelayCommand<WatchdogSettingsDialogViewModel>(p => CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true))); }
        }

        public ICommand CancelCommand
        {
            get { return new RelayCommand<WatchdogSettingsDialogViewModel>(p => CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(false))); }
        }

        #endregion

    }
}

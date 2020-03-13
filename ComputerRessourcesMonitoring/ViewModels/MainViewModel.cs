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
using ComputerResourcesMonitoring.Models;
using System.ComponentModel;
using System.Reflection;

namespace ComputerRessourcesMonitoring.ViewModels
{
    public class MainViewModel : WindowViewModelBase
    {
        #region constructor

        private ComputerMonitoringManagerModel _app_manager;

        public MainViewModel(IDialogService dialogService) : base (dialogService)
        {
            IsApplicationVisible = true;
            _app_manager = new ComputerMonitoringManagerModel(_eventHub);
            _dialogService = dialogService;
            SubscribeToEvents();
        }

        #endregion


        #region Private Methods

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

        private void OnMonitoringErrorOccuredEvent(Exception e)
        {
            _dialogService.ShowException(e);
        }

        private void SubscribeToEvents()
        {
            _app_manager.PropertyChanged += OnAppManagerPropertyChangedEvent;
            _app_manager.OnMonitoringErrorOccured += OnMonitoringErrorOccuredEvent;
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
                _app_manager.PropertyChanged -= OnAppManagerPropertyChangedEvent;
                _app_manager.OnMonitoringErrorOccured -= OnMonitoringErrorOccuredEvent;
                _app_manager.Dispose();
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
                var viewModel = new SettingsDialogViewModel(ProcessesUnderWatch,
                    _eventHub, 
                    _app_manager.GetMonitoringQueue(), 
                    manager: _app_manager.GetHardwareManager(), 
                    watchdog: _app_manager.GetWatchDog());
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

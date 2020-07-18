using Common.Reports;
using Common.UI.Infrastructure;
using Common.UI.ViewModels;
using Common.UI.WindowProperty;
using ComputerResourcesMonitoring.Models;
using Hardware.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Threading;
using static ComputerResourcesMonitoring.Models.ComputerMonitoringManagerModel;

namespace DesktopAssistant.ViewModels
{
    public class MainViewModel : WindowViewModelBase
    {
        private ComputerMonitoringManagerModel _app_manager;
        private Reporter _reporter;

        public MainViewModel(IDialogService dialogService, 
            ComputerMonitoringManagerModel manager, 
            IEventAggregator eventAgg,
            Reporter reporter) : base (dialogService, eventAgg)
        {
            IsApplicationVisible = true;
            _app_manager = manager;
            _dialogService = dialogService;
            _reporter = reporter;
            SubscribeToEvents();
        }

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
            _app_manager.PropertyChanged += new PropertyChangedEventHandler(OnAppManagerPropertyChangedEvent);
            _app_manager.OnMonitoringErrorOccured += new MonitoringErrorOccuredEventHandler(OnMonitoringErrorOccuredEvent);
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
                    watchdog: _app_manager.GetWatchDog(),
                    _reporter);

                _dialogService.Instantiate(viewModel);
                bool? result = _dialogService.ShowDialog(viewModel);
            }
            catch (Exception e)
            {
                _reporter.LogException(e);
                _dialogService.ShowException(e);
            }
        }

        public ICommand ResizeWindowCommand
        {
            get { return new RelayCommand<object[]>(ResizeWindowCommandExecute); }
        }

        public void ResizeWindowCommandExecute(object[] parameters)
        {
            ResizeWindow(parameters);
        }

        private void ResizeWindow(object[] parameters)
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

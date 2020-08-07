using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Common.Reports;
using Common.UI.Infrastructure;
using Common.UI.ViewModels;
using Common.UI.WindowProperty;
using DesktopAssistant.Assembler;
using DesktopAssistant.BL;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.BL.Persistence;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.Configuration;
using DesktopAssistant.UI;
using Prism.Events;

namespace DesktopAssistant.ViewModels
{
    public class MainViewModel : WindowViewModelBase, IManagerObserver
    {
        private readonly IAppManager _appManager;
        private readonly IRepository _repository;

        public MainViewModel(IDialogService dialogService, 
            IAppManager manager, 
            IEventAggregator eventAgg, 
            IRepository repository) : base (dialogService, eventAgg)
        {
            IsApplicationVisible = true;
            _appManager = manager;
            _dialogService = dialogService;
            _repository = repository;
            _appManager.RegisterManagerObserver(this);
            _appManager.Start();
            UiSettings = repository.Read<UserInterfaceConfiguration>();
        }
        
        public IUiSettings UiSettings { get;  }

        public void OnHardwareInfoChange()
        {
            ICollection<IHardwareInfo> hardwareInfos = _appManager.HardwareValues;
            HardwareValues = hardwareInfos.Select(HardwareAssembler.AssembleFromModel).ToList();
        }

        public void OnProcessWatchInfoChange()
        {
            IEnumerable<IProcessWatch> processWatches = _appManager.ProcessesUnderWatch;
            ProcessesUnderWatch = new ObservableCollection<ProcessViewModel>(processWatches.Select(ProcessWatchAssembler.AssembleFromModel));
        }

        public void OnError(Exception e)
        {
            _dialogService.ShowException(e);
        }

        #region Properties

        private ICollection<HardwareViewModel> _hardwareValues;
        public ICollection<HardwareViewModel> HardwareValues
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

        private bool _isOptionMenuVisible = false;
        public bool IsOptionMenuVisible
        {
            get => _isOptionMenuVisible;
            set
            {
                _isOptionMenuVisible = value;
                RaisePropertyChanged(nameof(IsOptionMenuVisible));
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
                _appManager.Dispose();
                window.Close();
            }
        }

        public ICommand OpenHardwareSettingsWindowCommand
        {
            get { return new RelayCommand(OpenHardwareSettingsWindowCommandExecute); }
        }

        public void OpenHardwareSettingsWindowCommandExecute()
        {
            try
            {
                HardwareSettingsViewModel viewModel = new HardwareSettingsViewModel(_eventHub, _appManager, UiSettings);
                _dialogService.Instantiate(viewModel);
                _dialogService.ShowDialog(viewModel);
            }
            catch (Exception e)
            {
                _repository.Update(e);
                _dialogService.ShowException(e);
            }
        }

        public ICommand OpenProcessSettingsWindowCommand
        {
            get { return new RelayCommand(OpenProcessSettingsWindowCommandExecute); }
        }

        public void OpenProcessSettingsWindowCommandExecute()
        {
            try
            {
                ProcessWatchSettingsViewModel viewModel = new ProcessWatchSettingsViewModel(_eventHub, ProcessesUnderWatch, UiSettings);
                _dialogService.Instantiate(viewModel);
                _dialogService.ShowDialog(viewModel);
            }
            catch (Exception e)
            {
                _repository.Update(e);
                _dialogService.ShowException(e);
            }
        }

        public ICommand UpdateOptionMenuDisplayCommand
        {
            get { return new RelayCommand<string>(SetOptionMenuVisibility);}
        }

        public void SetOptionMenuVisibility(string visibility)
        {
            IsOptionMenuVisible = bool.Parse(visibility);
        }
        
        #region UI location
        
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
            string[] stringData = parameters[0].ToString().Split(',');
            double desktopWorkingAreaRight = double.Parse(stringData[2]);
            double desktopWorkingAreaBottom = double.Parse(stringData[3]);
            if (!(parameters[1] is IRelocatable actualWindow)) return;
            actualWindow.Left = desktopWorkingAreaRight - actualWindow.ActualWidth;
            actualWindow.Top = desktopWorkingAreaBottom - actualWindow.ActualHeight;
        }

        public ICommand SetWindowOnTopCommand
        {
            get { return new RelayCommand<ITopMost>(w => w.Topmost = true); }
        }
        
        #endregion

        #endregion
    }
}

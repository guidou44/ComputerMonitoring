using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Common.UI.Infrastructure;
using Common.UI.ViewModels;
using DesktopAssistant.Assembler;
using DesktopAssistant.BL;
using DesktopAssistant.BL.Events;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.UI;
using Prism.Events;

namespace DesktopAssistant.ViewModels
{
    public class ProcessWatchSettingsViewModel : DialogViewModelBase
    {
        private readonly IEventAggregator _eventHub;

        public ProcessWatchSettingsViewModel(IEventAggregator eventHub, 
            IEnumerable<ProcessViewModel> processViewModels,
            IUiSettings uiSettings)
        {
            _eventHub = eventHub;

            ProcessesUnderWatch = new ObservableCollection<ProcessViewModel>(processViewModels);
            foreach (ProcessViewModel processUnderWatch in ProcessesUnderWatch)
            {
                processUnderWatch.OnProcessNameChangedEvent += OnWatchdogTargetChanged;
                processUnderWatch.OnProcessWatchRemoveEvent += OnWatchdogRemoveTarget;
            }

            UiSettings = uiSettings;
        }

        public IUiSettings UiSettings { get; set; }
        
        private void OnWatchdogRemoveTarget(object sender, EventArgs e)
        {
            var processVm = ProcessesUnderWatch.SingleOrDefault(puw => puw == (ProcessViewModel)sender);
            if (processVm != null)
            {
                processVm.OnProcessWatchRemoveEvent -= OnWatchdogRemoveTarget;
                ProcessesUnderWatch.Remove(processVm);
            }

            IEnumerable<IProcessWatch> models = ProcessesUnderWatch.Where(p => p.IsValidProcessViewModel)
                .Select(ProcessWatchAssembler.AssembleFromViewModel);
            _eventHub.GetEvent<OnWatchdogTargetChangedEvent>().Publish(models);
        }

        private void OnWatchdogTargetChanged(object sender, EventArgs e)
        {
            var processVm = ProcessesUnderWatch.SingleOrDefault(puw => puw == (ProcessViewModel)sender);
            
            IEnumerable<IProcessWatch> models = ProcessesUnderWatch.Select(ProcessWatchAssembler.AssembleFromViewModel);
            _eventHub.GetEvent<OnWatchdogTargetChangedEvent>().Publish(models);
        }
        
        public ICommand AddToWatchdogCollectionCommand
        {
            get { return new RelayCommand((() => 
                {
                    var newProcessVm = new ProcessViewModel(true);
                    newProcessVm.OnProcessNameChangedEvent += OnWatchdogTargetChanged;
                    newProcessVm.OnProcessWatchRemoveEvent += OnWatchdogRemoveTarget;
                    ProcessesUnderWatch.Add(newProcessVm); 
                }), 
                (() => { return ProcessesUnderWatch.Count() < 7 && !CanRemoveWatchdogTargets; })); }        
        }

        public ICommand RemoveFromWatchdogCollectionCommand
        {
            get { return new RelayCommand((() => 
                { 
                    ProcessesUnderWatch.ToList().ForEach(puw => puw.CanRemoveProcessWatch = true);
                    CanRemoveWatchdogTargets = true;
                }), 
                (() => { return ProcessesUnderWatch.Any(); })); }
        }

        public ICommand StopRemovingWatchdogProcessCommand
        {
            get { return new RelayCommand(() =>
            {
                ProcessesUnderWatch.ToList().ForEach(puw => puw.CanRemoveProcessWatch = false);
                CanRemoveWatchdogTargets = false;
            }); }
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
        
        private bool _canRemoveWatchdogTargets;
        public bool CanRemoveWatchdogTargets
        {
            get { return _canRemoveWatchdogTargets; }
            set 
            { 
                _canRemoveWatchdogTargets = value;
                RaisePropertyChanged(nameof(CanRemoveWatchdogTargets));
            }
        }

    }
}
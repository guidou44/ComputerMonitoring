using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Common.UI.ViewModels
{
    public abstract class WindowViewModelBase : NotifyPropertyChanged
    {
        protected IDialogService _dialogService;
        protected IEventAggregator _eventHub;
        public WindowViewModelBase()
        {
            _dialogService = new DialogService();
            _eventHub = new EventAggregator();
        }
    }
}

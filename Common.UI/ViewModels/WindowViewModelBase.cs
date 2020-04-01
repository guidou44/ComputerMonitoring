using Autofac;
using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.WindowProperty;
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
        protected IContainer _container;

        public WindowViewModelBase(IDialogService dialogService, IContainer container)
        {
            _container = container;
            _dialogService = dialogService;
            _eventHub = _container.Resolve<IEventAggregator>();
        }
    }
}

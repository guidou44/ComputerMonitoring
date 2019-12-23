using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPF_DS_Template.ViewModels
{
    public class DialogViewModel : IDialogRequestClose
    {
        #region Constructor

        private IEventAggregator _eventHub;

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        public DialogViewModel(IEventAggregator eventHub)
        {
            _eventHub = eventHub;
        }

        #endregion

        #region Commands

        public ICommand OkCommand
        {
            get { return new RelayCommand<IDialogRequestClose>(p => CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true))); }
        }

        #endregion

    }
}

using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.Interfacea;
using Common.UI.WindowProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Common.UI.ViewModels
{
    public abstract class DialogViewModelBase : NotifyPropertyChanged, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

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
            get { return new RelayCommand<DialogViewModelBase>(p => CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true))); }
        }

        public ICommand CancelCommand
        {
            get { return new RelayCommand<ErrorDialogViewModel>(p => CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(false))); }
        }
    }
}

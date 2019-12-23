using Common.UI.DialogServices;
using Common.UI;
using System;
using System.Windows.Input;
using Common.UI.Interfaces;
using Common.UI.Infrastructure;

namespace Common.Examples
{
    public class DialogViewModelEXAMPLE : IDialogRequestClose
    {
        public DialogViewModelEXAMPLE(string message)
        {
            Message       = message;
            OkCommand     = new RelayCommand<DialogViewModelEXAMPLE>(p => CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true)));
            CancelCommand = new RelayCommand<DialogViewModelEXAMPLE>(p => CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(false)));
        }

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        public string Message { get; }
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
    }
}
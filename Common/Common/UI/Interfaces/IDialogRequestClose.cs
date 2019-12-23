using Common.UI.DialogServices;
using System;


namespace Common.UI.Interfaces
{
    public interface IDialogRequestClose
    {
        event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }
}
using Common.UI.DialogServices;
using System;


namespace Common.UI.Interfacea
{
    public interface IDialogRequestClose
    {
        event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }
}
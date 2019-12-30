


using System;

namespace Common.UI.Interfaces
{

    public interface IDialogService
    {
        void Register<TViewModel, TView>() where TViewModel : IDialogRequestClose
                                           where TView : IDialog;

        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose;

        bool? ShowMessageBox(string message);

        void ShowException(Exception e);
    }
}
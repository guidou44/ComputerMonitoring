


using Common.UI.Interfacea;
using Common.UI.Interfaces;
using System;
using System.Collections.Generic;

namespace Common.UI.WindowProperty
{

    public interface IDialogService
    {
        IDictionary<Type, Type> Mappings { get; }

        void Register<TViewModel, TView>() where TViewModel : IDialogRequestClose
                                           where TView : IDialog;

        void Instantiate<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose;

        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose;

        bool? ShowMessageBox(string message);

        void ShowException(Exception e);
    }
}
using Common.UI.Interfaces;
using Common.UI.ViewModels;
using Common.UI.Views;
using System;
using System.Collections.Generic;
using System.Windows;


namespace Common.UI.DialogServices
{

    public class DialogService : IDialogService
    {
        private readonly Window owner;

        public DialogService(Window owner)
        {
            this.owner = owner;
            Mappings = new Dictionary<Type, Type>();
            RegisterBasicMappings();
        }
       
        public DialogService()
        {
            this.owner = Application.Current.MainWindow;
            Mappings = new Dictionary<Type, Type>();
            RegisterBasicMappings();
        }

        public IDictionary<Type, Type> Mappings { get; }

        private void RegisterBasicMappings()
        {
            Register<ErrorMessageViewModel, ErrorMessageView>();
            Register<MessageViewModel, MessageView>();
        }

        public void Register<TViewModel, TView>() where TViewModel : IDialogRequestClose
                                                  where TView : IDialog
        {
            if (Mappings.ContainsKey(typeof(TViewModel)))
            {
                throw new ArgumentException($"Type {typeof(TViewModel)} is already mapped to type {typeof(TView)}");
            }

            Mappings.Add(typeof(TViewModel), typeof(TView));
        }

        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose
        {
            Type viewType = Mappings[typeof(TViewModel)];

            IDialog dialog = (IDialog)Activator.CreateInstance(viewType);

            EventHandler<DialogCloseRequestedEventArgs> handler = null;

            handler = (sender, e) =>
            {
                viewModel.CloseRequested -= handler;

                if (e.DialogResult.HasValue)
                {
                    dialog.DialogResult = e.DialogResult;
                }
                else
                {
                    dialog.Close();
                }
            };

            viewModel.CloseRequested += handler;

            dialog.DataContext = viewModel;
            dialog.Owner = owner;

            return dialog.ShowDialog();
        }

        public bool? ShowMessageBox(string message)
        {
            return Application.Current.Dispatcher.Invoke(new Func<bool?>(() => 
            { 
            var messageVm = new MessageViewModel(message);
            bool? result = ShowDialog(messageVm);
            return result;
            }));
        }

        public void ShowException(Exception e)
        {
            var errorMessageVm = new ErrorMessageViewModel(e);
            bool? result = ShowDialog(errorMessageVm);
        }
    }
}







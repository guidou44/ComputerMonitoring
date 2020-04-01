using Common.UI.WindowProperty;
using Common.UI.ViewModels;
using Common.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Common.UI.Interfacea;
using Common.UI.Interfaces;

namespace Common.UI.DialogServices
{

    public class DialogService : IDialogService
    {
        private readonly Window owner;
        private IDictionary<IDialogRequestClose, IDialog> instances;
        public IDictionary<Type, Type> Mappings { get; }

        public DialogService(Window owner)
        {
            this.owner = owner;
            instances = new Dictionary<IDialogRequestClose, IDialog>();
            Mappings = new Dictionary<Type, Type>();
        }

        private void RegisterBasicMessageDialogMapping()
        {
            Register<MessageDialogViewModel, MessageView>();
        }

        private void RegisterBasicErrorDialogMapping()
        {
            Register<ErrorDialogViewModel, ErrorMessageView>();
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

        public void Instantiate<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose
        {
            if (instances.Select(i => i.Key.GetType()).Contains(viewModel.GetType()))
            {
                IDialogRequestClose oldViewModel = instances.SingleOrDefault(i => i.Key.GetType() == viewModel.GetType()).Key;
                instances.Remove(oldViewModel);
            }

            Type viewType = Mappings[typeof(TViewModel)];
            IDialog dialog = (IDialog)Activator.CreateInstance(viewType);

            dialog.DataContext = viewModel;
            dialog.Owner = owner;

            viewModel.CloseRequested += OnDialogCloseRequested;
            instances.Add(viewModel, dialog);
        }

        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose
        {
            return instances[viewModel].ShowDialog();
        }

        private void OnDialogCloseRequested(object sender, DialogCloseRequestedEventArgs e)
        {
            IDialogRequestClose viewModel = sender as IDialogRequestClose;
            IDialog dialog = instances[viewModel];
            viewModel.CloseRequested -= OnDialogCloseRequested;

            if (e.DialogResult.HasValue)
            {
                dialog.DialogResult = e.DialogResult;
            }
            else
            {
                dialog.Close();
            }
        }

        public bool? ShowMessageBox(string message)
        {
            Type messageDialogInterfaceType = typeof(MessageDialogViewModel);
            if (!Mappings.Any(M => messageDialogInterfaceType.IsAssignableFrom(M.Key)))
            {
                RegisterBasicMessageDialogMapping();
            }

            Type messageVmType = Mappings.SingleOrDefault(M => messageDialogInterfaceType.IsAssignableFrom(M.Key)).Key;
            dynamic messageVm = Activator.CreateInstance(messageVmType, message);
            Instantiate(messageVm);
            return ShowDialog(messageVm);
        }

        public void ShowException(Exception e)
        {
            Type errorDialogInterfaceType = typeof(ErrorDialogViewModel);
            if (!Mappings.Any(M => errorDialogInterfaceType.IsAssignableFrom(M.Key)))
            {
                RegisterBasicErrorDialogMapping();
            }

            Type errorVmType = Mappings.SingleOrDefault(M => errorDialogInterfaceType.IsAssignableFrom(M.Key)).Key;
            dynamic errorVm = Activator.CreateInstance(errorVmType, e);
            Instantiate(errorVm);
            bool? result = ShowDialog(errorVm);
        }
    }
}







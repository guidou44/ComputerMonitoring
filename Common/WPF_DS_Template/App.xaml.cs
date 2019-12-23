using Common.UI.DialogServices;
using Common.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPF_DS_Template.ViewModels;
using WPF_DS_Template.Views;

namespace WPF_DS_Template
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IDialogService dialogService = new DialogService(owner: MainWindow);

            dialogService.Register<DialogViewModel, DialogView>();

            var viewModel = new MainViewModel(dialogService);
            var view = new MainView { DataContext = viewModel };

            view.ShowDialog();

        }
    }
}

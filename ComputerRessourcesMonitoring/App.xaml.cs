using Autofac;
using Common.UI.DialogServices;
using Common.UI.Interfaces;
using Common.UI.ViewModels;
using Common.UI.Views;
using ComputerRessourcesMonitoring.ViewModels;
using ComputerRessourcesMonitoring.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace ComputerRessourcesMonitoring
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IDialogService dialogService = new DialogService(owner: MainWindow);
            dialogService.Register<SettingsDialogViewModel, WatchdogSettingsDialogView>();
            ContainerBuilder instanceContainerBuilder = new ContainerBuilder();

            MainViewModel viewModel = new MainViewModel(dialogService, instanceContainerBuilder);
            MainWindow view = new MainWindow { DataContext = viewModel };

            view.ShowDialog();
            
        }
    }
}

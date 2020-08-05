using System.Windows;
using Autofac;
using Common.Reports;
using Common.UI.WindowProperty;
using DesktopAssistant.BL;
using DesktopAssistant.UI;
using DesktopAssistant.ViewModels;
using DesktopAssistant.Views;
using Prism.Events;

namespace DesktopAssistant
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IContainer instanceContainer = ContainerResolver.Container;
            IDialogService dialogService = instanceContainer.Resolve<IDialogService>(new TypedParameter(typeof(Window), MainWindow));
            dialogService.Register<HardwareSettingsViewModel, HardwareSettingsView>();
            dialogService.Register<ProcessWatchSettingsViewModel, ProcessWatchSettingsView>();
            
            MainViewModel viewModel = instanceContainer.Resolve<MainViewModel>();
            MainWindow view = new MainWindow { DataContext = viewModel };

            view.ShowDialog();
        }
    }
}
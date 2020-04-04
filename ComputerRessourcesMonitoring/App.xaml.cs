﻿using Autofac;
using Common.UI.DialogServices;
using Common.UI.WindowProperty;
using Common.UI.ViewModels;
using Common.UI.Views;
using ComputerResourcesMonitoring.Models;
using ComputerRessourcesMonitoring.Infrastructure;
using ComputerRessourcesMonitoring.ViewModels;
using ComputerRessourcesMonitoring.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Common.Reports;

namespace ComputerRessourcesMonitoring
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IContainer instanceContainer = ContainerResolver.Container;
            IDialogService dialogService = instanceContainer.Resolve<IDialogService>(new TypedParameter(typeof(Window), MainWindow));
            dialogService.Register<SettingsDialogViewModel, WatchdogSettingsDialogView>();
            ComputerMonitoringManagerModel manager = instanceContainer.Resolve<ComputerMonitoringManagerModel>();
            Reporter reporter = instanceContainer.Resolve<Reporter>();

            MainViewModel viewModel = new MainViewModel(dialogService, manager, instanceContainer, reporter);
            MainWindow view = new MainWindow { DataContext = viewModel };

            view.ShowDialog();
        }
    }
}

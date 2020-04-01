using Autofac;
using Common.Helpers;
using Common.UI.DialogServices;
using Common.UI.Interfaces;
using Common.UI.ViewModels;
using Common.UI.Views;
using ComputerResourcesMonitoring.Models;
using HardwareAccess.Factories;
using HardwareManipulation;
using Prism.Events;
using ProcessMonitoring;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerRessourcesMonitoring.Infrastructure
{
    public static class ContainerResolver
    {
        private static IContainer _container;
        public static IContainer Container
        {
            get 
            { 
                if (_container == null)
                {
                    ContainerBuilder builder = new ContainerBuilder();
                    RegisterTypes(builder);
                    _container = builder.Build();
                }
                return _container; 
            }
        }

        private static void RegisterTypes(ContainerBuilder builder)
        {
            builder.RegisterType<DialogService>().As<IDialogService>();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>();
            builder.RegisterType<ConnectorFactory>().As<IFactory>();
            builder.RegisterType<XmlHelper>().AsSelf();
            builder.RegisterType<CommandLineHelper>().AsSelf();
            builder.RegisterType<DataManager>().AsSelf();
            builder.RegisterType<ProcessWatchDog>().AsSelf();
            builder.RegisterType<ComputerMonitoringManagerModel>().AsSelf();
        }

    }
}

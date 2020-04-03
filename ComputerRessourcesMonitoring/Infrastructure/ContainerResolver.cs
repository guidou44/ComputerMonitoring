using Autofac;
using Common.Helpers;
using Common.UI.DialogServices;
using Common.UI.WindowProperty;
using Common.UI.ViewModels;
using Common.UI.Views;
using ComputerResourcesMonitoring.Models;
using HardwareManipulation;
using Prism.Events;
using ProcessMonitoring;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareManipulation.Components;
using System.Diagnostics;
using ComputerRessourcesMonitoring.Factories;
using HardwareAccess.Connectors;
using HardwareAccess.Helpers;
using HardwareManipulation.Wrappers;

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
            builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<NvidiaWrapper>().As<INvidiaComponent>().SingleInstance();
            builder.RegisterType<ConnectorFactory>().As<IFactory<ConnectorBase>>().SingleInstance();
            builder.RegisterType<XmlHelper>().AsSelf();
            builder.RegisterType<CommandLineHelper>().AsSelf();
            builder.RegisterType<DataManager>().AsSelf();
            builder.RegisterType<ProcessWatchDog>().AsSelf(); 
            builder.RegisterType<ComputerMonitoringManagerModel>().AsSelf();

            builder.Register(c => new ServerResourceApiClient()).AsSelf();
            builder.RegisterType<OpenHardwareWrapper>().AsSelf().SingleInstance();
            builder.RegisterType<WmiHelper>().AsSelf().SingleInstance();
            builder.Register(c => new PerformanceCounter("Processor", "% Idle Time", "_Total")).AsSelf();

            builder.RegisterType<ASPNET_API_Connector>().AsSelf();
            builder.RegisterType<NVDIA_API_Connector>().AsSelf();
            builder.RegisterType<OpenHardware_Connector>().AsSelf();
            builder.RegisterType<SystemIO_Connector>().AsSelf();
            builder.RegisterType<WMI_Connector>().AsSelf();
            builder.Register<Func<Type, ConnectorBase>>(c =>
            {
                ILifetimeScope context = c.Resolve<ILifetimeScope>();
                return t => (ConnectorBase) context.Resolve(t);
            });
        }

    }
}

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
using System.Net.Mail;
using Common.Reports;
using Common.MailClient;
using Common.Wrappers;
using ProcessMonitoring.Models;
using ProcessMonitoring.Wrappers;
using ComputerRessourcesMonitoring.Wrappers;
using ComputerRessourcesMonitoring.Models;

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
            //Common.UI
            builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            //Common
            builder.RegisterType<CommandLineHelper>().AsSelf();
            builder.RegisterType<XmlHelper>().AsSelf();
            builder.RegisterType<Reporter>().AsSelf();
            builder.Register(c => new SmptClientWrapper("smtp.gmail.com")).As<IMailClient>();
                        
            //Process Monitoring
            builder.RegisterType<ProcessWatchDog>().AsSelf();
            builder.RegisterType<CaptureDeviceFactory>().As<ICaptureFactory<IPacketCaptureDevice>>();
            builder.RegisterType<CaptureDeviceWriterFactory>().As<ICaptureFactory<ICaptureFileWriter>>();

            //ComputerResourceMonitoring
            builder.RegisterType<ComputerMonitoringManagerModel>().AsSelf();
            builder.RegisterType<WatchdogThread>().As<IThread>().SingleInstance();
            builder.RegisterType<TimerWrapper>().As<ITimer>();

            //Hardware access
            builder.RegisterType<DataManager>().AsSelf();
            builder.RegisterType<WmiHelper>().AsSelf().SingleInstance();

            builder.RegisterType<DriveInfoProvider>().As<IDriveInfoProvider>().SingleInstance();
            builder.RegisterType<PerformanceCounterWrapper>().As<IPerformanceCounter>();
            builder.RegisterType<OpenHardwareComputerWrapper>().As<IOpenHardwareComputer>().SingleInstance();
            builder.RegisterType<NvidiaWrapper>().As<INvidiaComponent>().SingleInstance();
            builder.Register(c => new ServerResourceApiClientWrapper()).AsSelf();

            builder.RegisterType<ASPNET_API_Connector>().AsSelf();
            builder.RegisterType<NVDIA_API_Connector>().AsSelf();
            builder.RegisterType<OpenHardware_Connector>().AsSelf();
            builder.RegisterType<SystemIO_Connector>().AsSelf();
            builder.RegisterType<WMI_Connector>().AsSelf();

            builder.RegisterType<ConnectorFactory>().As<IFactory<ConnectorBase>>().SingleInstance();
            builder.Register<Func<Type, ConnectorBase>>(c =>
            {
                ILifetimeScope context = c.Resolve<ILifetimeScope>();
                return t => (ConnectorBase) context.Resolve(t);
            });
        }

    }
}

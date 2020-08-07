using System;
using Autofac;
using Common.Helpers;
using Common.MailClient;
using Common.Reports;
using Common.UI.DialogServices;
using Common.UI.WindowProperty;
using Common.Wrappers;
using DesktopAssistant.BL;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.BL.Persistence;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.BL.Wrappers;
using DesktopAssistant.Configuration;
using DesktopAssistant.Repository;
using DesktopAssistant.UI;
using DesktopAssistant.ViewModels;
using Hardware;
using Hardware.Components;
using Hardware.Connectors;
using Hardware.Factories;
using Hardware.Helpers;
using Hardware.Wrappers;
using Prism.Events;
using ProcessMonitoring;
using ProcessMonitoring.Factory;
using ProcessMonitoring.Models;

namespace DesktopAssistant
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
            //DesktopAssistant
            builder.RegisterType<MainViewModel>().AsSelf();
            builder.RegisterType<FlatFileRepository>().As<IRepository>();
            
            //Common.UI
            builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            //Common
            builder.RegisterType<CommandLineHelper>().AsSelf();
            builder.RegisterType<EmailSender>().AsSelf();

            //Process Monitoring
            builder.RegisterType<ProcessWatcher>().As<IProcessWatcher>();
            builder.RegisterType<CaptureDeviceFactory>().As<ICaptureDeviceFactory>();
            builder.RegisterType<NetworkHelper>().AsSelf();

            //ComputerResourceMonitoring
            builder.RegisterType<ComputerMonitoringManager>().As<IAppManager>();
            builder.RegisterType<JobTimer>().As<ITimer>();

            //Hardware access
            builder.RegisterType<HardwareManager>().As<IHardwareManager>();
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

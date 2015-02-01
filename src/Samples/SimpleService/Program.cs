#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2015 Alphacloud.Net

#endregion

namespace SimpleService
{
    using System;
    using AppServices;
    using Autofac;
    using Autofac.Extras.Quartz;
    using Common.Logging;
    using Jobs;
    using Quartz;
    using Quartz.Spi;
    using Topshelf;
    using Topshelf.Autofac;
    using Topshelf.Quartz;
    using Topshelf.ServiceConfigurators;

    internal static class Program
    {
        private static readonly ILog s_log = LogManager.GetLogger(typeof (Program));
        private static IContainer _container;

        private static int Main(string[] args)
        {
            Console.WriteLine("This sample demostrates how to integrate Quartz, TopShelf and Autofac.");
            s_log.Info("Starting...");
            try
            {
                _container = ConfigureContainer(new ContainerBuilder()).Build();

                HostFactory.Run(conf =>
                {
                    conf.SetServiceName("AutofacExtras.Quartz.Sample");
                    conf.SetDisplayName("Quartz.Net integration for autofac");
                    conf.UseLog4Net();
                    conf.UseAutofacContainer(_container);

                    conf.Service<ServiceCore>(svc =>
                    {
                        svc.ConstructUsingAutofacContainer();
                        svc.WhenStarted(o => o.Start());
                        svc.WhenStopped(o =>
                        {
                            o.Stop();
                            _container.Dispose();
                        });
                        ConfigureBackgroundJobs(svc);
                    });
                });

                s_log.Info("Shutting down...");
                log4net.LogManager.Shutdown();
                return 0;
            }

            catch (Exception ex)
            {
                s_log.Fatal("Unhandled exception", ex);
                log4net.LogManager.Shutdown();
                return 1;
            }
        }

        private static void ConfigureBackgroundJobs(ServiceConfigurator<ServiceCore> svc)
        {
            svc.UsingQuartzJobFactory(() => _container.Resolve<IJobFactory>());

            svc.ScheduleQuartzJob(q =>
            {
                q.WithJob(JobBuilder.Create<HeartbeatJob>()
                    .WithIdentity("Heartbeat", "Maintenance")
                    .Build);
                q.AddTrigger(() => TriggerBuilder.Create()
                    .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(2)).Build());
            });
        }

        internal static ContainerBuilder ConfigureContainer(ContainerBuilder cb)
        {
            cb.RegisterModule(new QuartzAutofacFactoryModule());
            cb.RegisterModule(new QuartzAutofacJobsModule(typeof (HeartbeatJob).Assembly));

            RegisterComponents(cb);
            return cb;
        }

        internal static void RegisterComponents(ContainerBuilder cb)
        {
            // register Service instance
            cb.RegisterType<ServiceCore>().AsSelf();
            // register dependencies
            cb.RegisterType<HeartbeatService>().As<IHeartbeatService>();
        }
    }
}

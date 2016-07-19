#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2016 Alphacloud.Net

#endregion

namespace SimpleService
{
    using System;
    using System.Collections.Specialized;
    using AppServices;
    using Autofac;
    using Autofac.Extras.Quartz;
    using Common.Logging;
    using Jobs;
    using Quartz;
    using Topshelf;
    using Topshelf.Autofac;
    using Topshelf.Quartz;
    using Topshelf.ServiceConfigurators;

    internal static class Program
    {
        static readonly ILog s_log = LogManager.GetLogger(typeof(Program));
        static IContainer _container;

        static int Main(string[] args)
        {
            Console.WriteLine("This sample demostrates how to integrate Quartz, TopShelf and Autofac.");
            s_log.Info("Starting...");
            try
            {
                _container = ConfigureContainer(new ContainerBuilder()).Build();

                ScheduleJobServiceConfiguratorExtensions.SchedulerFactory = () => _container.Resolve<IScheduler>();

                HostFactory.Run(conf => {
                    conf.SetServiceName("AutofacExtras.Quartz.Sample");
                    conf.SetDisplayName("Quartz.Net integration for Autofac");
                    conf.UseLog4Net();
                    conf.UseAutofacContainer(_container);

                    conf.Service<ServiceCore>(svc => {
                        svc.ConstructUsingAutofacContainer();
                        svc.WhenStarted(o => o.Start());
                        svc.WhenStopped(o => {
                            o.Stop();
                            _container.Dispose();
                        });
                        ConfigureScheduler(svc);
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

        static void ConfigureScheduler(ServiceConfigurator<ServiceCore> svc)
        {
            svc.ScheduleQuartzJob(q => {
                s_log.Trace("Configuring jobs");
                q.WithJob(JobBuilder.Create<HeartbeatJob>()
                    .WithIdentity("Heartbeat", "Maintenance")
                    .Build);
                q.AddTrigger(() => TriggerBuilder.Create()
                    .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(2)).Build());
            });
        }

        internal static ContainerBuilder ConfigureContainer(ContainerBuilder cb)
        {
            // configure and register Quartz
            var schedulerConfig = new NameValueCollection {
                {"quartz.threadPool.threadCount", "3"},
                {"quartz.threadPool.threadNamePrefix", "SchedulerWorker"},
                {"quartz.scheduler.threadName", "Scheduler"}
            };

            cb.RegisterModule(new QuartzAutofacFactoryModule {
                ConfigurationProvider = c => schedulerConfig
            });
            cb.RegisterModule(new QuartzAutofacJobsModule(typeof(HeartbeatJob).Assembly));

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

#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2016 Alphacloud.Net

#endregion

namespace SimpleService
{
    using System;
    using Autofac;
    using Configuration;
    using Jobs;
    using Logging;
    using Quartz;
    using Serilog;
    using Topshelf;
    using Topshelf.Autofac;
    using Topshelf.Quartz;
    using Topshelf.ServiceConfigurators;

    internal static class Program
    {
        static ILog _log;

        static void ConfigureScheduler(ServiceConfigurator<ServiceCore> svc)
        {
            svc.ScheduleQuartzJob(q => {
                _log.Trace("Configuring jobs");
                q.WithJob(JobBuilder.Create<HeartbeatJob>()
                    .WithIdentity("Heartbeat", "Maintenance")
                    .Build);
                q.AddTrigger(() => TriggerBuilder.Create()
                    .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(2)).Build());
            });
        }


        static int Main(string[] args)
        {
            Bootstrap.InitializeLogger();
            _log = LogProvider.GetLogger(typeof(Program));

            Console.WriteLine("This sample demonstrates how to integrate Quartz, TopShelf and Autofac.");
            _log.Info("Starting...");
            try
            {
                var cb = new ContainerBuilder();
                // register Service instance
                cb.RegisterType<ServiceCore>().AsSelf();
                var container = Bootstrap.ConfigureContainer(cb).Build();

                ScheduleJobServiceConfiguratorExtensions.SchedulerFactory = () => container.Resolve<IScheduler>();

                HostFactory.Run(conf => {
                    conf.SetServiceName("AutofacExtras.Quartz.Sample");
                    conf.SetDisplayName("Quartz.Net integration for Autofac");
                    conf.UseSerilog(Log.Logger);
                    conf.UseAutofacContainer(container);

                    conf.Service<ServiceCore>(svc => {
                        svc.ConstructUsingAutofacContainer();
                        svc.WhenStarted(o => o.Start());
                        svc.WhenStopped(o => {
                            o.Stop();
                            container.Dispose();
                        });
                        ConfigureScheduler(svc);
                    });
                });

                _log.Info("Shutting down...");

                return 0;
            }

            catch (Exception ex)
            {
                _log.FatalException("Unhandled exception", ex);

                return 1;
            }
        }
    }
}

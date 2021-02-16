#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2021 Alphacloud.Net

#endregion

// ReSharper disable once CheckNamespace
namespace SimpleService.Configuration
{
    using System;
    using System.Collections.Specialized;
    using AppServices;
    using Autofac;
    using Autofac.Extras.Quartz;
    using Jobs;
    using Serilog;
    using Serilog.Sinks.SystemConsole.Themes;


    class Bootstrap
    {
        public static void InitializeLogger()
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
            Log.Logger = log;
        }

        internal static ContainerBuilder ConfigureContainer(ContainerBuilder cb)
        {
            // configure and register Quartz
            var schedulerConfig = new NameValueCollection {
                {"quartz.threadPool.threadCount", "3"},
                {"quartz.scheduler.threadName", "Scheduler"}
            };

            cb.Register(_ => new ScopedDependency("global"))
                .AsImplementedInterfaces()
                .SingleInstance();

            cb.RegisterModule(new QuartzAutofacFactoryModule {
                ConfigurationProvider = _ => schedulerConfig,
                JobScopeConfigurator = (builder, tag) => {
                    // override dependency for job scope
                    builder.Register(_ => new ScopedDependency("job-local "+ DateTime.UtcNow.ToLongTimeString()))
                        .AsImplementedInterfaces()
                        .InstancePerMatchingLifetimeScope(tag);

                }
            });
            cb.RegisterModule(new QuartzAutofacJobsModule(typeof(HeartbeatJob).Assembly));

            RegisterComponents(cb);
            return cb;
        }

        internal static void RegisterComponents(ContainerBuilder cb)
        {
            // register dependencies
            cb.RegisterType<HeartbeatService>().As<IHeartbeatService>();
        }
    }
}

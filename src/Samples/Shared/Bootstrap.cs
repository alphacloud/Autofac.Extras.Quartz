using Autofac;

namespace SimpleService.Configuration
{
    using System.Collections.Specialized;
    using System.Reflection;
    using AppServices;
    using Autofac.Extras.Quartz;
    using JetBrains.Annotations;
    using Jobs;
    using Logging.LogProviders;
    using Serilog;
    using Serilog.Sinks.SystemConsole.Themes;

    internal class Bootstrap
    {
        public static void InitializeLogger()
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
            Log.Logger = log;
            var serilogLogProvider = new SerilogLogProvider();
            SimpleService.Logging.LogProvider.SetCurrentLogProvider(serilogLogProvider);
            Quartz.Logging.LogProvider.SetCurrentLogProvider(serilogLogProvider);
        }

        internal static ContainerBuilder ConfigureContainer(ContainerBuilder cb)
        {
            // configure and register Quartz
            var schedulerConfig = new NameValueCollection {
                {"quartz.threadPool.threadCount", "3"},
                {"quartz.scheduler.threadName", "Scheduler"}
            };

            cb.RegisterModule(new QuartzAutofacFactoryModule
            {
                ConfigurationProvider = c => schedulerConfig
            });
#if NETCOREAPP1_1
            cb.RegisterModule(new QuartzAutofacJobsModule(typeof(HeartbeatJob).GetTypeInfo().Assembly));
#else
            cb.RegisterModule(new QuartzAutofacJobsModule(typeof(HeartbeatJob).Assembly));
#endif

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

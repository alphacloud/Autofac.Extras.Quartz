namespace SimpleService.Configuration
{
    using System.Collections.Specialized;
    using System.Reflection;
    using AppServices;
    using Autofac;
    using Autofac.Extras.Quartz;
    using Jobs;
    using Logging;
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
            LogProvider.SetCurrentLogProvider(new SerilogLogProvider());
        }

        internal static ContainerBuilder ConfigureContainer(ContainerBuilder cb)
        {
            // configure and register Quartz
            var schedulerConfig = new NameValueCollection {
                {"quartz.threadPool.threadCount", "3"},
                {"quartz.scheduler.threadName", "Scheduler"}
            };

            cb.RegisterModule(new QuartzAutofacFactoryModule {
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

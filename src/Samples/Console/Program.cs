#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2019 Alphacloud.Net

#endregion

// ReSharper disable MethodSupportsCancellation

namespace ConsoleScheduler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Quartz;
    using Serilog;
    using SimpleService.Configuration;
    using SimpleService.Jobs;

    static class Program
    {
        static ILogger _log;

        static async Task Main(string[] args)
        {
            Bootstrap.InitializeLogger();
            _log = Serilog.Log.ForContext(typeof(Program));
            IContainer container = null;

            Console.WriteLine("This sample demonstrates how to integrate Quartz and Autofac.");
            _log.Information("Starting...");
            try
            {
                container = Bootstrap.ConfigureContainer(new ContainerBuilder()).Build();

                var job = JobBuilder.Create<HeartbeatJob>().WithIdentity("Heartbeat", "Maintenance").Build();
                var trigger = TriggerBuilder.Create()
                    .WithIdentity("Heartbeat", "Maintenance")
                    .StartNow()
                    .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(2)).Build();
                var cts = new CancellationTokenSource();

                var scheduler = container.Resolve<IScheduler>();
                await scheduler.ScheduleJob(job, trigger, cts.Token).ConfigureAwait(true);

                await scheduler.Start().ConfigureAwait(true);

                Console.WriteLine("======================");
                Console.WriteLine("Press Enter to exit...");
                Console.WriteLine("======================");
                Console.ReadLine();

                cts.Cancel();
                await scheduler.Shutdown().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _log.Fatal(ex, "Unhandled exception caught");
            }

            container?.Dispose();
        }
    }
}

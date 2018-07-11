#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz.Tests
{
    using System;
    using System.Threading.Tasks;
    using global::Quartz;
    using global::Quartz.Impl;
    using global::Quartz.Spi;
    using JetBrains.Annotations;
    using Moq;


    [Obsolete("Interruptable jobs are implemented by means of CancellationToken instead of IInterruptableJob now. Will be removed later.")]
    public class AutofacJobFactoryTests : MockedTestsBase
    {
        private readonly IContainer _container;
        private AutofacJobFactory _factory;
        private Mock<IScheduler> _scheduler;

        public AutofacJobFactoryTests()
        {
            _container = CreateContainer();

            _scheduler = Mockery.Create<IScheduler>();
            _factory = new AutofacJobFactory(_container.Resolve<ILifetimeScope>(), "job-scope");
        }

        private TriggerFiredBundle CreateBundle<TJob>()
        {
            var jobDetailImpl = new JobDetailImpl {JobType = typeof(TJob)};
            var trigger = Mockery.Create<IOperableTrigger>();
            var calendar = Mockery.Create<ICalendar>();
            return new TriggerFiredBundle(jobDetailImpl, trigger.Object, calendar.Object, false, DateTimeOffset.Now,
                DateTimeOffset.Now, null, null);
        }

        public sealed override void Dispose()
        {
            _container?.Dispose();
            base.Dispose();
        }

        private static IContainer CreateContainer()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<NonInterruptableJob>().AsSelf();

            return cb.Build();
        }


        [UsedImplicitly]
        private class NonInterruptableJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}

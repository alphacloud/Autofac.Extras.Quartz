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
    using Alphacloud.Common.Testing.Nunit;
    using FluentAssertions;
    using global::Quartz;
    using global::Quartz.Impl;
    using global::Quartz.Spi;
    using JetBrains.Annotations;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class AutofacJobFactoryTests : MockedTestsBase
    {
        private IContainer _container;
        private AutofacJobFactory _factory;
        private Mock<IScheduler> _scheduler;

        protected override void DoSetup()
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

        protected override void DoTearDown()
        {
            _container.Dispose();
            base.DoTearDown();
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

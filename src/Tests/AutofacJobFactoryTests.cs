#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz.Tests
{
    using System;
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
            cb.RegisterType<InterruptableJob>().AsSelf();

            return cb.Build();
        }

        [Test]
        public void Should_Create_InterruptableWrapper_For_InterruptableJob()
        {
            var bundle = CreateBundle<InterruptableJob>();
            var job = _factory.NewJob(bundle, _scheduler.Object);
            (job as IInterruptableJob).Should()
                .NotBeNull("wrapper should implement IInterruptableJob for interruptable jobs");
        }

        [Test]
        public void Should_Create_NonInterruptableWrapper_For_NonInterruptableJob()
        {
            var bundle = CreateBundle<NonInterruptableJob>();
            var job = _factory.NewJob(bundle, _scheduler.Object);
            (job as IInterruptableJob).Should().BeNull("wrapper should not implement IInterruptableJob");
        }

        [UsedImplicitly]
        private class NonInterruptableJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
            }
        }

        [UsedImplicitly]
        private class InterruptableJob : IJob, IInterruptableJob
        {
            public void Interrupt()
            {
            }

            public void Execute(IJobExecutionContext context)
            {
            }
        }
    }
}

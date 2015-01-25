#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz.Tests
{
    using System.Diagnostics;
    using System.Threading;
    using FluentAssertions;
    using global::Quartz;
    using global::Quartz.Impl;
    using JetBrains.Annotations;
    using NUnit.Framework;

    [TestFixture]
    public class TransparentWrapperTests
    {
        private IContainer _container;
        private StdSchedulerFactory _factory;
        private IScheduler _scheduler;

        [SetUp]
        public void SetUp()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<SampleJob>();
            _container = cb.Build();

            _factory = new StdSchedulerFactory();
            _scheduler = _factory.GetScheduler();
            _scheduler.JobFactory = new AutofacJobFactory(_container.Resolve<ILifetimeScope>(),
                QuartzAutofacFactoryModule.LifetimeScopeName);
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public void CanPersistDataInWrapper()
        {
            var key = new JobKey("job1", "grp1");
            var job1 = JobBuilder.Create<SampleJob>().WithIdentity(key).StoreDurably(true)
                .Build();
            var trigger =
                TriggerBuilder.Create().WithSimpleSchedule(s => s.WithIntervalInSeconds(1).WithRepeatCount(2)).Build();
            job1.JobDataMap.Put("counter", 0);

            _scheduler.ScheduleJob(job1, trigger);
            _scheduler.Start();

            Thread.Sleep(5.Seconds());
            var jobMap = _scheduler.GetJobDetail(key)
                .JobDataMap;

            jobMap.GetIntValue("counter").Should().Be(3);
        }

        [UsedImplicitly]
        [PersistJobDataAfterExecution]
        [DisallowConcurrentExecution]
        private class SampleJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                var data = context.JobDetail.JobDataMap;
                var counter = (int) data["counter"];
                counter++;
                Debug.WriteLine("counter: {0}", counter);
                data["counter"] = counter;
            }
        }

        [UsedImplicitly]
        private class Wrapper : IJob
        {
            private readonly IJob _target;

            public Wrapper(IJob target)
            {
                _target = target;
            }

            public void Execute(IJobExecutionContext context)
            {
                _target.Execute(context);
            }
        }
    }
}

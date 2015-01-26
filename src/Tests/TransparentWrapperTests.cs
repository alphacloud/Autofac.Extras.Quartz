#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2015 Alphacloud.Net

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
        private const string DisallowConcurrentExecution = "key.DisallowConcurrentExecution";
        private IContainer _container;
        private StdSchedulerFactory _factory;
        private IScheduler _scheduler;

        [SetUp]
        public void SetUp()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<SampleJob>();
            cb.RegisterType<LongRunningJob>();
            _container = cb.Build();

            _factory = new StdSchedulerFactory();
            _scheduler = _factory.GetScheduler();
            _scheduler.JobFactory = new AutofacJobFactory(_container.Resolve<ILifetimeScope>(),
                QuartzAutofacFactoryModule.LifetimeScopeName);
        }

        [TearDown]
        public void TearDown()
        {
            _scheduler.Shutdown(waitForJobsToComplete: false);
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

            var jobMap = _scheduler.GetJobDetail(key).JobDataMap;

            jobMap.GetIntValue("counter").Should().Be(3);
        }

        [Test]
        public void ShouldRespectDisallowConcurrentExecutionAttribute()
        {
            var key = new JobKey("job2", "grp2");
            var job1 = JobBuilder.Create<LongRunningJob>().WithIdentity(key).StoreDurably(true)
                .Build();
            var trigger =
                TriggerBuilder.Create().WithSimpleSchedule(s => s.WithIntervalInSeconds(3).WithRepeatCount(10)).Build();

            job1.ConcurrentExecutionDisallowed.Should().BeTrue("Concurrent execution should be disabled in JobDetail");

            job1.JobDataMap["counter"] = 0;
            job1.JobDataMap[DisallowConcurrentExecution] = false;

            _scheduler.ScheduleJob(job1, trigger);
            _scheduler.Start();

            Thread.Sleep(7.Seconds());

            var jobMap = _scheduler.GetJobDetail(key).JobDataMap;

            jobMap.GetBooleanValue(DisallowConcurrentExecution)
                .Should()
                .BeTrue("Concurrent execution was indicated as enabled inside job execution");
        }

        [UsedImplicitly]
        [PersistJobDataAfterExecution]
        private class SampleJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                var data = context.JobDetail.JobDataMap;
                var counter = data.GetIntValue("counter");
                Debug.WriteLine("SampleJob - Counter: {0}, ConcurrentExecutionDisallowed: {1}", counter,
                    context.JobDetail.ConcurrentExecutionDisallowed);
                data["counter"] = counter + 1;
            }
        }

        [UsedImplicitly]
        [PersistJobDataAfterExecution]
        [DisallowConcurrentExecution]
        private class LongRunningJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                var data = context.JobDetail.JobDataMap;
                var counter = data.GetIntValue("counter");
                data[DisallowConcurrentExecution] = context.JobDetail.ConcurrentExecutionDisallowed;
                Debug.WriteLine("Long Runinng Job - counter: {0}, ConcurrentExecutionDisallowed: {1}", counter,
                    context.JobDetail.ConcurrentExecutionDisallowed);
                data["counter"] = counter + 1;
                Thread.Sleep(6.Seconds());
            }
        }
    }
}

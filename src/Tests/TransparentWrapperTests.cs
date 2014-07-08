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
    using JetBrains.Annotations;
    using NUnit.Framework;


    [TestFixture]
    public class TransparentWrapperTests
    {
        [SetUp]
        public void SetUp()
        {
            var cb = new ContainerBuilder();
            cb.RegisterModule(new QuartzAutofacFactoryModule());
            cb.RegisterType<SampleJob>().AsSelf();

            _container = cb.Build();

            _scheduler = _container.Resolve<ISchedulerFactory>().GetScheduler();
        }


        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }


        private IContainer _container;
        private IScheduler _scheduler;


        [PersistJobDataAfterExecution]
        [DisallowConcurrentExecution, UsedImplicitly]
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
            private readonly IJob _targret;


            public Wrapper(IJob targret)
            {
                _targret = targret;
            }


            public void Execute(IJobExecutionContext context)
            {
                _targret.Execute(context);
            }
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
    }
}

#region copyright
// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014 Alphacloud.Net
#endregion
namespace Autofac.Extras.Quartz.Tests
{
    using System;
    using System.Threading;
    using FluentAssertions;
    using global::Quartz;
    using NUnit.Framework;

    [TestFixture]
    public class TransparentWrapperTests
    {
        private SampleJob _job;
        private Wrapper _wrapper;
        private IContainer _container;
        private IScheduler _scheduler;

        [PersistJobDataAfterExecution]
        [DisallowConcurrentExecution]
        class SampleJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                var data = context.JobDetail.JobDataMap;
                var counter = (int)data["counter"];
                Console.WriteLine("counter: {0}", counter);
                counter++;

                data["counter"] = counter;
            }
        }

        class Wrapper : IJob
        {
            private IJob _targret;

            public Wrapper(IJob targret)
            {
                _targret = targret;
            }

            public void Execute(IJobExecutionContext context)
            {
                _targret.Execute(context);
            }

            
        }


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

        [Test]
        public void CanPersistDataInWrapper()
        {
            var job1 = JobBuilder.Create<SampleJob>().WithIdentity("job1", "grp1").Build();
            var trigger =
                TriggerBuilder.Create().WithSimpleSchedule(s => s.WithIntervalInSeconds(10).WithRepeatCount(5)).Build();
            job1.JobDataMap.Put("counter", 1);

            _scheduler.ScheduleJob(job1, trigger);

            _scheduler.Start();

            Thread.Sleep(50.Seconds());
        }
    }
}
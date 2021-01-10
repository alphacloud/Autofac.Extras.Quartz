#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2021 Alphacloud.Net

#endregion

#pragma warning disable 169
namespace Autofac.Extras.Quartz.Tests
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::Quartz;
    using global::Quartz.Impl;
    using global::Quartz.Spi;
    using JetBrains.Annotations;
    using Moq;
    using Xunit;
    using IContainer = Autofac.IContainer;


    public class ScopeTrackerTests : IDisposable
    {
        private readonly IContainer _container;
        private readonly AutofacJobFactory _jobFactory;
        private readonly ILifetimeScope _lifetimeScope;


        [UsedImplicitly]
        [PersistJobDataAfterExecution]
        private class SampleJob : IJob
        {
            [UsedImplicitly] private readonly DisposableDependency _dependency;

            /// <summary>
            ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
            /// </summary>
            public SampleJob([NotNull] DisposableDependency dependency)
            {
                _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
            }

            public Task Execute(IJobExecutionContext context)
            {
                Debug.WriteLine("SampleJob started");
                return Task.CompletedTask;
            }
        }


        [UsedImplicitly]
        private class DisposableDependency : IDisposable
        {
            public static int DisposeCount;
            public static int CreateCount;

            public bool Disposed { get; private set; }

            public DisposableDependency()
            {
                CreateCount++;
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                DisposeCount++;
                Debug.WriteLine("Disposing dependency 0x{0:x}", GetHashCode());
                Disposed = true;
            }
        }


        public ScopeTrackerTests()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<SampleJob>();
            cb.RegisterType<DisposableDependency>().InstancePerLifetimeScope();

            _container = cb.Build();

            _lifetimeScope = _container.Resolve<ILifetimeScope>();
            _jobFactory = new AutofacJobFactory(_lifetimeScope, QuartzAutofacFactoryModule.LifetimeScopeName);
        }

        public void Dispose()
        {
            _container?.Dispose();
        }

        [Fact]
        [Description("See #17")]
        public void ReturnJob_Should_DisposeJobIfMatchingScopeIsMissing()
        {
            var job = new Mock<IJob>();
            var disposableJob = job.As<IDisposable>();
            _jobFactory.ReturnJob(job.Object);

            disposableJob.Verify(d => d.Dispose(), Times.Once, "Job was not disposed");
        }

        [Fact]
        [Description("See #17")]
        public void ReturnJob_Should_HandleMissingMatchingScope()
        {
            var job = new Mock<IJob>();
            Action returnJob = () => _jobFactory.ReturnJob(job.Object);
            returnJob.Should().NotThrow("Failed to handle missing job.");
        }

        [Fact]
        public void ShouldDisposeScopeAfterJobCompletion()
        {
            var jobDetail = new JobDetailImpl("test", typeof(SampleJob));
            var triggerBundle = new TriggerFiredBundle(
                jobDetail, Mock.Of<IOperableTrigger>(),
                Mock.Of<ICalendar>(), false,
                DateTimeOffset.UtcNow,
                null, null, null
            );

            var job = _jobFactory.NewJob(triggerBundle, Mock.Of<IScheduler>());
            _jobFactory.ReturnJob(job);

            _jobFactory.RunningJobs.Should().BeEmpty("Scope was not disposed after job completion");
            DisposableDependency.CreateCount.Should().BeGreaterThan(0, "No dependencies were created");
            DisposableDependency.DisposeCount.Should().BeGreaterThan(0, "Scoped dependencies were not disposed")
                .And.Be(DisposableDependency.CreateCount, "Not all dependencies were disposed");
        }
    }
}

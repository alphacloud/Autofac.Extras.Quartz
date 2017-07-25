#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2016 Alphacloud.Net

#endregion

// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.DelegateAllocation
namespace Autofac.Extras.Quartz.Tests
{
    using System.Reflection;
    using FluentAssertions;
    using global::Quartz;
    using JetBrains.Annotations;
    using NUnit.Framework;

    [TestFixture]
    internal class OptionalJobDependencyTests
    {
        [SetUp]
        public void SetUp()
        {
            _containerBuilder = new ContainerBuilder();
            _containerBuilder.RegisterType<JobDependency>().As<IJobDependency>();
        }

        [TearDown]
        public void TearDown()
        {
            _container?.Dispose();
        }

        private IContainer _container;
        private ContainerBuilder _containerBuilder;

        [UsedImplicitly]
        class TestJobWithOptionalDependency : IJob
        {
            public IJobDependency Dependency { get; [UsedImplicitly] set; }

            public void Execute(IJobExecutionContext context)
            {
            }
        }

        interface IJobDependency
        {
        }

        [UsedImplicitly]
        class JobDependency : IJobDependency
        {
        }

        [Test]
        public void ShouldIgnoreRegisteredOptionalDependencies_UnlessExplicitlyConfigured()
        {
            _containerBuilder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly()));
            _container = _containerBuilder.Build();

            var job = _container.Resolve<TestJobWithOptionalDependency>();
            job.Dependency.Should().BeNull();
        }

        [Test]
        public void ShouldWireRegisteredOptionalDependencies()
        {
            _containerBuilder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly()) {
                AutoWireProperties = true
            });
            _container = _containerBuilder.Build();


            var job = _container.Resolve<TestJobWithOptionalDependency>();
            job.Dependency.Should().NotBeNull("should wire optional dependency");
        }
    }
}

#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2021 Alphacloud.Net

#endregion

// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.DelegateAllocation
namespace Autofac.Extras.Quartz.Tests
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::Quartz;
    using JetBrains.Annotations;
    using Xunit;


    public class OptionalJobDependencyTests: IDisposable

    {
       
        public OptionalJobDependencyTests()
        {
            _containerBuilder = new ContainerBuilder();
            _containerBuilder.RegisterType<JobDependency>().As<IJobDependency>();
        }


        IContainer? _container;
        readonly ContainerBuilder _containerBuilder;

        [UsedImplicitly]
        class TestJobWithOptionalDependency : IJob
        {
            public IJobDependency? Dependency { get; [UsedImplicitly] set; }

            public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
        }

        interface IJobDependency
        {
        }

        [UsedImplicitly]
        class JobDependency : IJobDependency
        {
        }

        [Fact]
        public void ShouldIgnoreRegisteredOptionalDependencies_UnlessExplicitlyConfigured()
        {
            _containerBuilder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly()));
            _container = _containerBuilder.Build();

            var job = _container.Resolve<TestJobWithOptionalDependency>();
            job.Dependency.Should().BeNull();
        }

        [Fact]
        public void ShouldWireRegisteredOptionalDependencies()
        {
            _containerBuilder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly()) {
                AutoWireProperties = true
            });
            _container = _containerBuilder.Build();


            var job = _container.Resolve<TestJobWithOptionalDependency>();
            job.Dependency.Should().NotBeNull("should wire optional dependency");
        }

        public void Dispose()
        {
            _container?.Dispose();
        }
    }
}

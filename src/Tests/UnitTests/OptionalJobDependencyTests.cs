#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2022 Alphacloud.Net

#endregion

// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.DelegateAllocation
namespace Autofac.Extras.Quartz.Tests;

using System.Reflection;
using FluentAssertions;
using Xunit;

public class OptionalJobDependencyTests : IDisposable

{
    readonly ContainerBuilder _containerBuilder;


    IContainer? _container;

    public OptionalJobDependencyTests()
    {
        _containerBuilder = new ContainerBuilder();
        _containerBuilder.RegisterType<JobDependency>().As<IJobDependency>();
    }

    public void Dispose()
    {
        _container?.Dispose();
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
}

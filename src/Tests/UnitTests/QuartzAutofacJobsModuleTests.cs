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

public class QuartzAutofacJobsModuleTests : IDisposable
{
    IContainer? _container;

    public void Dispose()
    {
        _container?.Dispose();
    }

    [Fact]
    public void ShouldApplyJobRegistrationFilter()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly()) {
            JobFilter = type => type != typeof(TestJob2)
        });
        _container = builder.Build();

        _container.IsRegistered<TestJob2>().Should().BeFalse();
    }

    [Fact]
    public void ShouldRegisterAllJobsFromAssembly()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly()));
        _container = builder.Build();

        _container.IsRegistered<TestJob>()
            .Should().BeTrue();
    }


    [UsedImplicitly]
    class TestJob2 : IJob
    {
        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
    }


    [UsedImplicitly]
    class TestJob : IJob
    {
        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
    }
}

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


    public class QuartzAutofacJobsModuleTests: IDisposable
    {
        IContainer _container;


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

        public void Dispose()
        {
            _container?.Dispose();
        }
    }
}

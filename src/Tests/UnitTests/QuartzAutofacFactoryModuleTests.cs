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

using System.Collections.Specialized;
using Shouldly;
using Xunit;

public class QuartzAutofacFactoryModuleTests : IDisposable
{
    readonly IContainer _container;
    readonly QuartzAutofacFactoryModule _quartzAutofacFactoryModule;


    public QuartzAutofacFactoryModuleTests()
    {
        var cb = new ContainerBuilder();
        _quartzAutofacFactoryModule = new QuartzAutofacFactoryModule();
        cb.RegisterModule(_quartzAutofacFactoryModule);

        _container = cb.Build();
    }

    public void Dispose()
    {
        _container.Dispose();
    }

    [Fact]
    public void CanUseGenericAutofacModuleRegistrationSyntax()
    {
        var cb = new ContainerBuilder();
        cb.RegisterModule<QuartzAutofacFactoryModule>();
        cb.Build();
    }

    [Fact]
    public void ShouldExecuteConfigureSchedulerFactoryFunctionIfSet()
    {
        var configuration = new NameValueCollection();
        var customSchedulerName = Guid.NewGuid().ToString();
        configuration[StdSchedulerFactory.PropertySchedulerInstanceName] = customSchedulerName;

        _quartzAutofacFactoryModule.ConfigurationProvider = _ => configuration;

        var scheduler = _container.Resolve<IScheduler>();
        scheduler.SchedulerName.ShouldBe(customSchedulerName);
    }

    [Fact]
    public void ShouldRegisterAutofacJobFactory()
    {
        _container.Resolve<AutofacJobFactory>().ShouldNotBeNull();
        _container.Resolve<IJobFactory>().ShouldBeOfType<AutofacJobFactory>();
        _container.Resolve<IJobFactory>().ShouldBeSameAs(_container.Resolve<AutofacJobFactory>(),
            "should be singleton");
    }

    [Fact]
    public void ShouldRegisterAutofacSchedulerFactory()
    {
        var factory = _container.Resolve<ISchedulerFactory>();
        factory.ShouldBeOfType<AutofacSchedulerFactory>();
    }

    [Fact]
    public void ShouldRegisterFactoryAsSingleton()
    {
        var factory = _container.Resolve<ISchedulerFactory>();
        _container.Resolve<ISchedulerFactory>().ShouldBeSameAs(factory);
    }

    [Fact]
    public void ShouldRegisterSchedulerAsSingleton()
    {
        var scheduler = _container.Resolve<IScheduler>();
        _container.Resolve<IScheduler>().ShouldBeSameAs(scheduler);
    }


    [UsedImplicitly]
    class TestJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}

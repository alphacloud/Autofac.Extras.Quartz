#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz.Tests
{
    using System;
    using System.Collections.Specialized;
    using FluentAssertions;
    using global::Quartz;
    using global::Quartz.Spi;
    using JetBrains.Annotations;
    using NUnit.Framework;


    [TestFixture]
    internal class QuartzAutofacFactoryModuleTests
    {
        [SetUp]
        public void SetUp()
        {
            var cb = new ContainerBuilder();
            _quartzAutofacFactoryModule = new QuartzAutofacFactoryModule();
            cb.RegisterModule(_quartzAutofacFactoryModule);

            _container = cb.Build();
        }


        [TearDown]
        public void TearDown()
        {
            if (_container != null)
                _container.Dispose();
        }


        private IContainer _container;
        private QuartzAutofacFactoryModule _quartzAutofacFactoryModule;


        [UsedImplicitly]
        private class TestJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {}
        }

        [Test]
        public void CanUseGenericAutofacModuleRegistrationSyntax()
        {
            var cb = new ContainerBuilder();
            cb.RegisterModule<QuartzAutofacFactoryModule>();
            cb.Build();
        }

        [Test]
        public void ShouldRegisterAutofacSchedulerFactory()
        {
            var factory = _container.Resolve<ISchedulerFactory>();
            factory.Should().BeOfType<AutofacSchedulerFactory>();
        }


        [Test]
        public void ShouldRegisterFactoryAsSingleton()
        {
            var factory = _container.Resolve<ISchedulerFactory>();
            _container.Resolve<ISchedulerFactory>().Should().BeSameAs(factory);
        }


        [Test]
        public void ShouldRegisterAutofacJobFactory()
        {
            _container.Resolve<AutofacJobFactory>().Should().NotBeNull();
            _container.Resolve<IJobFactory>().Should().BeOfType<AutofacJobFactory>();
            _container.Resolve<IJobFactory>().Should().BeSameAs(_container.Resolve<AutofacJobFactory>(), 
                "should be singleton");

        }
        [Test]
        public void ShouldRegisterSchedulerAsSingleton()
        {
            var scheduler = _container.Resolve<IScheduler>();
            _container.Resolve<IScheduler>().Should().BeSameAs(scheduler);
        }

        [Test]
        public void ShouldExecuteConfigureSchedulerFactoryFunctionIfSet()
        {
            var configuration = new NameValueCollection();
            var customSchedulerName = Guid.NewGuid().ToString();
            configuration["quartz.scheduler.instanceName"] = customSchedulerName;

            _quartzAutofacFactoryModule.ConfigureSchedulerFactory = autofacSchedulerFactory => autofacSchedulerFactory.Initialize(configuration);
            
            var scheduler = _container.Resolve<IScheduler>();
            scheduler.SchedulerName.Should().BeEquivalentTo(customSchedulerName);
        }
    }
}
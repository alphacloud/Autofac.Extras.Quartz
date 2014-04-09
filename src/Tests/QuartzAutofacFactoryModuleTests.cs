#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz.Tests
{
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
            cb.RegisterModule(new QuartzAutofacFactoryModule());

            _container = cb.Build();
        }


        [TearDown]
        public void TearDown()
        {
            if (_container != null)
                _container.Dispose();
        }


        private IContainer _container;


        [UsedImplicitly]
        private class TestJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {}
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
    }
}
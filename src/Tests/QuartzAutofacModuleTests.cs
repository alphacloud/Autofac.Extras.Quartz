#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz.Tests
{
    using System.Reflection;
    using FluentAssertions;
    using global::Quartz;
    using JetBrains.Annotations;
    using NUnit.Framework;

    [TestFixture]
    internal class QuartzAutofacJobsModuleTests
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var cb = new ContainerBuilder();
            cb.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly()));

            _container = cb.Build();
        }

        [TearDown]
        public void TearDown()
        {
            _container?.Dispose();
        }

        [Test]
        public void ShouldRegisterJobsFromAssembly()
        {
            _container.IsRegistered<TestJob>()
                .Should().BeTrue();
        }

        [UsedImplicitly]
        private class TestJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
            }
        }
    }
}

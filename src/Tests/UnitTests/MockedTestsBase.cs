#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2018 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz.Tests
{
    using System;
    using Moq;

    /// <summary>
    ///     Base class for tests with mocks.
    /// </summary>
    public abstract class MockedTestsBase : IDisposable
    {
        /// <summary>
        ///     Mock repository.
        /// </summary>
        protected MockRepository Mockery { get; }

        /// <summary>
        ///     Setup tests
        /// </summary>
        public MockedTestsBase()
        {
            Mockery = new MockRepository(MockBehavior.Default);
        }

        public virtual void Dispose()
        {
            Mockery.Verify();
        }
    }


}

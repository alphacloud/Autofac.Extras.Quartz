#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2018 Alphacloud.Net

#endregion

namespace Alphacloud.Common.Testing.Nunit
{
    using Moq;
    using NUnit.Framework;

    //// ReSharper disable InconsistentNaming

    /// <summary>
    ///   Base class for tests with mocks.
    /// </summary>
    [TestFixture]
    internal abstract class MockedTestsBase
    {
        #region Setup/Teardown

        /// <summary>
        ///   Setup tests
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Mockery = new MockRepository(MockBehavior.Default);
            DoSetup();
        }


        /// <summary>
        ///   Cleanup tests
        /// </summary>
        /// <exception cref="MockException">One or more mocks had expectations that were not satisfied.</exception>
        [TearDown]
        public void TearDown()
        {
            DoTearDown();
            Mockery.Verify();
        }

        #endregion

        /// <summary>
        ///   Mock repository.
        /// </summary>
        protected MockRepository Mockery { get; private set; }


        /// <summary>
        ///   Performs test setup.
        /// </summary>
        protected abstract void DoSetup();


        /// <summary>
        ///   Performs test tear-down.
        /// </summary>
        protected virtual void DoTearDown()
        {
            // do nothing
        }
    }

//// ReSharper restore InconsistentNaming
}

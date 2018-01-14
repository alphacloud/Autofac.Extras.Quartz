#region copyright

// Copyright 2013 Alphacloud.Net
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

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

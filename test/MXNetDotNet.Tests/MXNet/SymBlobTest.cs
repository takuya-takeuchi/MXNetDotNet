using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MXNetDotNet.Tests.MXNet
{

    [TestClass]
    public class SymBlobTest : TestBase
    {

        #region Constructors

        [TestMethod]
        public void Constructor()
        {
            var blob = new SymBlob();
            this.DisposeAndCheckDisposedState(blob);
        }

        [TestMethod]
        public void Constructor2()
        {
            var blob = new SymBlob(IntPtr.Zero);
            this.DisposeAndCheckDisposedState(blob);
        }

        #endregion

        #region Properties

        [TestMethod]
        public void Handle()
        {
            var blob = new SymBlob(IntPtr.Zero);
            this.DisposeAndCheckDisposedState(blob);
        }

        #endregion

        #region Methods

        [TestMethod]
        public void Dispose()
        {
            var blob = new SymBlob(IntPtr.Zero);
            blob.Dispose();

            try
            {
                blob.Dispose();
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        #endregion

    }

}

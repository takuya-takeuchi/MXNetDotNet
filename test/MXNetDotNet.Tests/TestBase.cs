using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MXNetDotNet.Tests
{

    public abstract class TestBase
    {

        #region Events
        #endregion

        #region Fields
        #endregion

        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Methods


        public void DisposeAndCheckDisposedState(DisposableMXNetObject obj)
        {
            obj.Dispose();
            Assert.IsTrue(obj.IsDisposed);
            Assert.IsTrue(obj.NativePtr == IntPtr.Zero);
        }

        public void DisposeAndCheckDisposedState(IEnumerable<DisposableMXNetObject> objs)
        {
            foreach (var obj in objs)
                this.DisposeAndCheckDisposedState(obj);
        }

        #endregion

    }

}
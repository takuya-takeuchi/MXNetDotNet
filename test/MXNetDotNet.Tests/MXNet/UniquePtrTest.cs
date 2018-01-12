using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MXNetDotNet.Tests.MXNet
{

    [TestClass]
    public class UniquePtrTest : TestBase
    {

        #region Constructors

        [TestMethod]
        public void Constructor()
        {
            var inputData = Symbol.Variable("data");
            var ptr = new UniquePtr<Symbol>(inputData);
            ptr.Dispose();
            Assert.IsTrue(ptr.IsDisposed);
        }

        #endregion

        #region Properties

        [TestMethod]
        public void IsDisposed()
        {
            var inputData1 = Symbol.Variable("data");
            var ptr1 = new UniquePtr<Symbol>(inputData1);
            Assert.IsFalse(ptr1.IsDisposed);
            ptr1.Dispose();
            Assert.IsTrue(ptr1.IsDisposed);
        }

        [TestMethod]
        public void IsOwner()
        {
            var inputData1 = Symbol.Variable("data");
            var ptr1 = new UniquePtr<Symbol>(inputData1);
            Assert.IsTrue(ptr1.IsOwner);
        }

        [TestMethod]
        public void Ptr()
        {
            var inputData1 = Symbol.Variable("data");
            var ptr1 = new UniquePtr<Symbol>(inputData1);
            var obj1 = ptr1.Ptr;
            Assert.AreEqual(inputData1, obj1);

            var inputData2 = Symbol.Variable("data");
            var ptr2 = new UniquePtr<Symbol>(inputData2);
            var obj2 = ptr2.Ptr;
            Assert.AreNotEqual(inputData1, obj2);
        }

        #endregion

        #region Methods

        [TestMethod]
        public void Dispose()
        {
            var inputData1 = Symbol.Variable("data");
            var ptr = new UniquePtr<Symbol>(inputData1);
            ptr.Dispose();

            try
            {
                ptr.Dispose();
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void Move()
        {
            var inputData1 = Symbol.Variable("data");
            var ptr1 = new UniquePtr<Symbol>(inputData1);
            Assert.IsTrue(ptr1.IsOwner);

            UniquePtr<Symbol>.Move(ptr1, out var ptr2);
            Assert.IsFalse(ptr1.IsOwner);
            Assert.IsTrue(ptr2.IsOwner);
        }

        #endregion

    }

}
using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MXNetDotNet.Tests.MXNet
{

    [TestClass]
    public class SymbolTest : TestBase
    {

        #region Initialize

        private Symbol _LeNet;

        [TestInitialize]
        public void TestInitialize()
        {
            this._LeNet = Create();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this._LeNet?.Dispose();
        }

        #endregion

        #region Constructors

        [TestMethod]
        public void Constructor()
        {
            var variable = Symbol.Variable("data");
            var inputData1 = new Symbol(variable.NativePtr);
            this.DisposeAndCheckDisposedState(inputData1);
        }

        [TestMethod]
        public void Constructor2()
        {
            var inputData1 = new Symbol();
            this.DisposeAndCheckDisposedState(inputData1);
        }

        [TestMethod]
        public void Constructor3()
        {
            var inputData1 = new Symbol("data");
            this.DisposeAndCheckDisposedState(inputData1);

            try
            {
                inputData1 = new Symbol("hoge");
                Assert.Fail();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Properties

        [TestMethod]
        public void Indexer()
        {
            var lenet = this._LeNet;
            var ptr1 = lenet[0];
            var ptr2 = lenet[0];
            Assert.AreEqual(ptr1.Name, ptr2.Name);
            Assert.AreNotEqual(ptr1, ptr2);

            var le = lenet[1];
            Console.WriteLine(le.NativePtr);
            Assert.AreEqual(le.NativePtr, IntPtr.Zero);
        }

        #endregion

        #region Methods

        [TestMethod]
        public void Dispose()
        {
            var inputData1 = Symbol.Variable("data");
            inputData1.Dispose();

            try
            {
                inputData1.Dispose();
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void LoadJSON()
        {
            var filename = this.GetTestDirectory();

            filename = Path.Combine(filename, "LeNet.json");
            var json = this._LeNet.ToJSON();

            File.Delete(filename);
            File.WriteAllText(filename, json, Encoding.UTF8);

            var symbol = Symbol.LoadJSON(File.ReadAllText(filename));
            this.DisposeAndCheckDisposedState(symbol);
        }

        [TestMethod]
        public void Load()
        {
            var filename = this.GetTestDirectory();

            filename = Path.Combine(filename, "LeNet.symbol");

            File.Delete(filename);
            this._LeNet.Save(filename);

            var symbol = Symbol.Load(filename);
            this.DisposeAndCheckDisposedState(symbol);
        }

        [TestMethod]
        public void ToJSON()
        {
            var json = this._LeNet.ToJSON();
            Assert.IsFalse(string.IsNullOrWhiteSpace(json));
            Console.WriteLine(json);
        }

        [TestMethod]
        public void Save()
        {
            var filename = this.GetTestDirectory();
            filename = Path.Combine(filename, "Save.symbol");

            this._LeNet.Save(filename);
            Assert.IsTrue(File.Exists(filename));
        }

        #region Helpers

        private static Symbol Create()
        {
            Symbol data = Symbol.Variable("data");
            Symbol data_label = Symbol.Variable("data_label");
            Symbol conv1_w = new Symbol("conv1_w"); var conv1_b = new Symbol("conv1_b");
            Symbol conv2_w = new Symbol("conv2_w"); var conv2_b = new Symbol("conv2_b");
            Symbol conv3_w = new Symbol("conv3_w"); var conv3_b = new Symbol("conv3_b");
            Symbol fc1_w = new Symbol("fc1_w"); var fc1_b = new Symbol("fc1_b");
            Symbol fc2_w = new Symbol("fc2_w"); var fc2_b = new Symbol("fc2_b");

            Symbol conv1 = Operators.Convolution("conv1", data, conv1_w, conv1_b, new Shape(5, 5), 20);
            Symbol tanh1 = Operators.Activation("tanh1", conv1, ActivationActType.Tanh);
            Symbol pool1 = Operators.Pooling("pool1", tanh1, new Shape(2, 2), PoolingPoolType.Max,
              false, false, PoolingPoolingConvention.Valid, new Shape(2, 2));

            Symbol conv2 = Operators.Convolution("conv2", pool1, conv2_w, conv2_b, new Shape(5, 5), 50);
            Symbol tanh2 = Operators.Activation("tanh2", conv2, ActivationActType.Tanh);
            Symbol pool2 = Operators.Pooling("pool2", tanh2, new Shape(2, 2), PoolingPoolType.Max,
              false, false, PoolingPoolingConvention.Valid, new Shape(2, 2));

            Symbol conv3 = Operators.Convolution("conv3", pool2, conv3_w, conv3_b, new Shape(2, 2), 500);
            Symbol tanh3 = Operators.Activation("tanh3", conv3, ActivationActType.Tanh);
            Symbol pool3 = Operators.Pooling("pool3", tanh3, new Shape(2, 2), PoolingPoolType.Max,
              false, false, PoolingPoolingConvention.Valid, new Shape(1, 1));

            Symbol flatten = Operators.Flatten("flatten", pool3);
            Symbol fc1 = Operators.FullyConnected("fc1", flatten, fc1_w, fc1_b, 500);
            Symbol tanh4 = Operators.Activation("tanh4", fc1, ActivationActType.Tanh);
            Symbol fc2 = Operators.FullyConnected("fc2", tanh4, fc2_w, fc2_b, 10);

            Symbol lenet = Operators.SoftmaxOutput("softmax", fc2, data_label);

            return lenet;
        }

        private string GetTestDirectory()
        {
            var directory = $"{this.GetType().Name}";
            Directory.CreateDirectory(directory);
            return directory;
        }

        #endregion

        #endregion

    }

}

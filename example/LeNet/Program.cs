/*
 * This sample program is ported by C# from incubator-mxnet/blob/master/cpp-package/example/lenet.cpp.
*/

using System;
using System.Collections.Generic;
using System.IO;
using MXNetDotNet;

using size_t = System.UInt64;

namespace LeNet
{

    internal class Program
    {

        private sealed class Lenet
        {

            #region Fields

            private readonly Context _CtxCpu;

            private readonly Context _CtxDev;

            private readonly Dictionary<string, NDArray> _ArgsMap = new Dictionary<string, NDArray>();

            private NDArray _TrainData;

            private NDArray _TrainLabel;

            private NDArray _ValData;

            private NDArray _ValLabel;

            #endregion

            #region Constructors

            public Lenet()
            {
                this._CtxCpu = new Context(DeviceType.CPU, 0);
                this._CtxDev = new Context(DeviceType.CPU, 0);
            }

            #endregion

            #region Methods

            public void Run()
            {
                /*
                 * LeCun, Yann, Leon Bottou, Yoshua Bengio, and Patrick Haffner.
                 * "Gradient-based learning applied to document recognition."
                 * Proceedings of the IEEE (1998)
                 * */

                /*define the symbolic net*/
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

                foreach (var s in lenet.ListArguments())
                    Logging.LG(s);

                /*setup basic configs*/
                var valFold = 1;
                var W = 28u;
                var H = 28u;
                const int batchSize = 42;
                const int maxEpoch = 100000;
                const float learningRate = 1e-4f;
                const float weightDecay = 1e-4f;

                /*prepare the data*/
                var dataVec = new List<float>();
                var labelVec = new List<float>();
                var dataCount = GetData(dataVec, labelVec);
                var dptr = dataVec.ToArray();
                var lptr = labelVec.ToArray();
                NDArray dataArray = new NDArray(new Shape((uint)dataCount, 1, W, H), this._CtxCpu,
                                             false);  // store in main memory, and copy to
                                                      // device memory while training
                NDArray labelArray = new NDArray(new Shape((uint)dataCount), this._CtxCpu,
                            false);  // it's also ok if just store them all in device memory
                dataArray.SyncCopyFromCPU(dptr, (uint)(dataCount * W * H));
                labelArray.SyncCopyFromCPU(lptr, dataCount);
                dataArray.WaitToRead();
                labelArray.WaitToRead();

                var trainNum = (uint)(dataCount * (1 - valFold / 10.0));
                this._TrainData = dataArray.Slice(0, trainNum);
                this._TrainLabel = labelArray.Slice(0, trainNum);
                this._ValData = dataArray.Slice(trainNum, (uint)dataCount);
                this._ValLabel = labelArray.Slice(trainNum, (uint)dataCount);

                Logging.LG("here read fin");

                /*init some of the args*/
                // map<string, NDArray> args_map;
                this._ArgsMap["data"] = dataArray.Slice(0, (uint)batchSize).Copy(this._CtxDev);
                this._ArgsMap["data_label"] = labelArray.Slice(0, (uint)batchSize).Copy(this._CtxDev);
                NDArray.WaitAll();

                Logging.LG("here slice fin");

                /*
                 * we can also feed in some of the args other than the input all by
                 * ourselves,
                 * fc2-w , fc1-b for example:
                 * */
                // args_map["fc2_w"] =
                // NDArray(mshadow.Shape2(500, 4 * 4 * 50), ctx_dev, false);
                // NDArray.SampleGaussian(0, 1, &args_map["fc2_w"]);
                // args_map["fc1_b"] = NDArray(mshadow.Shape1(10), ctx_dev, false);
                // args_map["fc1_b"] = 0;

                lenet.InferArgsMap(this._CtxDev, this._ArgsMap, this._ArgsMap);
                var opt = OptimizerRegistry.Find("ccsgd");
                opt.SetParam("momentum", 0.9)
                   .SetParam("rescale_grad", 1.0)
                   .SetParam("clip_gradient", 10)
                   .SetParam("lr", learningRate)
                   .SetParam("wd", weightDecay);

                using (var exe = lenet.SimpleBind(this._CtxDev, this._ArgsMap))
                {
                    var argNames = lenet.ListArguments();

                    for (var iter = 0; iter < maxEpoch; ++iter)
                    {
                        size_t startIndex = 0;
                        while (startIndex < trainNum)
                        {
                            if (startIndex + (size_t)batchSize > trainNum)
                                startIndex = trainNum - (size_t)batchSize;

                            using (var slice = this._TrainData.Slice((uint)startIndex, (uint)(startIndex + batchSize)))
                            {
                                this._ArgsMap["data"]?.Dispose();
                                this._ArgsMap["data"] = slice.Copy(this._CtxDev);
                            }
                            using (var slice = this._TrainLabel.Slice((uint)startIndex, (uint)(startIndex + (ulong)batchSize)))
                            {
                                this._ArgsMap["data_label"]?.Dispose();
                                this._ArgsMap["data_label"] = slice.Copy(this._CtxDev);
                            }

                            startIndex += (size_t)batchSize;
                            NDArray.WaitAll();

                            exe.Forward(true);
                            exe.Backward();

                            // Update parameters
                            for (var i = 0; i < argNames.Count; ++i)
                            {
                                if (argNames[i] == "data" || argNames[i] == "data_label")
                                    continue;

                                var weight = exe.ArgmentArrays[i];
                                var grad = exe.GradientArrays[i];
                                opt.Update(i, weight, grad);
                            }
                        }

                        Logging.LG($"Iter {iter}, accuracy: {ValAccuracy(batchSize * 10, lenet)}");
                    }
                }
            }

            #region Helpers

            private size_t GetData(List<float> data, List<float> label)
            {
                const string trainDataPath = "./train.csv";
                var lines = File.ReadAllLines(trainDataPath);

                // ignore the header
                size_t n = 0;
                for (var index = 1; index < lines.Length; index++)
                {
                    var line = lines[index];
                    var values = line.Split(",");
                    label.Add(float.Parse(values[0]));
                    if (index == 1)
                        data.Capacity = values.Length * lines.Length;
                    for (var i = 1; i < values.Length; i++)
                        data.Add(float.Parse(values[i]) / 256.0f);

                    n++;
                }

                data.TrimExcess();

                GC.Collect(2, GCCollectionMode.Forced, true, true);

                return n;
            }

            private float ValAccuracy(int batchSize, Symbol lenet)
            {
                var valNum = this._ValData.GetShape()[0];

                size_t correctCount = 0;
                size_t allCount = 0;

                size_t startIndex = 0;
                while (startIndex < valNum)
                {
                    if (startIndex + (size_t)batchSize > valNum)
                        startIndex = valNum - (size_t)batchSize;

                    NDArray slice;
                    using (slice = this._ValData.Slice((uint)startIndex, (uint)(startIndex + (size_t)batchSize)))
                    {
                        this._ArgsMap["data"]?.Dispose();
                        this._ArgsMap["data"] = slice.Copy(this._CtxDev);
                    }
                    using (slice = this._ValLabel.Slice((uint)startIndex, (uint)(startIndex + (size_t)batchSize)))
                    {
                        this._ArgsMap["data_label"]?.Dispose();
                        this._ArgsMap["data_label"] = slice.Copy(this._CtxDev);
                    }

                    startIndex += (size_t)batchSize;
                    NDArray.WaitAll();

                    using (var exe = lenet.SimpleBind(this._CtxDev, this._ArgsMap))
                    {
                        exe.Forward(false);

                        var @out = exe.Outputs;
                        using (var outCpu = @out[0].Copy(this._CtxCpu))
                        {
                            NDArray labelCpu;
                            using (slice = this._ValLabel.Slice((uint)(startIndex - (size_t)batchSize), (uint)startIndex))
                                labelCpu = slice.Copy(this._CtxCpu);

                            NDArray.WaitAll();

                            unsafe
                            {
                                var dptrOut = (float*)outCpu.GetData();
                                var dptrLabel = (float*)labelCpu.GetData();
                                for (var i = 0; i < batchSize; ++i)
                                {
                                    var label = dptrLabel[i];
                                    var catNum = outCpu.GetShape()[1];
                                    float pLabel = 0;
                                    var maxP = dptrOut[i * catNum];
                                    for (var j = 0; j < catNum; ++j)
                                    {
                                        var p = dptrOut[i * catNum + j];
                                        if (maxP < p)
                                        {
                                            pLabel = j;
                                            maxP = p;
                                        }
                                    }

                                    if (Math.Abs(label - pLabel) < float.Epsilon)
                                        correctCount++;
                                }
                            }

                            allCount += (size_t)batchSize;
                        }

                        // Executor does not dispose outputs objects
                        foreach (var array in @out)
                            array?.Dispose();
                    }
                }

                return (float)(correctCount * 1.0 / allCount);
            }

            #endregion

            #endregion

        }

        private static void Main()
        {
            var lenet = new Lenet();
            lenet.Run();
            MXNet.MXNotifyShutdown();
        }

    }
}

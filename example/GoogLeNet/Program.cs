/*
 * This sample program is ported by C# from incubator-mxnet/blob/master/cpp-package/example/googlenet.cpp.
*/

using System;
using System.Collections.Generic;
using Examples;
using MXNetDotNet;

namespace GoogLeNet
{

    internal class Program
    {

        #region Methods

        private static int Main(string[] args)
        {
            var maxEpoch = args.Length > 0 && int.TryParse(args[0], out var ret) ? ret : 100;
            const int batchSize = 50;
            const float learningRate = 1e-4f;
            const float weightDecay = 1e-4f;

            try
            {
                var googlenet = GoogleNetSymbol(101 + 1); // +1 is BACKGROUND_Google
                var argsMap = new Dictionary<string, NDArray>();
                var auxMap = new Dictionary<string, NDArray>();

                // change device type if you want to use GPU
                var context = Context.Cpu();

                argsMap["data"] = new NDArray(new Shape(batchSize, 3, 256, 256), context);
                argsMap["data_label"] = new NDArray(new Shape(batchSize), context);
                googlenet.InferArgsMap(Context.Cpu(), argsMap, argsMap);

                string[] dataFiles = { "./data/mnist_data/train-images-idx3-ubyte",
                                         "./data/mnist_data/train-labels-idx1-ubyte",
                                         "./data/mnist_data/t10k-images-idx3-ubyte",
                                         "./data/mnist_data/t10k-labels-idx1-ubyte"
                                     };

                using (var trainIter = new MXDataIter("MNISTIter"))
                {
                    if (!Utils.SetDataIter(trainIter, "Train", dataFiles, batchSize))
                        return 1;

                    using (var valIter = new MXDataIter("MNISTIter"))
                    {
                        if (!Utils.SetDataIter(valIter, "Label", dataFiles, batchSize))
                            return 1;

                        using (var opt = OptimizerRegistry.Find("sgd"))
                        {
                            opt.SetParam("momentum", 0.9)
                               .SetParam("rescale_grad", 1.0 / batchSize)
                               .SetParam("clip_gradient", 10)
                               .SetParam("lr", learningRate)
                               .SetParam("wd", weightDecay);

                            using (var exec = googlenet.SimpleBind(Context.Cpu(), argsMap))
                            {
                                var argNames = googlenet.ListArguments();

                                for (var iter = 0; iter < maxEpoch; ++iter)
                                {
                                    Logging.LG($"Epoch: {iter}");

                                    trainIter.Reset();
                                    while (trainIter.Next())
                                    {
                                        var dataBatch = trainIter.GetDataBatch();
                                        dataBatch.Data.CopyTo(argsMap["data"]);
                                        dataBatch.Label.CopyTo(argsMap["data_label"]);
                                        NDArray.WaitAll();
                                        exec.Forward(true);
                                        exec.Backward();
                                        for (var i = 0; i < argNames.Count; ++i)
                                        {
                                            if (argNames[i] == "data" || argNames[i] == "data_label")
                                                continue;

                                            var weight = exec.ArgmentArrays[i];
                                            var grad = exec.GradientArrays[i];
                                            opt.Update(i, weight, grad);
                                        }
                                    }

                                    var acu = new Accuracy();
                                    valIter.Reset();
                                    while (valIter.Next())
                                    {
                                        var dataBatch = valIter.GetDataBatch();
                                        dataBatch.Data.CopyTo(argsMap["data"]);
                                        dataBatch.Label.CopyTo(argsMap["data_label"]);
                                        NDArray.WaitAll();
                                        exec.Forward(false);
                                        NDArray.WaitAll();
                                        acu.Update(dataBatch.Label, exec.Outputs[0]);
                                    }

                                    Logging.LG($"Accuracy: {acu.Get()}");
                                }
                            }

                            MXNet.MXNotifyShutdown();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return 0;
        }

        #region Helpers

        private static Symbol ConvFactory(Symbol data,
                                          int numFilter,
                                          Shape kernel)
        {
            return ConvFactory(data, numFilter, kernel, new Shape(1, 1));
        }

        private static Symbol ConvFactory(Symbol data,
                                          int numFilter,
                                          Shape kernel,
                                          Shape stride)
        {
            return ConvFactory(data, numFilter, kernel, stride, new Shape(0, 0));
        }

        private static Symbol ConvFactory(Symbol data,
                                          int numFilter,
                                          Shape kernel,
                                          Shape stride,
                                          Shape pad,
                                          string name = "",
                                          string suffix = "")
        {
            var conv_w = new Symbol($"conv_{name}{suffix}_w");
            var conv_b = new Symbol($"conv_{name}{suffix}_b");

            var conv = Operators.Convolution($"conv_{name}{suffix}", data,
                                             conv_w, conv_b, kernel,
                                             (uint)numFilter, stride, new Shape(1, 1), pad);
            return Operators.Activation($"relu_{name}{suffix}", conv, ActivationActType.Relu);
        }

        private static Symbol InceptionFactory(Symbol data,
                                               int num_1x1,
                                               int num_3x3red,
                                               int num_3x3,
                                               int num_d5x5red,
                                               int num_d5x5,
                                               PoolingPoolType pool,
                                               int proj,
                                               string name)
        {
            var c1x1 = ConvFactory(data, num_1x1, new Shape(1, 1),
                                   new Shape(1, 1), new Shape(0, 0), name + "_1x1");

            var c3x3r = ConvFactory(data, num_3x3red, new Shape(1, 1),
                                    new Shape(1, 1), new Shape(0, 0), name + "_3x3", "_reduce");

            var c3x3 = ConvFactory(c3x3r, num_3x3, new Shape(3, 3),
                                   new Shape(1, 1), new Shape(1, 1), name + "_3x3");

            var cd5x5r = ConvFactory(data, num_d5x5red, new Shape(1, 1),
                                     new Shape(1, 1), new Shape(0, 0), name + "_5x5", "_reduce");

            var cd5x5 = ConvFactory(cd5x5r, num_d5x5, new Shape(5, 5),
                                    new Shape(1, 1), new Shape(2, 2), name + "_5x5");

            var pooling = Operators.Pooling(name + "_pool", data, new Shape(3, 3), pool,
                                            false, false, PoolingPoolingConvention.Valid,
                                            new Shape(1, 1), new Shape(1, 1));

            var cproj = ConvFactory(pooling, proj, new Shape(1, 1),
                                    new Shape(1, 1), new Shape(0, 0), name + "_proj");

            var lst = new List<Symbol>();
            lst.Add(c1x1);
            lst.Add(c3x3);
            lst.Add(cd5x5);
            lst.Add(cproj);
            return Operators.Concat("ch_concat_" + name + "_chconcat", lst, lst.Count);
        }

        private static Symbol GoogleNetSymbol(int numClasses)
        {
            // data and label
            var data = Symbol.Variable("data");
            var data_label = Symbol.Variable("data_label");

            var conv1 = ConvFactory(data, 64, new Shape(7, 7), new Shape(2, 2), new Shape(3, 3), "conv1");
            var pool1 = Operators.Pooling("pool1", conv1, new Shape(3, 3), PoolingPoolType.Max,
                                          false, false, PoolingPoolingConvention.Valid, new Shape(2, 2));
            var conv2 = ConvFactory(pool1, 64, new Shape(1, 1), new Shape(1, 1),
                new Shape(0, 0), "conv2");
            var conv3 = ConvFactory(conv2, 192, new Shape(3, 3), new Shape(1, 1), new Shape(1, 1), "conv3");
            var pool3 = Operators.Pooling("pool3", conv3, new Shape(3, 3), PoolingPoolType.Max,
                                          false, false, PoolingPoolingConvention.Valid, new Shape(2, 2));

            var in3a = InceptionFactory(pool3, 64, 96, 128, 16, 32, PoolingPoolType.Max, 32, "in3a");
            var in3b = InceptionFactory(in3a, 128, 128, 192, 32, 96, PoolingPoolType.Max, 64, "in3b");
            var pool4 = Operators.Pooling("pool4", in3b, new Shape(3, 3), PoolingPoolType.Max,
                                          false, false, PoolingPoolingConvention.Valid, new Shape(2, 2));
            var in4a = InceptionFactory(pool4, 192, 96, 208, 16, 48, PoolingPoolType.Max, 64, "in4a");
            var in4b = InceptionFactory(in4a, 160, 112, 224, 24, 64, PoolingPoolType.Max, 64, "in4b");
            var in4c = InceptionFactory(in4b, 128, 128, 256, 24, 64, PoolingPoolType.Max, 64, "in4c");
            var in4d = InceptionFactory(in4c, 112, 144, 288, 32, 64, PoolingPoolType.Max, 64, "in4d");
            var in4e = InceptionFactory(in4d, 256, 160, 320, 32, 128, PoolingPoolType.Max, 128, "in4e");
            var pool5 = Operators.Pooling("pool5", in4e, new Shape(3, 3), PoolingPoolType.Max,
                                          false, false, PoolingPoolingConvention.Valid, new Shape(2, 2));
            var in5a = InceptionFactory(pool5, 256, 160, 320, 32, 128, PoolingPoolType.Max, 128, "in5a");
            var in5b = InceptionFactory(in5a, 384, 192, 384, 48, 128, PoolingPoolType.Max, 128, "in5b");
            var pool6 = Operators.Pooling("pool6", in5b, new Shape(7, 7), PoolingPoolType.Avg,
                                          false, false, PoolingPoolingConvention.Valid, new Shape(1, 1));

            var flatten = Operators.Flatten("flatten", pool6);

            var fc1_w = new Symbol("fc1_w");
            var fc1_b = new Symbol("fc1_b");
            var fc1 = Operators.FullyConnected("fc1", flatten, fc1_w, fc1_b, numClasses);

            return Operators.SoftmaxOutput("softmax", fc1, data_label);
        }

        #endregion

        #endregion

    }
}

/*
 * This sample program is ported by C# from incubator-mxnet/blob/master/cpp-package/example/inception_bn.cpp.
*/

using System.Collections.Generic;
using MXNetDotNet;

namespace InceptionBN
{

    internal class Program
    {

        #region Methods

        private static void Main()
        {
            const uint batchSize = 40u;
            const int maxEpoch = 100;
            const float learningRate = 1e-4f;
            const float weightDecay = 1e-4f;

            var inceptionBnNet = InceptionSymbol(10);
            var argsMap = new SortedDictionary<string, NDArray>();
            var auxMap = new SortedDictionary<string, NDArray>();

            // change device type if you want to use GPU
            var context = Context.Gpu();

            argsMap["data"] = new NDArray(new Shape(batchSize, 3, 224, 224), context);
            argsMap["data_label"] = new NDArray(new Shape(batchSize), context);
            inceptionBnNet.InferArgsMap(context, argsMap, argsMap);

            var trainIter = new MXDataIter("ImageRecordIter")
                .SetParam("path_imglist", "./data/train.lst")
                .SetParam("path_imgrec", "./data/train.rec")
                .SetParam("data_shape", new Shape(3, 224, 224))
                .SetParam("batch_size", batchSize)
                .SetParam("shuffle", 1)
                .CreateDataIter();

            var valIter = new MXDataIter("ImageRecordIter")
                .SetParam("path_imglist", "./data/val.lst")
                .SetParam("path_imgrec", "./data/val.rec")
                .SetParam("data_shape", new Shape(3, 224, 224))
                .SetParam("batch_size", batchSize)
                .CreateDataIter();

            var opt = OptimizerRegistry.Find("ccsgd");
            opt.SetParam("momentum", 0.9)
               .SetParam("rescale_grad", 1.0 / batchSize)
               .SetParam("clip_gradient", 10)
               .SetParam("lr", learningRate)
               .SetParam("wd", weightDecay);

            using (var exec = inceptionBnNet.SimpleBind(context, argsMap))
            {
                var argNames = inceptionBnNet.ListArguments();

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
                        // Update parameters
                        for (var i = 0; i < argNames.Count; ++i)
                        {
                            if (argNames[i] == "data" || argNames[i] == "data_label")
                                continue;

                            opt.Update(i, exec.ArgmentArrays[i], exec.GradientArrays[i]);
                        }

                        NDArray.WaitAll();
                    }

                    var accuracy = new Accuracy();
                    valIter.Reset();
                    while (valIter.Next())
                    {
                        var dataBatch = valIter.GetDataBatch();
                        dataBatch.Data.CopyTo(argsMap["data"]);
                        dataBatch.Label.CopyTo(argsMap["data_label"]);
                        NDArray.WaitAll();
                        exec.Forward(false);
                        NDArray.WaitAll();
                        accuracy.Update(dataBatch.Label, exec.Outputs[0]);
                    }

                    Logging.LG($"Accuracy: {accuracy.Get()}");
                }
            }

            MXNet.MXNotifyShutdown();
        }

        #region Helpers

        private static Symbol ConvFactoryBN(Symbol data,
                                            int numFilter,
                                            Shape kernel,
                                            Shape stride,
                                            Shape pad,
                                            string name,
                                            string suffix = "")
        {
            var conv_w = new Symbol($"conv_{name}{suffix}_w");
            var conv_b = new Symbol($"conv_{name}{suffix}_b");

            var conv = Operators.Convolution($"conv_{name}{suffix}", data,
                conv_w, conv_b, kernel,
                (uint)numFilter, stride, new Shape(1, 1), pad);
            var nameSuffix = name + suffix;
            var gamma = new Symbol(nameSuffix + "_gamma");
            var beta = new Symbol(nameSuffix + "_beta");
            var mmean = new Symbol(nameSuffix + "_mmean");
            var mvar = new Symbol(nameSuffix + "_mvar");
            var bn = Operators.BatchNorm("bn_" + name + suffix, conv, gamma, beta, mmean, mvar);
            return Operators.Activation("relu_" + name + suffix, bn, ActivationActType.Relu);
        }

        private static Symbol InceptionFactoryA(Symbol data,
                                                int num_1x1,
                                                int num_3x3red,
                                                int num_3x3,
                                                int num_d3x3red,
                                                int num_d3x3,
                                                PoolingPoolType pool,
                                                int proj,
                                                string name)
        {
            var c1x1 = ConvFactoryBN(data, num_1x1, new Shape(1, 1), new Shape(1, 1),
                                        new Shape(0, 0), name + "1x1");
            var c3x3r = ConvFactoryBN(data, num_3x3red, new Shape(1, 1), new Shape(1, 1),
                                         new Shape(0, 0), name + "_3x3r");
            var c3x3 = ConvFactoryBN(c3x3r, num_3x3, new Shape(3, 3), new Shape(1, 1),
                                        new Shape(1, 1), name + "_3x3");
            var cd3x3r = ConvFactoryBN(data, num_d3x3red, new Shape(1, 1), new Shape(1, 1),
                                          new Shape(0, 0), name + "_double_3x3", "_reduce");
            var cd3x3 = ConvFactoryBN(cd3x3r, num_d3x3, new Shape(3, 3), new Shape(1, 1),
                                         new Shape(1, 1), name + "_double_3x3_0");
            cd3x3 = ConvFactoryBN(cd3x3, num_d3x3, new Shape(3, 3), new Shape(1, 1),
                                  new Shape(1, 1), name + "_double_3x3_1");
            var pooling = Operators.Pooling(name + "_pool", data,
                                     new Shape(3, 3), pool, false, false,
                                     PoolingPoolingConvention.Valid,
                                     new Shape(1, 1), new Shape(1, 1));
            var cproj = ConvFactoryBN(pooling, proj, new Shape(1, 1), new Shape(1, 1),
                                         new Shape(0, 0), name + "_proj");

            var lst = new List<Symbol>();
            lst.Add(c1x1);
            lst.Add(c3x3);
            lst.Add(cd3x3);
            lst.Add(cproj);
            return Operators.Concat($"ch_concat_{name}_chconcat", lst, lst.Count);
        }


        private static Symbol InceptionFactoryB(Symbol data,
                                                int num_3x3red,
                                                int num_3x3,
                                                int num_d3x3red,
                                                int num_d3x3,
                                                string name)
        {
            var c3x3r = ConvFactoryBN(data, num_3x3red, new Shape(1, 1),
                                         new Shape(1, 1), new Shape(0, 0),
                                         name + "_3x3", "_reduce");
            var c3x3 = ConvFactoryBN(c3x3r, num_3x3, new Shape(3, 3), new Shape(2, 2),
                                        new Shape(1, 1), name + "_3x3");
            var cd3x3r = ConvFactoryBN(data, num_d3x3red, new Shape(1, 1), new Shape(1, 1),
                                          new Shape(0, 0), name + "_double_3x3", "_reduce");
            var cd3x3 = ConvFactoryBN(cd3x3r, num_d3x3, new Shape(3, 3), new Shape(1, 1), new Shape(1, 1), name + "_double_3x3_0");
            cd3x3 = ConvFactoryBN(cd3x3, num_d3x3, new Shape(3, 3), new Shape(2, 2), new Shape(1, 1), name + "_double_3x3_1");
            var pooling = Operators.Pooling("max_pool_" + name + "_pool", data,
                                     new Shape(3, 3), PoolingPoolType.Max,
                                     false, false, PoolingPoolingConvention.Valid, new Shape(2, 2), new Shape(1, 1));

            var lst = new List<Symbol>();
            lst.Add(c3x3);
            lst.Add(cd3x3);
            lst.Add(pooling);
            return Operators.Concat("ch_concat_" + name + "_chconcat", lst, lst.Count);
        }

        private static Symbol InceptionSymbol(int numClasses)
        {
            // data and label
            var data = Symbol.Variable("data");
            var data_label = Symbol.Variable("data_label");

            // stage 1
            var conv1 = ConvFactoryBN(data, 64, new Shape(7, 7), new Shape(2, 2), new Shape(3, 3), "conv1");
            var pool1 = Operators.Pooling("pool1", conv1, new Shape(3, 3), PoolingPoolType.Max,
                false, false, PoolingPoolingConvention.Valid, new Shape(2, 2));

            // stage 2
            var conv2red = ConvFactoryBN(pool1, 64, new Shape(1, 1), new Shape(1, 1), new Shape(0, 0), "conv2red");
            var conv2 = ConvFactoryBN(conv2red, 192, new Shape(3, 3), new Shape(1, 1), new Shape(1, 1), "conv2");
            var pool2 = Operators.Pooling("pool2", conv2, new Shape(3, 3), PoolingPoolType.Max,
                false, false, PoolingPoolingConvention.Valid, new Shape(2, 2));

            // stage 3
            var in3a = InceptionFactoryA(pool2, 64, 64, 64, 64, 96, PoolingPoolType.Avg, 32, "3a");
            var in3b = InceptionFactoryA(in3a, 64, 64, 96, 64, 96, PoolingPoolType.Avg, 64, "3b");
            var in3c = InceptionFactoryB(in3b, 128, 160, 64, 96, "3c");

            // stage 4
            var in4a = InceptionFactoryA(in3c, 224, 64, 96, 96, 128, PoolingPoolType.Avg, 128, "4a");
            var in4b = InceptionFactoryA(in4a, 192, 96, 128, 96, 128, PoolingPoolType.Avg, 128, "4b");
            var in4c = InceptionFactoryA(in4b, 160, 128, 160, 128, 160, PoolingPoolType.Avg, 128, "4c");
            var in4d = InceptionFactoryA(in4c, 96, 128, 192, 160, 192, PoolingPoolType.Avg, 128, "4d");
            var in4e = InceptionFactoryB(in4d, 128, 192, 192, 256, "4e");

            // stage 5
            var in5a = InceptionFactoryA(in4e, 352, 192, 320, 160, 224, PoolingPoolType.Avg, 128, "5a");
            var in5b = InceptionFactoryA(in5a, 352, 192, 320, 192, 224, PoolingPoolType.Max, 128, "5b");

            // average pooling
            Symbol avg = Operators.Pooling("global_pool", in5b, new Shape(7, 7), PoolingPoolType.Avg);

            // classifier
            var flatten = Operators.Flatten("flatten", avg);
            var conv1_w = new Symbol("conv1_w");
            var conv1_b = new Symbol("conv1_b");
            Symbol fc1 = Operators.FullyConnected("fc1", flatten, conv1_w, conv1_b, numClasses);
            return Operators.SoftmaxOutput("softmax", fc1, data_label);
        }


        #endregion

        #endregion

    }

}

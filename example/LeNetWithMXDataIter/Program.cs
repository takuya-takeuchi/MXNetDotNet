/*
 * This sample program is ported by C# from incubator-mxnet/blob/master/cpp-package/example/lenet_with_mxdataiter.cpp.
*/

using System.Collections.Generic;
using System.Diagnostics;
using MXNetDotNet;

namespace LeNetWithMXDataIter
{

    internal class Program
    {

        #region Methods

        private static void Main()
        {
            /*setup basic configs*/
            const int W = 28;
            const int H = 28;
            const int batchSize = 128;
            const int maxEpoch = 100;
            const float learningRate = 1e-4f;
            const float weightDecay = 1e-4f;

            var contest = Context.Gpu();

            var lenet = LenetSymbol();
            var argsMap = new SortedDictionary<string, NDArray>();

            argsMap["data"] = new NDArray(new Shape(batchSize, 1, W, H), contest);
            argsMap["data_label"] = new NDArray(new Shape(batchSize), contest);
            lenet.InferArgsMap(contest, argsMap, argsMap);

            argsMap["fc1_w"] = new NDArray(new Shape(500, 4 * 4 * 50), contest);
            NDArray.SampleGaussian(0, 1, argsMap["fc1_w"]);
            argsMap["fc2_b"] = new NDArray(new Shape(10), contest);
            argsMap["fc2_b"].Set(0);

            var trainIter = new MXDataIter("MNISTIter")
               .SetParam("image", "./mnist_data/train-images-idx3-ubyte")
               .SetParam("label", "./mnist_data/train-labels-idx1-ubyte")
               .SetParam("batch_size", batchSize)
               .SetParam("shuffle", 1)
               .SetParam("flat", 0)
               .CreateDataIter();
            var valIter = new MXDataIter("MNISTIter")
               .SetParam("image", "./mnist_data/t10k-images-idx3-ubyte")
               .SetParam("label", "./mnist_data/t10k-labels-idx1-ubyte")
               .CreateDataIter();

            var opt = OptimizerRegistry.Find("ccsgd");
            opt.SetParam("momentum", 0.9)
               .SetParam("rescale_grad", 1.0)
               .SetParam("clip_gradient", 10)
               .SetParam("lr", learningRate)
               .SetParam("wd", weightDecay);

            using (var exec = lenet.SimpleBind(contest, argsMap))
            {
                var argNames = lenet.ListArguments();

                // Create metrics
                var trainAccuracy = new Accuracy();
                var valAccuracy = new Accuracy();

                var sw = new Stopwatch();
                for (var iter = 0; iter < maxEpoch; ++iter)
                {
                    var samples = 0;
                    trainIter.Reset();
                    trainAccuracy.Reset();

                    sw.Restart();

                    while (trainIter.Next())
                    {
                        samples += batchSize;
                        var dataBatch = trainIter.GetDataBatch();

                        dataBatch.Data.CopyTo(argsMap["data"]);
                        dataBatch.Label.CopyTo(argsMap["data_label"]);
                        NDArray.WaitAll();

                        // Compute gradients
                        exec.Forward(true);
                        exec.Backward();

                        // Update parameters
                        for (var i = 0; i < argNames.Count; ++i)
                        {
                            if (argNames[i] == "data" || argNames[i] == "data_label")
                                continue;
                            opt.Update(i, exec.ArgmentArrays[i], exec.GradientArrays[i]);
                        }

                        // Update metric
                        trainAccuracy.Update(dataBatch.Label, exec.Outputs[0]);
                    }

                    // one epoch of training is finished
                    sw.Stop();
                    var duration = sw.ElapsedMilliseconds / 1000.0;
                    Logging.LG($"Epoch[{iter}] {samples / duration} samples/sec Train-Accuracy={trainAccuracy.Get()}");

                    valIter.Reset();
                    valAccuracy.Reset();

                    var accuracy = new Accuracy();
                    valIter.Reset();
                    while (valIter.Next())
                    {
                        var dataBatch = valIter.GetDataBatch();
                        dataBatch.Data.CopyTo(argsMap["data"]);
                        dataBatch.Label.CopyTo(argsMap["data_label"]);
                        NDArray.WaitAll();

                        // Only forward pass is enough as no gradient is needed when evaluating
                        exec.Forward(false);
                        NDArray.WaitAll();
                        accuracy.Update(dataBatch.Label, exec.Outputs[0]);
                        valAccuracy.Update(dataBatch.Label, exec.Outputs[0]);
                    }

                    Logging.LG($"Epoch[{iter}] Val-Accuracy={valAccuracy.Get()}");
                }
            }

            MXNet.MXNotifyShutdown();
        }

        #region Helpers

        private static Symbol LenetSymbol()
        {
            /*
             * LeCun, Yann, Leon Bottou, Yoshua Bengio, and Patrick Haffner.
             * "Gradient-based learning applied to document recognition."
             * Proceedings of the IEEE (1998)
             * */

            /*define the symbolic net*/
            Symbol data = Symbol.Variable("data");
            Symbol data_label = Symbol.Variable("data_label");
            Symbol conv1_w = new Symbol("conv1_w"), conv1_b = new Symbol("conv1_b");
            Symbol conv2_w = new Symbol("conv2_w"), conv2_b = new Symbol("conv2_b");
            Symbol conv3_w = new Symbol("conv3_w"), conv3_b = new Symbol("conv3_b");
            Symbol fc1_w = new Symbol("fc1_w"), fc1_b = new Symbol("fc1_b");
            Symbol fc2_w = new Symbol("fc2_w"), fc2_b = new Symbol("fc2_b");

            Symbol conv1 = Operators.Convolution("conv1", data, conv1_w, conv1_b, new Shape(5, 5), 20);
            Symbol tanh1 = Operators.Activation("tanh1", conv1, ActivationActType.Tanh);
            Symbol pool1 = Operators.Pooling("pool1", tanh1, new Shape(2, 2), PoolingPoolType.Max,
                false, false, PoolingPoolingConvention.Valid, new Shape(2, 2));

            Symbol conv2 = Operators.Convolution("conv2", pool1, conv2_w, conv2_b, new Shape(5, 5), 50);
            Symbol tanh2 = Operators.Activation("tanh2", conv2, ActivationActType.Tanh);
            Symbol pool2 = Operators.Pooling("pool2", tanh2, new Shape(2, 2), PoolingPoolType.Max,
                false, false, PoolingPoolingConvention.Valid, new Shape(2, 2));

            Symbol flatten = Operators.Flatten("flatten", pool2);
            Symbol fc1 = Operators.FullyConnected("fc1", flatten, fc1_w, fc1_b, 500);
            Symbol tanh3 = Operators.Activation("tanh3", fc1, ActivationActType.Tanh);
            Symbol fc2 = Operators.FullyConnected("fc2", tanh3, fc2_w, fc2_b, 10);

            Symbol lenet = Operators.SoftmaxOutput("softmax", fc2, data_label);

            return lenet;
        }

        #endregion

        #endregion

    }

}
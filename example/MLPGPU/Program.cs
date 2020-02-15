/*
 * This sample program is ported by C# from incubator-mxnet/blob/master/cpp-package/example/mlp_gpu.cpp.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Examples;
using MXNetDotNet;

namespace MLPGPU
{

    internal class Program
    {

        #region Methods

        private static int Main()
        {
            const int imageSize = 28;
            int[] layers = { 128, 64, 10 };
            const int batchSize = 100;
            const int maxEpoch = 10;
            const float learningRate = 0.1f;
            const float weightDecay = 1e-2f;

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

                    try
                    {
                        var net = Mlp(layers);

                        Context ctx = Context.Gpu();  // Use GPU for training

                        var args = new SortedDictionary<string, NDArray>();
                        args["X"] = new NDArray(new Shape(batchSize, imageSize * imageSize), ctx);
                        args["label"] = new NDArray(new Shape(batchSize), ctx);
                        // Let MXNet infer shapes of other parameters such as weights
                        net.InferArgsMap(ctx, args, args);

                        // Initialize all parameters with uniform distribution U(-0.01, 0.01)
                        var initializer = new Uniform(0.01f);
                        foreach (var arg in args)
                        {
                            // arg.first is parameter name, and arg.second is the value
                            initializer.Function(arg.Key, arg.Value);
                        }

                        // Create sgd optimizer
                        using (var opt = OptimizerRegistry.Find("sgd"))
                        {

                            opt.SetParam("rescale_grad", 1.0 / batchSize)
                               .SetParam("lr", learningRate)
                               .SetParam("wd", weightDecay);
                            var lrSch = new UniquePtr<LRScheduler>(new FactorScheduler(5000, 0.1f));
                            opt.SetLearningRateScheduler(lrSch);

                            // Create executor by binding parameters to the model
                            using (var exec = net.SimpleBind(ctx, args))
                            {
                                var argNames = net.ListArguments();

                                // Create metrics
                                var trainAcc = new Accuracy();
                                var valAcc = new Accuracy();

                                // Start training
                                var sw = new Stopwatch();
                                for (var iter = 0; iter < maxEpoch; ++iter)
                                {
                                    var samples = 0;
                                    trainIter.Reset();
                                    trainAcc.Reset();

                                    sw.Restart();

                                    while (trainIter.Next())
                                    {
                                        samples += batchSize;
                                        var dataBatch = trainIter.GetDataBatch();
                                        // Data provided by DataIter are stored in memory, should be copied to GPU first.
                                        dataBatch.Data.CopyTo(args["X"]);
                                        dataBatch.Label.CopyTo(args["label"]);
                                        // CopyTo is imperative, need to wait for it to complete.
                                        NDArray.WaitAll();

                                        // Compute gradients
                                        exec.Forward(true);
                                        exec.Backward();

                                        // Update parameters
                                        for (var i = 0; i < argNames.Count; ++i)
                                        {
                                            if (argNames[i] == "X" || argNames[i] == "label")
                                                continue;

                                            opt.Update(i, exec.ArgmentArrays[i], exec.GradientArrays[i]);
                                        }
                                        // Update metric
                                        trainAcc.Update(dataBatch.Label, exec.Outputs[0]);
                                    }
                                    // one epoch of training is finished
                                    sw.Stop();

                                    var duration = sw.ElapsedMilliseconds / 1000.0;
                                    Logging.LG($"Epoch[{iter}] {samples / duration} samples/sec Train-Accuracy={trainAcc.Get()}");

                                    valIter.Reset();
                                    valAcc.Reset();
                                    while (valIter.Next())
                                    {
                                        var dataBatch = valIter.GetDataBatch();
                                        dataBatch.Data.CopyTo(args["X"]);
                                        dataBatch.Label.CopyTo(args["label"]);
                                        NDArray.WaitAll();

                                        // Only forward pass is enough as no gradient is needed when evaluating
                                        exec.Forward(false);
                                        valAcc.Update(dataBatch.Label, exec.Outputs[0]);
                                    }
                                    Logging.LG($"Epoch[{iter}] Val-Accuracy={valAcc.Get()}");
                                }
                            }

                            MXNet.MXNotifyShutdown();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    return 0;
                }
            }
        }

        #region Helpers

        private static Symbol Mlp(IList<int> layers)
        {
            var x = Symbol.Variable("X");
            var label = Symbol.Variable("label");

            var weights = new Symbol[layers.Count];
            var biases = new Symbol[layers.Count];
            var outputs = new Symbol[layers.Count];

            for (var i = 0; i < layers.Count; ++i)
            {
                weights[i] = Symbol.Variable("w" + i);
                biases[i] = Symbol.Variable("b" + i);
                Symbol fc = Operators.FullyConnected(
                    i == 0 ? x : outputs[i - 1],  // data
                    weights[i],
                    biases[i],
                    layers[i]);
                outputs[i] = i == layers.Count - 1 ? fc : Operators.Activation(fc, ActivationActType.Relu);
            }

            return Operators.SoftmaxOutput(outputs.Last(), label);
        }

        #endregion

        #endregion

    }

}

/*
 * This sample program is ported by C# from incubator-mxnet/blob/master/cpp-package/example/test_score.cpp.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using MXNetDotNet;

namespace TestScore
{

    internal class Program
    {

        #region Methods

        private static void Main(string[] args)
        {
            //var minScore = float.Parse(args[0], NumberStyles.Float, null);
            var minScore = 0.9f;

            const int imageSize = 28;
            var layers = new[] { 128, 64, 10 };
            const int batchSize = 100;
            const int maxEpoch = 10;
            const float learningRate = 0.1f;
            const float weightDecay = 1e-2f;

            var trainIter = new MXDataIter("MNISTIter")
                .SetParam("image", "./mnist_data/train-images-idx3-ubyte")
                .SetParam("label", "./mnist_data/train-labels-idx1-ubyte")
                .SetParam("batch_size", batchSize)
                .SetParam("flat", 1)
                .CreateDataIter();
            var valIter = new MXDataIter("MNISTIter")
                .SetParam("image", "./mnist_data/t10k-images-idx3-ubyte")
                .SetParam("label", "./mnist_data/t10k-labels-idx1-ubyte")
                .SetParam("batch_size", batchSize)
                .SetParam("flat", 1)
                .CreateDataIter();

            var net = Mlp(layers);

            var ctx = Context.Cpu();  // Use GPU for training

            var dictionary = new Dictionary<string, NDArray>();
            dictionary["X"] = new NDArray(new Shape(batchSize, imageSize * imageSize), ctx);
            dictionary["label"] = new NDArray(new Shape(batchSize), ctx);
            // Let MXNet infer shapes of other parameters such as weights
            net.InferArgsMap(ctx, dictionary, dictionary);

            // Initialize all parameters with uniform distribution U(-0.01, 0.01)
            var initializer = new Uniform(0.01f);
            foreach (var arg in dictionary)
            {
                // arg.first is parameter name, and arg.second is the value
                initializer.Operator(arg.Key, arg.Value);
            }

            // Create sgd optimizer
            var opt = OptimizerRegistry.Find("sgd");
            opt.SetParam("rescale_grad", 1.0 / batchSize)
               .SetParam("lr", learningRate)
               .SetParam("wd", weightDecay);
            var lrSch = new UniquePtr<LRScheduler>(new FactorScheduler(5000, 0.1f));
            opt.SetLearningRateScheduler(lrSch);

            // Create executor by binding parameters to the model
            using (var exec = net.SimpleBind(ctx, dictionary))
            {
                var argNames = net.ListArguments();

                float score = 0;
                // Start training

                var sw = new Stopwatch();
                for (var iter = 0; iter < maxEpoch; ++iter)
                {
                    var samples = 0;
                    trainIter.Reset();

                    sw.Restart();
                    while (trainIter.Next())
                    {
                        samples += batchSize;
                        var dataBatch = trainIter.GetDataBatch();
                        // Data provided by DataIter are stored in memory, should be copied to GPU first.
                        dataBatch.Data.CopyTo(dictionary["X"]);
                        dataBatch.Label.CopyTo(dictionary["label"]);
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

                            var weight = exec.ArgmentArrays[i];
                            var grad = exec.GradientArrays[i];
                            opt.Update(i, weight, grad);
                        }
                    }

                    sw.Stop();

                    var acc = new Accuracy();
                    valIter.Reset();
                    while (valIter.Next())
                    {
                        var dataBatch = valIter.GetDataBatch();
                        dataBatch.Data.CopyTo(dictionary["X"]);
                        dataBatch.Label.CopyTo(dictionary["label"]);
                        NDArray.WaitAll();
                        // Only forward pass is enough as no gradient is needed when evaluating
                        exec.Forward(false);
                        acc.Update(dataBatch.Label, exec.Outputs[0]);
                    }

                    var duration = sw.ElapsedMilliseconds / 1000.0;
                    var message = $"Epoch: {iter} {samples / duration} samples/sec Accuracy: {acc.Get()}";
                    Logging.LG(message);
                    score = acc.Get();
                }

                MXNet.MXNotifyShutdown();
                var ret = score >= minScore ? 0 : 1;
                Console.WriteLine($"{ret}");
            }
        }

        #region Helpers

        private static Symbol Mlp(int[] layers)
        {
            var x = Symbol.Variable("X");
            var label = Symbol.Variable("label");

            var weights = new Symbol[layers.Length];
            var biases = new Symbol[layers.Length];
            var outputs = new Symbol[layers.Length];

            for (var i = 0; i < layers.Length; ++i)
            {
                weights[i] = Symbol.Variable($"w{i}");
                biases[i] = Symbol.Variable($"b{i}");
                var fc = Operators.FullyConnected(i == 0 ? x : outputs[i - 1],  // data
                    weights[i],
                    biases[i],
                    layers[i]);

                outputs[i] = i == layers.Length - 1 ? fc : Operators.Activation(fc, ActivationActType.Relu);
            }

            return Operators.SoftmaxOutput(outputs.Last(), label);
        }

        #endregion

        #endregion

    }
}

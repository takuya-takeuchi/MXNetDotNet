/*
 * This sample program is ported by C# from incubator-mxnet/blob/master/cpp-package/example/mlp_cpu.cpp.
*/

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MXNetDotNet;

namespace MLPCPU
{

    internal class Program
    {

        #region Methods

        private static void Main()
        {
            const int imageSize = 28;
            int[] layers = { 128, 64, 10 };
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

            Context ctx = Context.Cpu();  // Use CPU for training

            var args = new SortedDictionary<string, NDArray>();
            args["X"] = new NDArray(new Shape(batchSize, imageSize * imageSize), ctx);
            args["label"] = new NDArray(new Shape(batchSize), ctx);
            // Let MXNet infer shapes other parameters such as weights
            net.InferArgsMap(ctx, args, args);

            // Initialize all parameters with uniform distribution U(-0.01, 0.01)
            var initializer = new Uniform(0.01f);
            foreach (var arg in args)
            {
                // arg.first is parameter name, and arg.second is the value
                initializer.Operator(arg.Key, arg.Value);
            }

            // Create sgd optimizer
            var opt = OptimizerRegistry.Find("sgd");
            opt.SetParam("rescale_grad", 1.0 / batchSize)
               .SetParam("lr", learningRate)
               .SetParam("wd", weightDecay);

            // Create executor by binding parameters to the model
            using (var exec = net.SimpleBind(ctx, args))
            {
                var argNames = net.ListArguments();

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
                        // Set data and label
                        dataBatch.Data.CopyTo(args["X"]);
                        dataBatch.Label.CopyTo(args["label"]);

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
                    }

                    sw.Stop();

                    var accuracy = new Accuracy();
                    valIter.Reset();
                    while (valIter.Next())
                    {
                        var dataBatch = valIter.GetDataBatch();
                        dataBatch.Data.CopyTo(args["X"]);
                        dataBatch.Label.CopyTo(args["label"]);
                        // Forward pass is enough as no gradient is needed when evaluating
                        exec.Forward(false);
                        accuracy.Update(dataBatch.Label, exec.Outputs[0]);
                    }

                    var duration = sw.ElapsedMilliseconds / 1000.0;
                    Logging.LG($"Epoch: {iter} {samples / duration} samples/sec Accuracy: {accuracy.Get()}");
                }
            }

            MXNet.MXNotifyShutdown();
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

/*
 * This sample program is ported by C# from incubator-mxnet/blob/master/cpp-package/example/mlp.cpp.
*/

using System;
using System.Collections.Generic;
using MXNetDotNet;

using mx_float = System.Single;

namespace MLP
{

    internal class Program
    {

        #region Methods

        private static void Main()
        {
            Mlp();
            MXNet.MXNotifyShutdown();
        }

        #region Helpers

        private static void Mlp()
        {
            var symX = Symbol.Variable("X");
            var symLabel = Symbol.Variable("label");

            const int nLayers = 2;
            var layerSizes = new List<int>(new[] { 512, 10 });
            var weights = new Symbol[nLayers];
            var biases = new Symbol[nLayers];
            var outputs = new Symbol[nLayers];

            for (var i = 0; i < nLayers; i++)
            {
                var istr = i.ToString();
                weights[i] = Symbol.Variable($"w{istr}");
                biases[i] = Symbol.Variable($"b{istr}");
                var fc = Operators.FullyConnected($"fc{istr}",
                  i == 0 ? symX : outputs[i - 1],
                  weights[i], biases[i], layerSizes[i]);
                outputs[i] = Operators.LeakyReLU($"act{istr}", fc);
            }
            var sym_out = Operators.SoftmaxOutput("softmax", outputs[nLayers - 1], symLabel);

            var ctx_dev = new Context(DeviceType.CPU, 0);

            var array_x = new NDArray(new Shape(128, 28), ctx_dev, false);
            var array_y = new NDArray(new Shape(128), ctx_dev, false);

            var aptr_x = new mx_float[128 * 28];
            var aptr_y = new mx_float[128];

            // we make the data by hand, in 10 classes, with some pattern
            for (var i = 0; i < 128; i++)
            {
                for (var j = 0; j < 28; j++)
                {
                    aptr_x[i * 28 + j] = i % 10 * 1.0f;
                }
                aptr_y[i] = i % 10;
            }
            array_x.SyncCopyFromCPU(aptr_x, 128 * 28);
            array_x.WaitToRead();
            array_y.SyncCopyFromCPU(aptr_y, 128);
            array_y.WaitToRead();

            // init the parameters
            var array_w_1 = new NDArray(new Shape(512, 28), ctx_dev, false);
            var array_b_1 = new NDArray(new Shape(512), ctx_dev, false);
            var array_w_2 = new NDArray(new Shape(10, 512), ctx_dev, false);
            var array_b_2 = new NDArray(new Shape(10), ctx_dev, false);

            // the parameters should be initialized in some kind of distribution,
            // so it learns fast
            // but here just give a const value by hand
            array_w_1.Set(0.5f);
            array_b_1.Set(0.0f);
            array_w_2.Set(0.5f);
            array_b_2.Set(0.0f);

            // the grads
            var array_w_1_g = new NDArray(new Shape(512, 28), ctx_dev, false);
            var array_b_1_g = new NDArray(new Shape(512), ctx_dev, false);
            var array_w_2_g = new NDArray(new Shape(10, 512), ctx_dev, false);
            var array_b_2_g = new NDArray(new Shape(10), ctx_dev, false);

            // Bind the symolic network with the ndarray
            // all the input args
            var inArgs = new List<NDArray>();
            inArgs.Add(array_x);
            inArgs.Add(array_w_1);
            inArgs.Add(array_b_1);
            inArgs.Add(array_w_2);
            inArgs.Add(array_b_2);
            inArgs.Add(array_y);
            // all the grads
            var argGradStore = new List<NDArray>();
            argGradStore.Add(new NDArray());  // we don't need the grad of the input
            argGradStore.Add(array_w_1_g);
            argGradStore.Add(array_b_1_g);
            argGradStore.Add(array_w_2_g);
            argGradStore.Add(array_b_2_g);
            argGradStore.Add(
                new NDArray());  // neither do we need the grad of the loss
                                 // how to handle the grad
            var gradReqType = new List<OpReqType>();
            gradReqType.Add(OpReqType.NullOp);
            gradReqType.Add(OpReqType.WriteTo);
            gradReqType.Add(OpReqType.WriteTo);
            gradReqType.Add(OpReqType.WriteTo);
            gradReqType.Add(OpReqType.WriteTo);
            gradReqType.Add(OpReqType.NullOp);
            var auxStates = new List<NDArray>();

            Logging.LG("make the Executor");
            using (var exe = new Executor(sym_out, ctx_dev, inArgs, argGradStore, gradReqType, auxStates))
            {
                Logging.LG("Training");
                const int maxIters = 20000;
                const float learningRate = 0.0001f;
                for (var iter = 0; iter < maxIters; ++iter)
                {
                    exe.Forward(true);

                    if (iter % 100 == 0)
                    {
                        Logging.LG($"epoch {iter}");
                        var @out = exe.Outputs;
                        var cptr = new float[128 * 10];
                        @out[0].SyncCopyToCPU(cptr);
                        NDArray.WaitAll();
                        OutputAccuracy(cptr, aptr_y);
                    }

                    // update the parameters
                    exe.Backward();
                    for (var i = 1; i < 5; ++i)
                        using (var tmp = argGradStore[i] * learningRate)
                            inArgs[i].Subtract(tmp);
                    NDArray.WaitAll();
                }
            }
        }

        private static void OutputAccuracy(mx_float[] pred, mx_float[] target)
        {
            var right = 0;
            for (var i = 0; i < 128; ++i)
            {
                var mxP = pred[i * 10 + 0];
                var pY = 0f;
                for (var j = 0; j < 10; ++j)
                {
                    if (pred[i * 10 + j] > mxP)
                    {
                        mxP = pred[i * 10 + j];
                        pY = j;
                    }
                }

                if (Math.Abs(pY - target[i]) < float.Epsilon) right++;
            }

            Logging.LG($"Accuracy: {right / 128.0}");
        }

        #endregion

        #endregion

    }

}

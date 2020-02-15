/*
 * This sample program is ported by C# from incubator-mxnet/blob/master/cpp-package/example/alexnet.cpp.
*/

using System.Collections.Generic;
using System.Text;
using MXNetDotNet;

namespace AlexNet
{

    internal class Program
    {

        #region Methods

        private static void Main()
        {

            /*basic config*/
            const int batchSize = 256;
            const int maxEpo = 100;
            const float learningRate = 1e-4f;
            const float weightDecay = 1e-4f;

            /*context and net symbol*/
            var ctx = Context.Gpu();
            var net = AlexnetSymbol(2);

            /*args_map and aux_map is used for parameters' saving*/
            var argsMap = new Dictionary<string, NDArray>();
            var auxMap = new Dictionary<string, NDArray>();

            /*we should tell mxnet the shape of data and label*/
            argsMap["data"] = new NDArray(new Shape(batchSize, 3, 256, 256), ctx);
            argsMap["label"] = new NDArray(new Shape(batchSize), ctx);

            /*with data and label, executor can be generated varmatically*/
            using (var exec = net.SimpleBind(ctx, argsMap))
            {
                var argNames = net.ListArguments();
                var auxiliaryDictionary = exec.AuxiliaryDictionary();
                var argmentDictionary = exec.ArgmentDictionary();

                /*if fine tune from some pre-trained model, we should load the parameters*/
                // NDArray.Load("./model/alex_params_3", nullptr, &args_map);
                /*else, we should use initializer Xavier to init the params*/
                var xavier = new Xavier(RandType.Gaussian, FactorType.In, 2.34f);
                foreach (var arg in argmentDictionary)
                {
                    /*be careful here, the arg's name must has some specific ends or starts for
                     * initializer to call*/
                    xavier.Function(arg.Key, arg.Value);
                }

                /*print out to check the shape of the net*/
                foreach (var s in net.ListArguments())
                {
                    Logging.LG(s);

                    var sb = new StringBuilder();
                    var k = argmentDictionary[s].GetShape();
                    foreach (var i in k)
                        sb.Append($"{i} ");

                    Logging.LG(sb.ToString());
                }

                /*these binary files should be generated using im2rc tools, which can be found
                 * in mxnet/bin*/
                var trainIter = new MXDataIter("ImageRecordIter")
                                      .SetParam("path_imglist", "./data/train.lst")
                                      .SetParam("path_imgrec", "./data/train.rec")
                                      .SetParam("data_shape", new Shape(3, 256, 256))
                                      .SetParam("batch_size", batchSize)
                                      .SetParam("shuffle", 1)
                                      .CreateDataIter();
                var valIter = new MXDataIter("ImageRecordIter")
                                    .SetParam("path_imglist", "./data/val.lst")
                                    .SetParam("path_imgrec", "./data/val.rec")
                                    .SetParam("data_shape", new Shape(3, 256, 256))
                                    .SetParam("batch_size", batchSize)
                                    .CreateDataIter();

                var opt = OptimizerRegistry.Find("ccsgd");
                opt.SetParam("momentum", 0.9)
                   .SetParam("rescale_grad", 1.0 / batchSize)
                   .SetParam("clip_gradient", 10)
                   .SetParam("lr", learningRate)
                   .SetParam("wd", weightDecay);

                var accuracyTrain = new Accuracy();
                var accuracyVal = new Accuracy();
                var loglossVal = new LogLoss();
                for (var iter = 0; iter < maxEpo; ++iter)
                {
                    Logging.LG($"Train Epoch: {iter}");
                    /*reset the metric every epoch*/
                    accuracyTrain.Reset();
                    /*reset the data iter every epoch*/
                    trainIter.Reset();
                    while (trainIter.Next())
                    {
                        var batch = trainIter.GetDataBatch();
                        Logging.LG($"{trainIter.GetDataBatch().Index.Length}");
                        /*use copyto to feed new data and label to the executor*/
                        batch.Data.CopyTo(argmentDictionary["data"]);
                        batch.Label.CopyTo(argmentDictionary["label"]);
                        exec.Forward(true);
                        exec.Backward();
                        for (var i = 0; i < argNames.Count; ++i)
                        {
                            if (argNames[i] == "data" || argNames[i] == "label")
                                continue;
                            opt.Update(i, exec.ArgmentArrays[i], exec.GradientArrays[i]);
                        }

                        NDArray.WaitAll();
                        accuracyTrain.Update(batch.Label, exec.Outputs[0]);
                    }
                    Logging.LG($"ITER: {iter} Train Accuracy: {accuracyTrain.Get()}");

                    Logging.LG($"Val Epoch: {iter}");
                    accuracyVal.Reset();
                    valIter.Reset();
                    loglossVal.Reset();
                    while (valIter.Next())
                    {
                        var batch = valIter.GetDataBatch();
                        Logging.LG($"{valIter.GetDataBatch().Index.Length}");
                        batch.Data.CopyTo(argmentDictionary["data"]);
                        batch.Label.CopyTo(argmentDictionary["label"]);
                        exec.Forward(false);
                        NDArray.WaitAll();
                        accuracyVal.Update(batch.Label, exec.Outputs[0]);
                        loglossVal.Update(batch.Label, exec.Outputs[0]);
                    }
                    Logging.LG($"ITER: {iter} Val Accuracy: {accuracyVal.Get()}");
                    Logging.LG($"ITER: {iter} Val LogLoss: {loglossVal.Get()}");

                    /*save the parameters*/
                    var savePathParam = $"./model/alex_param_{iter}";
                    var saveArgs = argmentDictionary;
                    /*we do not want to save the data and label*/
                    if (saveArgs.ContainsKey("data"))
                        saveArgs.Remove("data");
                    if (saveArgs.ContainsKey("label"))
                        saveArgs.Remove("label");
                    /*the alexnet does not get any aux array, so we do not need to save
                     * aux_map*/
                    Logging.LG($"ITER: {iter} Saving to...{savePathParam}");
                    NDArray.Save(savePathParam, saveArgs);
                }
                /*don't foget to release the executor*/
            }

            MXNet.MXNotifyShutdown();
        }

        #region Helpers

        private static Symbol AlexnetSymbol(int numClasses)
        {
            var inputData = Symbol.Variable("data");
            var targetLabel = Symbol.Variable("label");

            /*stage 1*/
            var conv1 = new Operator("Convolution").SetParam("kernel", new Shape(11, 11))
                                                   .SetParam("num_filter", 96)
                                                   .SetParam("stride", new Shape(4, 4))
                                                   .SetParam("dilate", new Shape(1, 1))
                                                   .SetParam("pad", new Shape(0, 0))
                                                   .SetParam("num_group", 1)
                                                   .SetParam("workspace", 512)
                                                   .SetParam("no_bias", false)
                                                   .SetInput("data", inputData)
                                                   .CreateSymbol("conv1");
            var relu1 = new Operator("Activation").SetParam("act_type", "relu") /*relu,sigmoid,softrelu,tanh */
                                                  .SetInput("data", conv1)
                                                  .CreateSymbol("relu1");
            var pool1 = new Operator("Pooling").SetParam("kernel", new Shape(3, 3))
                                               .SetParam("pool_type", "max") /*avg,max,sum */
                                               .SetParam("global_pool", false)
                                               .SetParam("stride", new Shape(2, 2))
                                               .SetParam("pad", new Shape(0, 0))
                                               .SetInput("data", relu1)
                                               .CreateSymbol("pool1");
            var lrn1 = new Operator("LRN").SetParam("nsize", 5)
                                          .SetParam("alpha", 0.0001)
                                          .SetParam("beta", 0.75)
                                          .SetParam("knorm", 1)
                                          .SetInput("data", pool1)
                                          .CreateSymbol("lrn1");
            /*stage 2*/
            var conv2 = new Operator("Convolution").SetParam("kernel", new Shape(5, 5))
                                                   .SetParam("num_filter", 256)
                                                   .SetParam("stride", new Shape(1, 1))
                                                   .SetParam("dilate", new Shape(1, 1))
                                                   .SetParam("pad", new Shape(2, 2))
                                                   .SetParam("num_group", 1)
                                                   .SetParam("workspace", 512)
                                                   .SetParam("no_bias", false)
                                                   .SetInput("data", lrn1)
                                                   .CreateSymbol("conv2");
            var relu2 = new Operator("Activation").SetParam("act_type", "relu") /*relu,sigmoid,softrelu,tanh */
                                                  .SetInput("data", conv2)
                                                  .CreateSymbol("relu2");
            var pool2 = new Operator("Pooling").SetParam("kernel", new Shape(3, 3))
                                               .SetParam("pool_type", "max") /*avg,max,sum */
                                               .SetParam("global_pool", false)
                                               .SetParam("stride", new Shape(2, 2))
                                               .SetParam("pad", new Shape(0, 0))
                                               .SetInput("data", relu2)
                                               .CreateSymbol("pool2");
            var lrn2 = new Operator("LRN").SetParam("nsize", 5)
                                          .SetParam("alpha", 0.0001)
                                          .SetParam("beta", 0.75)
                                          .SetParam("knorm", 1)
                                          .SetInput("data", pool2)
                                          .CreateSymbol("lrn2");
            /*stage 3*/
            var conv3 = new Operator("Convolution").SetParam("kernel", new Shape(3, 3))
                                                   .SetParam("num_filter", 384)
                                                   .SetParam("stride", new Shape(1, 1))
                                                   .SetParam("dilate", new Shape(1, 1))
                                                   .SetParam("pad", new Shape(1, 1))
                                                   .SetParam("num_group", 1)
                                                   .SetParam("workspace", 512)
                                                   .SetParam("no_bias", false)
                                                   .SetInput("data", lrn2)
                                                   .CreateSymbol("conv3");
            var relu3 = new Operator("Activation").SetParam("act_type", "relu") /*relu,sigmoid,softrelu,tanh */
                                                  .SetInput("data", conv3)
                                                  .CreateSymbol("relu3");
            var conv4 = new Operator("Convolution").SetParam("kernel", new Shape(3, 3))
                                                   .SetParam("num_filter", 384)
                                                   .SetParam("stride", new Shape(1, 1))
                                                   .SetParam("dilate", new Shape(1, 1))
                                                   .SetParam("pad", new Shape(1, 1))
                                                   .SetParam("num_group", 1)
                                                   .SetParam("workspace", 512)
                                                   .SetParam("no_bias", false)
                                                   .SetInput("data", relu3)
                                                   .CreateSymbol("conv4");
            var relu4 = new Operator("Activation").SetParam("act_type", "relu") /*relu,sigmoid,softrelu,tanh */
                                                  .SetInput("data", conv4)
                                                  .CreateSymbol("relu4");
            var conv5 = new Operator("Convolution").SetParam("kernel", new Shape(3, 3))
                                                   .SetParam("num_filter", 256)
                                                   .SetParam("stride", new Shape(1, 1))
                                                   .SetParam("dilate", new Shape(1, 1))
                                                   .SetParam("pad", new Shape(1, 1))
                                                   .SetParam("num_group", 1)
                                                   .SetParam("workspace", 512)
                                                   .SetParam("no_bias", false)
                                                   .SetInput("data", relu4)
                                                   .CreateSymbol("conv5");
            var relu5 = new Operator("Activation").SetParam("act_type", "relu")
                                                  .SetInput("data", conv5)
                                                  .CreateSymbol("relu5");
            var pool3 = new Operator("Pooling").SetParam("kernel", new Shape(3, 3))
                                               .SetParam("pool_type", "max")
                                               .SetParam("global_pool", false)
                                               .SetParam("stride", new Shape(2, 2))
                                               .SetParam("pad", new Shape(0, 0))
                                               .SetInput("data", relu5)
                                               .CreateSymbol("pool3");
            /*stage4*/
            var flatten = new Operator("Flatten").SetInput("data", pool3)
                                                 .CreateSymbol("flatten");
            var fc1 = new Operator("FullyConnected").SetParam("num_hidden", 4096)
                                                    .SetParam("no_bias", false)
                                                    .SetInput("data", flatten)
                                                    .CreateSymbol("fc1");
            var relu6 = new Operator("Activation").SetParam("act_type", "relu")
                                                  .SetInput("data", fc1)
                                                  .CreateSymbol("relu6");
            var dropout1 = new Operator("Dropout").SetParam("p", 0.5)
                                                  .SetInput("data", relu6)
                                                  .CreateSymbol("dropout1");
            /*stage5*/
            var fc2 = new Operator("FullyConnected").SetParam("num_hidden", 4096)
                                                    .SetParam("no_bias", false)
                                                    .SetInput("data", dropout1)
                                                    .CreateSymbol("fc2");
            var relu7 = new Operator("Activation").SetParam("act_type", "relu")
                                                  .SetInput("data", fc2)
                                                  .CreateSymbol("relu7");
            var dropout2 = new Operator("Dropout").SetParam("p", 0.5)
                                                  .SetInput("data", relu7)
                                                  .CreateSymbol("dropout2");
            /*stage6*/
            var fc3 = new Operator("FullyConnected").SetParam("num_hidden", numClasses)
                                                    .SetParam("no_bias", false)
                                                    .SetInput("data", dropout2)
                                                    .CreateSymbol("fc3");
            var softmax = new Operator("SoftmaxOutput").SetParam("grad_scale", 1)
                                                       .SetParam("ignore_label", -1)
                                                       .SetParam("multi_output", false)
                                                       .SetParam("use_ignore", false)
                                                       .SetParam("normalization", "null") /*batch,null,valid */
                                                       .SetInput("data", fc3)
                                                       .SetInput("label", targetLabel)
                                                       .CreateSymbol("softmax");
            return softmax;
        }

        #endregion

        #endregion

    }

}

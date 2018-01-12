/*
 * This sample program is ported by C# from incubator-mxnet/example/image-classification/predict-cpp/image-classification-predict.cc.
*/

using System;
using CommandLine;

namespace YOLOv2
{

    internal class Program
    {

        #region Methods

        private static int Main(string[] args)
        {
            Parser.Default.ParseArguments<DetectOptions>(args)
                .MapResult(
                    opts =>
                    {
                        // create detector
                        var deviceType = 1;
                        var deviceId = 0;
                        if (opts.GpuId > -1)
                        {
                            deviceType = 2;
                            deviceId = opts.GpuId;
                        }

                        var detector = new Detector(opts.Model, opts.Epoch, opts.Width, opts.Height, opts.MeanR, opts.MeanG, opts.MeanB, deviceType, deviceId);

                        // detect image
                        var img = opts.InputImage;
                        var dets = detector.Detect(img);

                        if (dets.Length == 0)
                        {
                            Console.WriteLine("No detection found.");
                            return 0;
                        }

                        // load class names from text file if set
                        var classNames = new[]{
                            "aeroplane", "bicycle", "bird", "boat",
                            "bottle", "bus", "car", "cat", "chair",
                            "cow", "diningtable", "dog", "horse",
                            "motorbike", "person", "pottedplant",
                            "sheep", "sofa", "train", "tvmonitor"};

                        if (!string.IsNullOrWhiteSpace(opts.ClassMap))
                            classNames = detector.LoadClassMap(opts.ClassMap);

                        // save results
                        if (!string.IsNullOrWhiteSpace(opts.SaveResult))
                            detector.SaveDetectionResults(opts.SaveResult, dets, classNames);

                        // visualize detections
                        if (opts.DisplaySize > 0)
                            detector.VisualizeDetection(opts.InputImage, dets, opts.Threshold, opts.DisplaySize, classNames, opts.OutFile);

                        return 0;
                    },
                    errs =>
                    {
                        foreach (var error in errs)
                            Console.WriteLine($"{error.Tag}");
                        //Console.WriteLine($"Usage: {Assembly.GetAssembly(typeof(Program)).FullName} model_file weights_file list_file");
                        return 1;
                    });

            return 0;
        }

        #endregion

    }

}

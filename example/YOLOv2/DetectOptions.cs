/*
 * This sample program is ported by C# from caffe/tools/caffe.cpp.
*/

using CommandLine;

namespace YOLOv2
{

    [Verb("detect")]
    internal sealed class DetectOptions
    {

        [Option("input",
            Required = true,
            Default = "",
            HelpText = "input image")]
        public string InputImage { get; set; }

        [Option("out",
            Required = false,
            Default = "",
            HelpText = "output detection result to image")]
        public string OutFile { get; set; }

        [Option("model",
            Required = true,
            Default = "deploy_ssd_300",
            HelpText = "load model prefix")]
        public string Model { get; set; }

        [Option("epoch",
            Required = true,
            Default = 1,
            HelpText = "load model epoch")]
        public int Epoch { get; set; }

        [Option("classmap",
            Required = false,
            Default = "",
            HelpText = "load classes from text file")]
        public string ClassMap { get; set; }

        [Option("width",
            Required = false,
            Default = 300,
            HelpText = "resize width")]
        public int Width { get; set; }

        [Option("height",
            Required = false,
            Default = 300,
            HelpText = "resize height")]
        public int Height { get; set; }

        [Option("meanr",
            Required = false,
            Default = 123f,
            HelpText = "red mean pixel value")]
        public float MeanR { get; set; }

        [Option("meang",
            Required = false,
            Default = 117f,
            HelpText = "green mean pixel value")]
        public float MeanG { get; set; }

        [Option("meanb",
            Required = false,
            Default = 104f,
            HelpText = "blue mean pixel value")]
        public float MeanB { get; set; }

        [Option("threshold",
            Required = false,
            Default = 0.5f,
            HelpText = "visualize threshold")]
        public float Threshold { get; set; }

        [Option("gpu",
            Required = false,
            Default = -1,
            HelpText = "gpu id to detect with, default use cpu")]
        public int GpuId { get; set; }

        [Option("dispsize",
            Required = false,
            Default = 640,
            HelpText = "display size, -1 to disable display")]
        public int DisplaySize { get; set; }

        [Option("save",
            Required = false,
            Default = "",
            HelpText = "save result in text file")]
        public string SaveResult { get; set; }

    }

}
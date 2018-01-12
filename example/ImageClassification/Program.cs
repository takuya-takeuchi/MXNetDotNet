/*
 * This sample program is ported by C# from incubator-mxnet/example/image-classification/predict-cpp/image-classification-predict.cc.
*/

using System;
using System.IO;
using MXNetDotNet;
using OpenCvSharp;

namespace ImageClassification
{

    internal class Program
    {

        #region Fields

        const float DEFAULT_MEAN = 117.0f;

        #endregion

        #region Methods

        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No test image here.");
                Console.WriteLine("Usage: ./image-classification-predict apple.jpg");
                return 0;
            }

            var testFile = args[0];

            // Models path for your model, you have to modify it
            var jsonFile = "model/Inception/Inception-BN-symbol.json";
            var paramFile = "model/Inception/Inception-BN-0126.params";
            var synsetFile = "model/Inception/synset.txt";
            var ndFile = "model/Inception/mean_224.nd";

            if (!File.Exists(jsonFile))
            {
                Console.WriteLine($"{jsonFile} is not found.");
                return 0;
            }

            if (!File.Exists(paramFile))
            {
                Console.WriteLine($"{paramFile} is not found.");
                return 0;
            }

            if (!File.Exists(synsetFile))
            {
                Console.WriteLine($"{synsetFile} is not found.");
                return 0;
            }

            var jsonData = new BufferFile(jsonFile);
            var paramData = new BufferFile(paramFile);

            // Parameters
            var devType = 1;          // 1: cpu, 2: gpu
            const int devId = 0;      // arbitrary.
            var numInputNodes = 1u;   // 1 for feedforward
            const string inputKey = "data";
            string[] inputKeys = { inputKey };

            // Image size and channels
            const int width = 224;
            const int height = 224;
            const int channels = 3;

            var inputShapeIndptr = new[] { 0u, 4u };
            var inputShapeData = new[]{ (uint)1,
                                        (uint)channels,
                                        (uint)height,
                                        (uint)width};

            if (jsonData.GetLength() == 0 || paramData.GetLength() == 0)
            {
                return -1;
            }

            MXNet.MXPredCreate(File.ReadAllText(jsonFile),
                               paramData.GetBuffer(),
                               paramData.GetLength(),
                               devType,
                               devId,
                               numInputNodes,
                               inputKeys,
                               inputShapeIndptr,
                               inputShapeData,
                               out var predHnd);

            var imageSize = (uint)(width * height * channels);

            // Read Mean Data
            float[] ndData = null;
            NDListHandle ndHnd = null;
            var ndBuf = new BufferFile(ndFile);

            if (ndBuf.GetLength() > 0)
            {
                const uint nd_index = 0u;
                var ndLen = 0u;
                uint[] ndShape = null;
                string ndKey = null;
                var ndNdim = 0u;

                MXNet.MXNDListCreate(ndBuf.GetBuffer(), ndBuf.GetLength(), out ndHnd, out ndLen);

                MXNet.MXNDListGet(ndHnd, nd_index, out ndKey, out ndData, out ndShape, out ndNdim);
            }

            // Read Image Data
            var imageData = new float[imageSize];

            GetImageFile(testFile, imageData, channels, new Size(width, height), ndData);

            // Set Input Image
            MXNet.MXPredSetInput(predHnd, "data", imageData, imageSize);

            // Do Predict Forward
            MXNet.MXPredForward(predHnd);

            var outputIndex = 0u;

            // Get Output Result
            MXNet.MXPredGetOutputShape(predHnd, outputIndex, out var shape, out var shape_len);

            var size = 1u;
            for (var i = 0u; i < shape_len; ++i) size *= shape[i];

            var data = new float[size];

            MXNet.MXPredGetOutput(predHnd, outputIndex, data, size);

            // Release NDList
            ndHnd?.Dispose();

            // Release Predictor
            predHnd?.Dispose();


            // Synset path for your model, you have to modify it
            var synset = LoadSynset(synsetFile);

            // Print Output Data
            PrintOutputResult(data, synset);

            return 0;
        }

        #region Helpers

        private static void GetImageFile(string imageFile, float[] imageData, int channels, Size resize_size, float[] meanData)
        {
            // Read all kinds of file into a BGR color 3 channels image
            using (var imOri = Cv2.ImRead(imageFile))
            {
                if (imOri.Empty())
                {
                    Console.WriteLine($"Can't open the image. Please check {imageFile}.");
                    Environment.Exit(0);
                }

                using (var im = new Mat())
                {

                    Cv2.Resize(imOri, im, resize_size);

                    var size = im.Rows * im.Cols * channels;

                    unsafe
                    {
                        fixed (float* ptr = &imageData[0])
                        {
                            var ptrImageR = ptr;
                            var ptrImageG = ptr + size / 3;
                            var ptrImageB = ptr + size / 3 * 2;

                            float meanB, meanG, meanR;
                            meanB = meanG = meanR = DEFAULT_MEAN;

                            var index = 0;
                            for (var i = 0; i < im.Rows; i++)
                            {
                                var data = (byte*)im.Ptr(i);

                                for (var j = 0; j < im.Cols; j++)
                                {
                                    if (meanData != null)
                                    {
                                        meanR = meanData[index];
                                        if (channels > 1)
                                        {
                                            meanG = meanData[index + size / 3];
                                            meanB = meanData[index + size / 3 * 2];
                                        }

                                        index++;
                                    }

                                    if (channels > 1)
                                    {
                                        *ptrImageB = *data - meanB;
                                        data++;
                                        *ptrImageG = *data - meanG;
                                        data++;

                                        ptrImageB++;
                                        ptrImageG++;
                                    }

                                    *ptrImageR = *data - meanR;
                                    ptrImageR++;
                                    data++;
                                }
                            }
                        }
                    }
                }
            }
        }

        // LoadSynsets
        // Code from : https://github.com/pertusa/mxnet_predict_cc/blob/master/mxnet_predict.cc
        private static string[] LoadSynset(string synsetFile)
        {
            if (!File.Exists(synsetFile))
            {
                Console.WriteLine($"Error opening synset file {synsetFile}");
                Environment.Exit(0);
            }

            return File.ReadAllLines(synsetFile);
        }

        private static void PrintOutputResult(float[] data, string[] synset)
        {
            if (data.Length != synset.Length)
                Console.WriteLine("Result data and synset size does not match!");

            var bestAccuracy = 0.0f;
            var bestIdx = 0;
            for (var i = 0; i < data.Length; i++)
            {
                Console.WriteLine($"Accuracy[{i}] = {data[i]:F8}");

                if (data[i] > bestAccuracy)
                {
                    bestAccuracy = data[i];
                    bestIdx = i;
                }
            }

            Console.WriteLine($"Best Result: [{synset[bestIdx]}] id = {bestIdx}, accuracy ={bestAccuracy:F8}");
        }

        #endregion

        #endregion

        // Read file to buffer
        internal sealed class BufferFile
        {

            #region Fields

            readonly int length_;

            readonly byte[] buffer_;

            #endregion

            #region Constructors

            public BufferFile(string filePath)
            {
                if (!File.Exists(filePath))
                    return;

                this.buffer_ = File.ReadAllBytes(filePath);
                this.length_ = this.buffer_.Length;
            }

            #endregion

            #region Methods

            public int GetLength()
            {
                return length_;
            }

            public byte[] GetBuffer()
            {
                return buffer_;
            }

            #endregion

        }

    }

}

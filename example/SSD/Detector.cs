using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using MXNetDotNet;
using OpenCvSharp;

namespace SSD
{

    internal sealed class Detector
    {

        #region Fields

        private const int DetUnitSize = 6;

        private readonly int _Height;

        private readonly int _Width;

        private readonly string InputName;

        private readonly string[] InputKeys;

        private readonly float _MeanR;

        private readonly float _MeanG;

        private readonly float _MeanB;

        private readonly PredictorHandle _Predictor;

        #endregion

        #region Constructors

        public Detector(string modelPrefix, int epoch, int width, int height, float meanR, float meanG, float meanB, int deviceType, int deviceId)
        {
            if (epoch < 0 || epoch > 9999)
                throw new ArgumentOutOfRangeException($"Invalid epoch number: {epoch}");

            var modelFile = $"{modelPrefix}-{epoch:D4}.params";
            if (!File.Exists(modelFile))
                throw new FileNotFoundException($"{modelFile} is not found");

            var jsonFile = $"{modelPrefix}-symbol.json";
            if (!File.Exists(jsonFile))
                throw new FileNotFoundException($"{jsonFile} is not found");

            if (width < 1 || height < 1)
                throw new ArgumentOutOfRangeException($"Invalid width or height: {width}, {height}");

            this._Width = width;
            this._Height = height;
            this.InputName = "data";
            this.InputKeys = new string[1];
            this.InputKeys[0] = this.InputName;
            var inputShapeIndptr = new[] { 0u, 4u };
            var inputShapeData = new[] { 1u, 3u, (uint)this._Width, (uint)this._Height };
            this._MeanR = meanR;
            this._MeanG = meanG;
            this._MeanB = meanB;

            // load model
            var paramFileBuffer = File.ReadAllBytes(modelFile);
            var json = File.ReadAllText(jsonFile);

            if (paramFileBuffer.Length > 0)
            {
                MXNet.MXPredCreate(json, paramFileBuffer, paramFileBuffer.Length, deviceType, deviceId, 1, this.InputKeys, inputShapeIndptr, inputShapeData, out this._Predictor);
            }
            else
            {
                throw new ArgumentException($"Unable to read model file: {modelFile}");
            }
        }

        #endregion

        #region Methods

        public float[] Detect(string img)
        {
            if (!File.Exists(img))
                throw new FileNotFoundException($"{img} is not found.");

            using (var image = Cv2.ImRead(img))
            {
                if (image.Empty())
                    throw new ArgumentException($"Unable to load image file: {img}");

                if (image.Channels() != 3)
                    throw new ArgumentException($"RGB image required");

                using (var resized = image.Resize(new Size(this._Width, this._Height)))
                {
                    var size = resized.Channels() * resized.Rows * resized.Cols;
                    var inData = new float[size];

                    // de-interleave and minus means
                    unsafe
                    {
                        var ptr = (byte*)resized.Ptr();
                        fixed (float* dataPtr = &inData[0])
                        {
                            var tmp = dataPtr;
                            for (var i = 0; i < size; i += 3)
                            {
                                *tmp = ptr[i] - this._MeanR;
                                tmp++;
                            }

                            for (var i = 1; i < size; i += 3)
                            {
                                *tmp = ptr[i] - this._MeanG;
                                tmp++;
                            }

                            for (var i = 2; i < size; i += 3)
                            {
                                *tmp = ptr[i] - this._MeanB;
                                tmp++;
                            }
                        }
                    }

                    // usr model to forwad
                    MXNet.MXPredSetInput(this._Predictor, "data", inData, (uint)size);

                    var sw = new Stopwatch();
                    sw.Start();
                    MXNet.MXPredForward(this._Predictor);
                    MXNet.MXPredGetOutputShape(this._Predictor, 0, out var shape, out var shapeLen);

                    var ttSize = 1u;
                    for (var i = 0u; i < shapeLen; ++i) ttSize *= shape[i];

                    var outputs = new float[ttSize];
                    MXNet.MXPredGetOutput(this._Predictor, 0, outputs, ttSize);
                    sw.Stop();
                    Console.WriteLine($"Forward elapsed time: {sw.ElapsedMilliseconds} ms");

                    return outputs;
                }
            }
        }

        public string[] LoadClassMap(string mapFile)
        {
            if (!File.Exists(mapFile))
                throw new FileNotFoundException($"{mapFile} is not found.");

            return File.ReadAllLines(mapFile);
        }

        public void SaveDetectionResults(string filename, float[] dets, string[] classNames, float threshold = 0)
        {
            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var sw = new StreamWriter(fs, Encoding.UTF8))
            {
                for (var i = 0; i < dets.Length; i += DetUnitSize)
                {
                    if (dets[i] < 0)
                        continue;

                    var id = (int)dets[i];
                    var score = dets[i + 1];
                    if (score < threshold)
                        continue;
                    var xmin = dets[i + 2];
                    var ymin = dets[i + 3];
                    var xmax = dets[i + 4];
                    var ymax = dets[i + 5];

                    var strId = "";
                    if (0 < classNames.Length && id < classNames.Length)
                    {
                        strId = classNames[id];
                    }
                    else
                    {
                        strId = id.ToString();
                    }

                    var text = $"{strId}\t{score}\t{xmin}\t{ymin}\t{xmax}\t{ymax}";
                    sw.WriteLine(text);
                }
            }
        }

        public void VisualizeDetection(string imgPath, float[] detections, float visualizeThreshold, int maxDisplaySize, string[] classNames, string outFile)
        {
            using (var image = Cv2.ImRead(imgPath))
            {
                var bakImg = image;

                // resize for display
                if (maxDisplaySize > 0)
                {
                    var maxSize = (double)maxDisplaySize;
                    var ratio1 = maxSize / image.Rows;
                    var ratio2 = maxSize / image.Cols;
                    var ratio = ratio1 > ratio2 ? ratio2 : ratio1;

                    bakImg = bakImg.Resize(Size.Zero, ratio, ratio);
                }

                this.VisualizeDetections(bakImg, detections, classNames, visualizeThreshold);

                // save drawing if required
                if (!string.IsNullOrWhiteSpace(outFile))
                {
                    try
                    {
                        bakImg.SaveImage(outFile);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }

                // display
                Cv2.ImShow("detection", bakImg);

                Console.WriteLine("Please push any key to close window...");
                Cv2.WaitKey();
            }
        }

        #region Helpers

        private static void DrawBox(Mat img, int x0, int y0, int x1, int y1, Scalar color, int thickness = 1)
        {
            Cv2.Rectangle(img, Rect.FromLTRB(x0, y0, x1, y1), color, thickness);
        }

        private static void DrawText(Mat img, int x, int y, string text, Scalar bgColor, Scalar textColor, float opacity = 0.5f, int fontSize = 20)
        {
            var fontface = HersheyFonts.HersheySimplex;
            var scale = 0.4d;
            var thickness = 1;
            var size = Cv2.GetTextSize(text, fontface, scale, thickness, out var baseline);
            var or = new Point(x, y - baseline * 2.5);
            Cv2.Rectangle(img, or, or + new Point(size.Width, size.Height + baseline * 1), bgColor, -1);
            Cv2.PutText(img, text, new Point(x, y), fontface, scale, textColor);
        }

        private void VisualizeDetections(Mat img, float[] dets, string[] classNames, float visualizeThreshold)
        {
            if (dets.Length % DetUnitSize != 0)
                throw new ArgumentException($"{nameof(dets)} size is invalid");

            var width = img.Cols;
            var height = img.Rows;

            // generate random colors
            var numClasses = classNames.Length;
            if (numClasses < 1)
                numClasses = 1;

            // use constant seed
            var random = new Random(1);
            var colors = new List<uint[]>();
            for (var c = 0; c < numClasses; c++)
                colors.Add(new[]
                {
                    (uint) random.Next() % 255, (uint) random.Next() % 255, (uint) random.Next() % 255
                });

            for (var i = 0; i < dets.Length; i += DetUnitSize)
            {
                if (dets[i] < 0)
                    continue;

                var id = (int)dets[i];
                var score = dets[i + 1];
                if (score < visualizeThreshold)
                    continue;
                var xmin = (int)(dets[i + 2] * width);
                var ymin = (int)(dets[i + 3] * height);
                var xmax = (int)(dets[i + 4] * width);
                var ymax = (int)(dets[i + 5] * height);

                var text = score.ToString("F4");

                var color = colors[0];
                if (id < numClasses)
                {
                    color = colors[id];
                    text = $"{classNames[id]}: {text}";
                }

                var bgColor = new Scalar(color[0], color[1], color[2]);
                const int thickness = 3;
                DrawBox(img, xmin, ymin, xmax, ymax, bgColor, thickness);
                DrawText(img, xmin, ymin, text, bgColor, new Scalar(255, 255, 255));
            }
        }

        #endregion

        #endregion

    }

}
using System;
using mx_float = System.Single;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{
    
    public sealed class PSNR : EvalMetric
    {

        #region Fields

        private static readonly mx_float Log10;

        #endregion

        #region Constructors

        static PSNR()
        {
            Log10 = (mx_float)Math.Log(10.0f);
        }

        public PSNR() : base("psnr") { }

        #endregion

        #region Methods

        public override void Update(NDArray labels, NDArray preds)
        {
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));
            if (preds == null)
                throw new ArgumentNullException(nameof(preds));

            CheckLabelShapes(labels, preds);
            
            var predData = new mx_float[preds.Size];
            var labelData = new mx_float[labels.Size];
            preds.SyncCopyToCPU(predData);
            labels.SyncCopyToCPU(labelData);

            var len = (uint)preds.Size;
            mx_float sum = 0;
            for (var i = 0; i < len; ++i)
            {
                var diff = predData[i] - labelData[i];
                sum += diff * diff;
            }

            mx_float mse = sum / len;
            if (mse > 0)
            {
                this.SumMetric += (mx_float)(10 * Math.Log(255.0d / mse) / Log10);
            }
            else
            {
                this.SumMetric += 99.0f;
            }

            ++this.NumInst;
        }

        #endregion

    }

}

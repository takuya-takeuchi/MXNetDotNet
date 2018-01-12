using System;
using mx_float = System.Single;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed class RMSE : EvalMetric
    {

        #region Constructors

        public RMSE() : base("rmse") { }

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

            this.SumMetric += (float)Math.Sqrt(sum / len);
            ++this.NumInst;
        }

        #endregion

    }

}

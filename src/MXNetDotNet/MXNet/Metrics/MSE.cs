using System;
using mx_float = System.Single;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed class MSE : EvalMetric
    {

        #region Constructors

        public MSE() : base("mse") { }

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
                mx_float diff = predData[i] - labelData[i];
                sum += diff * diff;
            }

            this.SumMetric += sum / len;
            ++this.NumInst;
        }

        #endregion

    }

}

using System;
using mx_float = System.Single;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed class Accuracy : EvalMetric
    {

        #region Constructors

        public Accuracy() : base("accuracy") { }

        #endregion

        #region Methods

        public override void Update(NDArray labels, NDArray preds)
        {
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));
            if (preds == null)
                throw new ArgumentNullException(nameof(preds));

            Logging.CHECK_EQ(labels.GetShape().Count, 1);

            var len = labels.GetShape()[0];
            var predData = new mx_float[len];
            var labelData = new mx_float[len];
            
            preds.ArgmaxChannel().SyncCopyToCPU(predData);
            labels.SyncCopyToCPU(labelData);

            for (var i = 0; i < len; ++i)
            {
                this.SumMetric += Math.Abs(predData[i] - labelData[i]) < float.Epsilon ? 1 : 0;
                this.NumInst += 1;
            }
        }

        #endregion

    }

}
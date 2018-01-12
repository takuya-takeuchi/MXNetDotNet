using System;
using mx_float = System.Single;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed class LogLoss : EvalMetric
    {

        #region Fields

        private const float Epsilon = 1e-15f;

        #endregion

        #region Constructors

        public LogLoss() : base("logloss") { }

        #endregion

        #region Methods

        public override void Update(NDArray labels, NDArray preds)
        {
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));
            if (preds == null)
                throw new ArgumentNullException(nameof(preds));
            
            var len = labels.GetShape()[0];
            var m = preds.GetShape()[1];
            var predData = new mx_float[len * m];
            var labelData = new mx_float[len];

            preds.SyncCopyToCPU(predData);
            labels.SyncCopyToCPU(labelData);

            for (var i = 0; i < len; ++i)
            {
                // ToDo: (int)(i * m + labelData[i]) is no problem?
                this.SumMetric += (float)(-Math.Log(Math.Max(predData[(int)(i * m + labelData[i])], Epsilon)));
                this.NumInst += 1;
            }
        }

        #endregion

    }

}

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public class MSPReLU : Xavier
    {

        #region Constructors

        public MSPReLU(FactorType factorType = FactorType.Average, float slope = 0.25f)
            : base(RandType.Gaussian, factorType, (float)(2d / (1 + slope * slope)))
        {
        }

        #endregion

    }

}
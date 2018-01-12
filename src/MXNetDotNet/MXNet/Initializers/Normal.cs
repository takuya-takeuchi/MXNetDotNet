// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public class Normal : Initializer
    {

        #region Constructors

        public Normal(float mu, float sigma)
        {
            this.mu = mu;
            this.sigma = sigma;
        }

        #endregion

        #region Properties

        protected float mu
        {
            get;
        }

        protected float sigma
        {
            get;
        }

        #endregion

        #region Methods

        public override void Operator(string name, NDArray array)
        {
            NDArray.SampleGaussian(this.mu, this.sigma, array);
        }

        #endregion

    }

}
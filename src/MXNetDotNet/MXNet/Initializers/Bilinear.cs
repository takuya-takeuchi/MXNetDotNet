// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed class Bilinear : Initializer
    {

        #region Methods

        public override void Operator(string name, NDArray array)
        {
            base.InitBilinear(array);
        }

        #endregion

    }

}
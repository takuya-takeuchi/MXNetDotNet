// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed class Bilinear : Initializer
    {

        #region Methods

        public override void Function(string name, NDArray array)
        {
            base.InitBilinear(array);
        }

        #endregion

    }

}
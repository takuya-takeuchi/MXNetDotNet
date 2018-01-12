// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public class Constant : Initializer
    {

        #region Constructors

        public Constant(float value)
        {
            this.Value = value;
        }

        #endregion

        #region Properties

        protected float Value
        {
            get;
        }

        #endregion

        #region Methods

        public override void Operator(string name, NDArray array)
        {
            array.Set(this.Value);
        }

        #endregion

    }

}
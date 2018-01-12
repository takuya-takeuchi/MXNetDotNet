// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public class Uniform : Initializer
    {

        #region Constructors

        public Uniform(float scale)
            : this(-scale, scale)
        {

        }

        public Uniform(float begin, float end)
        {
            this.Begin = begin;
            this.End = end;
        }

        #endregion

        #region Properties

        protected float Begin
        {
            get;
        }

        protected float End
        {
            get;
        }

        #endregion

        #region Methods

        public override void Operator(string name, NDArray array)
        {
            NDArray.SampleUniform(this.Begin, this.End, array);
        }

        #endregion

    }

}
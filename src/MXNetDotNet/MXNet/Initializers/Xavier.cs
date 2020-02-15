using System;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public class Xavier : Initializer
    {

        #region Constructors

        public Xavier(RandType randType = RandType.Gaussian, FactorType factorType = FactorType.Average, float magnitude = 3)
        {
            this.Rand = randType;
            this.Factor = factorType;
            this.Magnitude = magnitude;
        }

        #endregion

        #region Properties

        public FactorType Factor
        {
            get;
        }

        public float Magnitude
        {
            get;
        }

        public RandType Rand
        {
            get;
        }

        #endregion

        #region Methods

        public override void Function(string name, NDArray array)
        {
            var shape = new Shape(array.GetShape());
            var hwScale = 1.0f;
            if (shape.Dimension > 2)
            {
                for (uint i = 2; i < shape.Dimension; ++i)
                    hwScale *= shape[i];
            }

            var @in = shape[1] * hwScale;
            var @out = shape[0] * hwScale;
            var factor = 1.0f;
            switch (this.Factor)
            {
                case FactorType.Average:
                    factor = (@in + @out) / 2.0f;
                    break;
                case FactorType.In:
                    factor = @in;
                    break;
                case FactorType.Out:
                    factor = @out;
                    break;
            }

            var scale = (float)Math.Sqrt(this.Magnitude / factor);
            switch (this.Rand)
            {
                case RandType.Uniform:
                    NDArray.SampleUniform(-scale, scale, array);
                    break;
                case RandType.Gaussian:
                    NDArray.SampleGaussian(0, scale, array);
                    break;
            }
        }

        #endregion

    }

}
using mx_float = System.Single;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Fields

        private static readonly string[] LeakyReLUActTypeValues =
        {
            "elu",
            "gelu",
            "leaky",
            "prelu",
            "rrelu",
            "selu"
        };

        #endregion

        #region Methods

        public static Symbol LeakyReLU(string symbolName,
                                       Symbol data,
                                       Symbol gamma,
                                       LeakyReLUActType actType = LeakyReLUActType.Leaky,
                                       mx_float slope = 0.25f,
                                       mx_float lowerBound = 0.125f,
                                       mx_float upperBound = 0.334f)
        {
            return new Operator("LeakyReLU").SetParam("act_type", LeakyReLUActTypeValues[(int)actType])
                                            .SetParam("slope", slope)
                                            .SetParam("lower_bound", lowerBound)
                                            .SetParam("upper_bound", upperBound)
                                            .SetInput("data", data)
                                            .SetInput("gamma", gamma)
                                            .CreateSymbol(symbolName);
        }

        public static Symbol LeakyReLU(Symbol data,
                                       Symbol gamma,
                                       LeakyReLUActType actType = LeakyReLUActType.Leaky,
                                       mx_float slope = 0.25f,
                                       mx_float lowerBound = 0.125f,
                                       mx_float upperBound = 0.334f)
        {
            return new Operator("LeakyReLU").SetParam("act_type", LeakyReLUActTypeValues[(int)actType])
                                            .SetParam("slope", slope)
                                            .SetParam("lower_bound", lowerBound)
                                            .SetParam("upper_bound", upperBound)
                                            .SetInput("data", data)
                                            .SetInput("gamma", gamma)
                                            .CreateSymbol();
        }

        #endregion

    }

}

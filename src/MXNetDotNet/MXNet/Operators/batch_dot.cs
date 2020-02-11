// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol batch_dot(string symbolName,
                                       Symbol lhs,
                                       Symbol rhs,
                                       bool transposeA = false,
                                       bool transposeB = false)
        {
            return new Operator("batch_dot").SetParam("transpose_a", transposeA)
                                            .SetParam("transpose_b", transposeB)
                                            .SetInput("lhs", lhs)
                                            .SetInput("rhs", rhs)
                                            .CreateSymbol(symbolName);
        }

        public static Symbol batch_dot(Symbol lhs,
                                       Symbol rhs,
                                       bool transposeA = false,
                                       bool transposeB = false)
        {
            return new Operator("batch_dot").SetParam("transpose_a", transposeA)
                                            .SetParam("transpose_b", transposeB)
                                            .SetInput("lhs", lhs)
                                            .SetInput("rhs", rhs)
                                            .CreateSymbol();
        }

        #endregion

    }

}

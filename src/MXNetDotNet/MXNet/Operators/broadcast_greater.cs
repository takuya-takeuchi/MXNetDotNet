// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol broadcast_greater(string symbolName,
                                               Symbol lhs,
                                               Symbol rhs)
        {
            return new Operator("broadcast_greater").SetInput("lhs", lhs)
                                                    .SetInput("rhs", rhs)
                                                    .CreateSymbol(symbolName);
        }

        public static Symbol broadcast_greater(Symbol lhs,
                                               Symbol rhs)
        {
            return new Operator("broadcast_greater").SetInput("lhs", lhs)
                                                    .SetInput("rhs", rhs)
                                                    .CreateSymbol();
        }

        #endregion

    }

}

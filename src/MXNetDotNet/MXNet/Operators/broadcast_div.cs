// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol broadcast_div(string symbolName,
                                           Symbol lhs,
                                           Symbol rhs)
        {
            return new Operator("broadcast_div").SetInput("lhs", lhs)
                                                .SetInput("rhs", rhs)
                                                .CreateSymbol(symbolName);
        }

        public static Symbol broadcast_div(Symbol lhs,
                                           Symbol rhs)
        {
            return new Operator("broadcast_div").SetInput("lhs", lhs)
                                                .SetInput("rhs", rhs)
                                                .CreateSymbol();
        }

        #endregion

    }

}

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol broadcast_add(string symbolName,
                                           Symbol lhs,
                                           Symbol rhs)
        {
            return new Operator("broadcast_add").SetInput("lhs", lhs)
                                                .SetInput("rhs", rhs)
                                                .CreateSymbol(symbolName);
        }

        public static Symbol broadcast_add(Symbol lhs,
                                           Symbol rhs)
        {
            return new Operator("broadcast_add").SetInput("lhs", lhs)
                                                .SetInput("rhs", rhs)
                                                .CreateSymbol();
        }

        #endregion

    }

}

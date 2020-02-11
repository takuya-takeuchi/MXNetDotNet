// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol broadcast_lesser_equal(string symbolName,
                                                    Symbol lhs,
                                                    Symbol rhs)
        {
            return new Operator("broadcast_lesser_equal").SetInput("lhs", lhs)
                                                         .SetInput("rhs", rhs)
                                                         .CreateSymbol(symbolName);
        }

        public static Symbol broadcast_lesser_equal(Symbol lhs,
                                                    Symbol rhs)
        {
            return new Operator("broadcast_lesser_equal").SetInput("lhs", lhs)
                                                         .SetInput("rhs", rhs)
                                                         .CreateSymbol();
        }

        #endregion

    }

}

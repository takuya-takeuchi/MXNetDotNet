// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol batch_take(string symbolName,
                                        Symbol a,
                                        Symbol indices)
        {
            return new Operator("batch_take")
                .SetInput("a", a)
                .SetInput("indices", indices)
                .CreateSymbol(symbolName);
        }

        public static Symbol batch_take(Symbol a, Symbol indices)
        {
            return new Operator("batch_take").SetInput("a", a)
                                             .SetInput("indices", indices)
                                             .CreateSymbol();
        }

        #endregion

    }

}

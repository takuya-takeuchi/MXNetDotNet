// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol arccosh(string symbolName, Symbol data)
        {
            return new Operator("arccosh").SetInput("data", data)
                                          .CreateSymbol(symbolName);
        }

        public static Symbol arccosh(Symbol data)
        {
            return new Operator("arccosh").SetInput("data", data)
                                          .CreateSymbol();
        }

        #endregion

    }

}

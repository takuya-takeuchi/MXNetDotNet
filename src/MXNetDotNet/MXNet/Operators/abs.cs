// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol abs(string symbolName, Symbol data)
        {
            return new Operator("abs").SetInput("data", data)
                                      .CreateSymbol(symbolName);
        }

        public static Symbol abs(Symbol data, Shape axes)
        {
            return new Operator("abs").SetInput("data", data)
                                      .CreateSymbol();
        }

        #endregion

    }

}

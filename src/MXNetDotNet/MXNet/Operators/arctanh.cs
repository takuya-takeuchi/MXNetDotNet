// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol arctanh(string symbolName, Symbol data)
        {
            return new Operator("arctanh").SetInput("data", data)
                                          .CreateSymbol(symbolName);
        }

        public static Symbol arctanh(Symbol data)
        {
            return new Operator("arctanh").SetInput("data", data)
                                          .CreateSymbol();
        }

        #endregion

    }

}

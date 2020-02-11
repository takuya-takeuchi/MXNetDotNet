// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol arcsinh(string symbolName, Symbol data)
        {
            return new Operator("arcsinh").SetInput("data", data)
                                          .CreateSymbol(symbolName);
        }

        public static Symbol arcsinh(Symbol data)
        {
            return new Operator("arcsinh").SetInput("data", data)
                                          .CreateSymbol();
        }

        #endregion

    }

}

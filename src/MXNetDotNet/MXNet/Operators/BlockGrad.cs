// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol BlockGrad(string symbolName, Symbol data)
        {
            return new Operator("BlockGrad").SetInput("data", data)
                                            .CreateSymbol(symbolName);
        }

        public static Symbol BlockGrad(Symbol data)
        {
            return new Operator("BlockGrad").SetInput("data", data)
                                            .CreateSymbol();
        }

        #endregion

    }

}

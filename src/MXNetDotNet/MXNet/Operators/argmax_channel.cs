// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol argmax_channel(string symbolName, Symbol data)
        {
            return new Operator("argmax_channel").SetInput("data", data)
                                                 .CreateSymbol(symbolName);
        }

        public static Symbol argmax_channel(Symbol data)
        {
            return new Operator("argmax_channel").SetInput("data", data)
                                                 .CreateSymbol();
        }

        #endregion

    }

}

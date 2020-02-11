// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol arccos(string symbolName, Symbol data)
        {
            return new Operator("arccos").SetInput("data", data)
                                         .CreateSymbol(symbolName);
        }

        public static Symbol arccos(Symbol data)
        {
            return new Operator("arccos").SetInput("data", data)
                                         .CreateSymbol();
        }

        #endregion

    }

}

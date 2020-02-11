// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol arctan(string symbolName, Symbol data)
        {
            return new Operator("arctan").SetInput("data", data)
                                         .CreateSymbol(symbolName);
        }

        public static Symbol arctan(Symbol data)
        {
            return new Operator("arctan").SetInput("data", data)
                                         .CreateSymbol();
        }

        #endregion

    }

}

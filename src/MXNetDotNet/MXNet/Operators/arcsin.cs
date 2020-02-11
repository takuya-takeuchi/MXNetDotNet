// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol arcsin(string symbolName, Symbol data)
        {
            return new Operator("arcsin").SetInput("data", data)
                                         .CreateSymbol(symbolName);
        }

        public static Symbol arcsin(Symbol data)
        {
            return new Operator("arcsin").SetInput("data", data)
                                         .CreateSymbol();
        }

        #endregion

    }

}

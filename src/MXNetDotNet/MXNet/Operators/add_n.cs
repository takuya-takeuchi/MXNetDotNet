using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol add_n(string symbolName, IEnumerable<Symbol> args)
        {
            return new Operator("add_n").Set(args)
                                        .CreateSymbol(symbolName);
        }

        public static Symbol add_n(IEnumerable<Symbol> args)
        {
            return new Operator("add_n").Set(args)
                                        .CreateSymbol();
        }

        #endregion

    }

}

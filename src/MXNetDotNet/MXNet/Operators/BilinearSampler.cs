// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol BilinearSampler(string symbolName,
                                             Symbol data,
                                             Symbol grid)
        {
            return new Operator("BilinearSampler").SetInput("data", data)
                                                  .SetInput("grid", grid)
                                                  .CreateSymbol(symbolName);
        }

        public static Symbol BilinearSampler(Symbol data, Symbol grid)
        {
            return new Operator("BilinearSampler").SetInput("data", data)
                                                  .SetInput("grid", grid)
                                                  .CreateSymbol();
        }

        #endregion

    }

}

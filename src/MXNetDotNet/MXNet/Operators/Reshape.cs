using uint32_t = System.UInt32;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol Reshape(string symbolName,
                                     Symbol data)
        {
            return Reshape(symbolName, data, new Shape());
        }

        public static Symbol Reshape(string symbolName,
                                     Symbol data,
                                     Shape shape)
        {
            return Reshape(symbolName, data, shape, new Shape());
        }

        public static Symbol Reshape(string symbolName,
                                     Symbol data,
                                     Shape shape,
                                     Shape targetShape,
                                     bool reverse = false,
                                     bool keepHighest = false)
        {
            return new Operator("Reshape").SetParam("shape", shape)
                                          .SetParam("reverse", reverse)
                                          .SetParam("target_shape", targetShape)
                                          .SetParam("keep_highest", keepHighest)
                                          .SetInput("data", data)
                                          .CreateSymbol(symbolName);
        }

        public static Symbol Reshape(Symbol data)
        {
            return Reshape(data, new Shape());
        }

        public static Symbol Reshape(Symbol data,
                                     Shape shape)
        {
            return Reshape(data, shape, new Shape());
        }

        public static Symbol Reshape(Symbol data,
                                     Shape shape,
                                     Shape targetShape,
                                     bool reverse = false,
                                     bool keepHighest = false)
        {
            return new Operator("Reshape").SetParam("shape", shape)
                                          .SetParam("reverse", reverse)
                                          .SetParam("target_shape", targetShape)
                                          .SetParam("keep_highest", keepHighest)
                                          .SetInput("data", data)
                                          .CreateSymbol();
        }

        #endregion

    }

}

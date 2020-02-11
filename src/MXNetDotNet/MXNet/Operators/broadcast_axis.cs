// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol broadcast_axis(string symbolName,
                                            Symbol data)
        {
            return broadcast_axis(symbolName, data, new Shape());
        }

        public static Symbol broadcast_axis(string symbolName,
                                            Symbol data,
                                            Shape axis)
        {
            return broadcast_axis(symbolName, data, new Shape(), new Shape());
        }

        public static Symbol broadcast_axis(string symbolName,
                                            Symbol data,
                                            Shape axis,
                                            Shape size)
        {
            return new Operator("broadcast_axis").SetParam("axis", axis)
                                                 .SetParam("size", size)
                                                 .SetInput("data", data)
                                                 .CreateSymbol(symbolName);
        }

        public static Symbol broadcast_axis(Symbol data)
        {
            return broadcast_axis(data, new Shape());
        }

        public static Symbol broadcast_axis(Symbol data,
                                            Shape axis)
        {
            return broadcast_axis(data, new Shape(), new Shape());
        }

        public static Symbol broadcast_axis(Symbol data,
                                            Shape axis,
                                            Shape size)
        {
            return new Operator("broadcast_axis").SetParam("axis", axis)
                                                 .SetParam("size", size)
                                                 .SetInput("data", data)
                                                 .CreateSymbol();
        }

        #endregion

    }

}

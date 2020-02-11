using mx_float = System.Single;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol BatchNorm_v1(string symbolName,
                                          Symbol data,
                                          Symbol gamma,
                                          Symbol beta,
                                          mx_float eps = 0.001f,
                                          mx_float momentum = 0.9f,
                                          bool fixGamma = true,
                                          bool useGlobalStats = false,
                                          bool outputMeanVar = false)
        {
            return new Operator("BatchNorm_v1").SetParam("eps", eps)
                                               .SetParam("momentum", momentum)
                                               .SetParam("fix_gamma", fixGamma)
                                               .SetParam("use_global_stats", useGlobalStats)
                                               .SetParam("output_mean_var", outputMeanVar)
                                               .SetInput("data", data)
                                               .SetInput("gamma", gamma)
                                               .SetInput("beta", beta)
                                               .CreateSymbol(symbolName);
        }

        public static Symbol BatchNorm_v1(Symbol data,
                                          Symbol gamma,
                                          Symbol beta,
                                          mx_float eps = 0.001f,
                                          mx_float momentum = 0.9f,
                                          bool fixGamma = true,
                                          bool useGlobalStats = false,
                                          bool outputMeanVar = false)
        {
            return new Operator("BatchNorm_v1").SetParam("eps", eps)
                                               .SetParam("momentum", momentum)
                                               .SetParam("fix_gamma", fixGamma)
                                               .SetParam("use_global_stats", useGlobalStats)
                                               .SetParam("output_mean_var", outputMeanVar)
                                               .SetInput("data", data)
                                               .SetInput("gamma", gamma)
                                               .SetInput("beta", beta)
                                               .CreateSymbol();
        }

        #endregion

    }

}

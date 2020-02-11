using mx_float=System.Single;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class Operators
    {

        #region Methods

        public static Symbol adam_update(string symbolName, 
                                         Symbol weight,
                                         Symbol grad,
                                         Symbol mean,
                                         Symbol var,
                                         mx_float lr,
                                         mx_float beta1 = 0.9f,
                                         mx_float beta2 = 0.999f,
                                         mx_float epsilon = 1e-08f,
                                         mx_float wd = 0,
                                         mx_float rescaleGrad = 1,
                                         mx_float clipGradient = -1)
        {
            return new Operator("adam_update").SetParam("lr", lr)
                                              .SetParam("beta1", beta1)
                                              .SetParam("beta2", beta2)
                                              .SetParam("epsilon", epsilon)
                                              .SetParam("wd", wd)
                                              .SetParam("rescale_grad", rescaleGrad)
                                              .SetParam("clip_gradient", clipGradient)
                                              .SetInput("weight", weight)
                                              .SetInput("grad", grad)
                                              .SetInput("mean", mean)
                                              .SetInput("var", var)
                                              .CreateSymbol(symbolName);
        }

        public static Symbol adam_update(Symbol weight,
                                         Symbol grad,
                                         Symbol mean,
                                         Symbol var,
                                         mx_float lr,
                                         mx_float beta1 = 0.9f,
                                         mx_float beta2 = 0.999f,
                                         mx_float epsilon = 1e-08f,
                                         mx_float wd = 0,
                                         mx_float rescaleGrad = 1,
                                         mx_float clipGradient = -1)
        {
            return new Operator("adam_update").SetParam("lr", lr)
                                              .SetParam("beta1", beta1)
                                              .SetParam("beta2", beta2)
                                              .SetParam("epsilon", epsilon)
                                              .SetParam("wd", wd)
                                              .SetParam("rescale_grad", rescaleGrad)
                                              .SetParam("clip_gradient", clipGradient)
                                              .SetInput("weight", weight)
                                              .SetInput("grad", grad)
                                              .SetInput("mean", mean)
                                              .SetInput("var", var)
                                              .CreateSymbol();
        }

        #endregion

    }

}

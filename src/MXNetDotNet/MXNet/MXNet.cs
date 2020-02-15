using MXNetDotNet.Interop;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed partial class MXNet
    {

        #region Methods

        public static void MXGetGPUCount(out int gpuCount)
        {
            Logging.CHECK_EQ(NativeMethods.MXGetGPUCount(out gpuCount), NativeMethods.OK);
        }

        public static void MXNotifyShutdown()
        {
            Logging.CHECK_EQ(NativeMethods.MXNotifyShutdown(), NativeMethods.OK);
        }

        #endregion

    }

}

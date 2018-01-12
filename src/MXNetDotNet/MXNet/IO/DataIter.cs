// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public abstract class DataIter : DisposableMXNetObject
    {

        #region Methods

        public abstract void BeforeFirst();

        public abstract NDArray GetData();

        public DataBatch GetDataBatch()
        {
            return new DataBatch
            {
                Data = this.GetData(),
                Label = this.GetLabel(),
                PadNum = this.GetPadNum(),
                Index = this.GetIndex()
            };
        }

        public abstract int[] GetIndex();

        public abstract NDArray GetLabel();

        public abstract int GetPadNum();

        public abstract bool Next();

        public void Reset()
        {
            this.BeforeFirst();
        }

        #endregion

    }

}
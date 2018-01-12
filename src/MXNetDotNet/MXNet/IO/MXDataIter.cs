using System;
using System.Collections.Generic;
using System.Linq;
using MXNetDotNet.Extensions;
using MXNetDotNet.Interop;
using mx_float = System.Single;
using DataIterCreator = System.IntPtr;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed class MXDataIter : DataIter
    {

        #region Fields

        private static readonly MXDataIterMap DataiterMap = new MXDataIterMap();

        private readonly DataIterCreator _Creator;

        private readonly Dictionary<string, string> _Params = new Dictionary<string, string>();

        private MXDataIterBlob _BlobPtr;

        #endregion

        #region Constructors

        public MXDataIter(string mxdataiterType)
        {
            this._Creator = DataiterMap.GetMXDataIterCreator(mxdataiterType);
            this._BlobPtr = new MXDataIterBlob();
        }

        public MXDataIter(MXDataIter other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            this._Creator = other._Creator;
            this._Params = new Dictionary<string, string>(other._Params);

            other._BlobPtr.AddRef();
            this._BlobPtr = other._BlobPtr;
        }

        #endregion

        #region Methods

        public MXDataIter CreateDataIter()
        {
            var keys = this._Params.Keys.ToArray();
            var param_keys = new string[keys.Length];
            var param_values = new string[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                param_keys[i] = key;
                param_values[i] = this._Params[key];
            }

            NativeMethods.MXDataIterCreateIter(this._Creator,
                                               (uint)param_keys.Length,
                                               param_keys,
                                               param_values,
                                               out var @out);
            this._BlobPtr.Handle = @out;

            return this;
        }

        public MXDataIter SetParam(string name, object value)
        {
            this._Params[name] = value.ToValueString();
            return this;
        }

        #region Overrides

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            this._BlobPtr?.ReleaseRef();
            this._BlobPtr = null;
        }

        #endregion

        #endregion

        #region DataIter Members

        public override void BeforeFirst()
        {
            var r = NativeMethods.MXDataIterBeforeFirst(this._BlobPtr.Handle);
            Logging.CHECK_EQ(r, 0);
        }

        public override NDArray GetData()
        {
            var r = NativeMethods.MXDataIterGetData(this._BlobPtr.Handle, out var handle);
            Logging.CHECK_EQ(r, 0);
            return new NDArray(handle);
        }

        public override int[] GetIndex()
        {
            var r = NativeMethods.MXDataIterGetIndex(this._BlobPtr.Handle, out var out_index, out var out_size);
            Logging.CHECK_EQ(r, 0);

            var outIndexArray = InteropHelper.ToUInt64Array(out_index, (uint)out_size);
            var ret = new int[out_size];
            for (var i = 0ul; i < out_size; ++i)
                ret[i] = (int)outIndexArray[i];

            return ret;
        }

        public override NDArray GetLabel()
        {
            var r = NativeMethods.MXDataIterGetLabel(this._BlobPtr.Handle, out var handle);
            Logging.CHECK_EQ(r, 0);
            return new NDArray(handle);
        }

        public override int GetPadNum()
        {
            var r = NativeMethods.MXDataIterGetPadNum(this._BlobPtr.Handle, out var @out);
            Logging.CHECK_EQ(r, 0);
            return @out;
        }

        public override bool Next()
        {
            int r = NativeMethods.MXDataIterNext(this._BlobPtr.Handle, out var @out);
            Logging.CHECK_EQ(r, 0);
            return @out > 0;
        }

        #endregion

    }

}

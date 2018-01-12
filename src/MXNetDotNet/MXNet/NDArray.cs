using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MXNetDotNet.Extensions;
using MXNetDotNet.Interop;
using NDArrayHandle = System.IntPtr;
using mx_uint = System.UInt32;
using mx_float = System.Single;
using size_t = System.UInt64;

// ReSharper disable once CheckNamespace
namespace MXNetDotNet
{

    public sealed class NDArray : DisposableMXNetObject
    {

        #region Fields

        private readonly NDBlob _Blob;

        #endregion

        #region Constructors

        public NDArray()
        {
            Logging.CHECK_EQ(NativeMethods.MXNDArrayCreateNone(out var @out), NativeMethods.OK);

            this.NativePtr = @out;
            this._Blob = new NDBlob(@out);
        }

        public NDArray(NDArrayHandle handle)
        {
            if (handle == NDArrayHandle.Zero)
                throw new ArgumentException("Can not pass IntPtr.Zero", nameof(handle));

            this.NativePtr = handle;
            this._Blob = new NDBlob(handle);
        }

        public NDArray(IList<mx_uint> shape, Context context, bool delayAlloc = true)
        {
            if (shape == null)
                throw new ArgumentNullException(nameof(shape));
            if (context == null)
                throw new ArgumentNullException(nameof(context));


            var floats = shape as mx_uint[];
            var arg = floats ?? shape.ToArray();

            Logging.CHECK_EQ(NativeMethods.MXNDArrayCreate(arg,
                                                           (uint)arg.Length,
                                                           (int)context.GetDeviceType(),
                                                           context.GetDeviceId(),
                                                           delayAlloc.ToInt32(),
                                                           out var @out), NativeMethods.OK);
            this.NativePtr = @out;
            this._Blob = new NDBlob(@out);
        }

        public NDArray(Shape shape, Context context, bool delayAlloc = true)
        {
            if (shape == null)
                throw new ArgumentNullException(nameof(shape));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Logging.CHECK_EQ(NativeMethods.MXNDArrayCreate(shape.Data,
                                                           shape.Dimension,
                                                           (int)context.GetDeviceType(),
                                                           context.GetDeviceId(),
                                                           delayAlloc.ToInt32(),
                                                           out var @out), NativeMethods.OK);
            this.NativePtr = @out;
            this._Blob = new NDBlob(@out);
        }

        public NDArray(mx_float[] data, Shape shape, Context context)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (shape == null)
                throw new ArgumentNullException(nameof(shape));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Logging.CHECK_EQ(NativeMethods.MXNDArrayCreate(shape.Data,
                                                           (uint)shape.Dimension,
                                                           (int)context.GetDeviceType(),
                                                           context.GetDeviceId(),
                                                           false.ToInt32(),
                                                           out var @out), NativeMethods.OK);

            NativeMethods.MXNDArraySyncCopyFromCPU(@out, data, shape.Size);

            this.NativePtr = @out;
            this._Blob = new NDBlob(@out);
        }

        public NDArray(IList<mx_float> data, Shape shape, Context context)
            : this(data?.ToArray(), shape, context)
        {
        }

        public NDArray(IList<mx_float> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Logging.CHECK_EQ(NativeMethods.MXNDArrayCreateNone(out var @out), NativeMethods.OK);

            NativeMethods.MXNDArraySyncCopyFromCPU(@out, data.ToArray(), (size_t)data.Count);

            this.NativePtr = @out;
            this._Blob = new NDBlob(@out);
        }

        #endregion

        #region Properties

        public size_t Size
        {
            get
            {
                size_t ret = 1;
                var shape = this.GetShape();
                for (var i = 0; i < shape.Count; i++)
                    ret *= shape[i];

                return ret;
            }
        }

        #endregion

        #region Methods

        public NDArray ArgmaxChannel()
        {
            var ret = new NDArray();
            using (var op = new Operator("argmax_channel"))
                op.Set(this).Invoke(ret);

            return ret;
        }

        public NDArray Copy(Context context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var ret = new NDArray(this.GetShape(), context);
            using (var op = new Operator("_copyto"))
                op.Set(this).Invoke(ret);

            return ret;
        }

        public NDArray CopyTo(NDArray other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            using (var op = new Operator("_copyto"))
                op.Set(this).Invoke(other);
            return other;
        }

        public Context GetContext()
        {
            NativeMethods.MXNDArrayGetContext(this._Blob.Handle, out var out_dev_type, out var out_dev_id);
            return new Context((DeviceType)out_dev_type, out_dev_id);
        }

        public NDArrayHandle GetData()
        {
            NativeMethods.MXNDArrayGetData(this._Blob.Handle, out var ret);
            if (this.GetDType() != 0)
                return IntPtr.Zero;

            return ret;
        }

        public int GetDType()
        {
            NativeMethods.MXNDArrayGetDType(this._Blob.Handle, out var ret);
            return ret;
        }

        public NDArrayHandle GetHandle()
        {
            this.ThrowIfDisposed();
            return this.NativePtr;
        }

        public IList<mx_uint> GetShape()
        {
            NativeMethods.MXNDArrayGetShape(this.NativePtr, out var outDim, out var outData);
            return InteropHelper.ToUInt32Array(outData, outDim);
        }

        public static IDictionary<string, NDArray> LoadToMap(string fileName)
        {
            var arrayMap = new SortedDictionary<string, NDArray>();
            Logging.CHECK_EQ(NativeMethods.MXNDArrayLoad(fileName, 
                                                         out var outSize,
                                                         out var outArr,
                                                         out var outNameSize,
                                                         out var outNames), NativeMethods.OK);
            if (outNameSize > 0)
            {
                var array = InteropHelper.ToPointerArray(outArr, outSize);
                var namearray = InteropHelper.ToPointerArray(outNames, outNameSize);

                Logging.CHECK_EQ(outNameSize, outSize);
                for (mx_uint i = 0; i < outSize; ++i)
                {
                    var name = Marshal.PtrToStringAnsi(namearray[i]);
                    arrayMap[name] = new NDArray(array[i]);
                }
            }

            return arrayMap;
        }

        public static void SampleGaussian(mx_float mu, mx_float sigma, NDArray @out)
        {
            using (var op = new Operator("_random_normal"))
                op.Set(mu, sigma).Invoke(@out);
        }

        public static void SampleUniform(mx_float begin, mx_float end, NDArray @out)
        {
            using (var op = new Operator("_random_uniform"))
                op.Set(begin, end).Invoke(@out);
        }

        public static void Save(string fileName, IDictionary<string, NDArray> arrayMap)
        {
            var tmp = arrayMap.Keys.ToArray();

            var args = new NDArrayHandle[tmp.Length];
            var keys = new string[tmp.Length];
            for (var i = 0; i < tmp.Length; i++)
            {
                var kv = arrayMap[keys[i]];
                args[i] = kv.GetHandle();
                keys[i] = keys[i];
            }

            Logging.CHECK_EQ(NativeMethods.MXNDArraySave(fileName, (uint)args.Length, args, keys), NativeMethods.OK);
        }

        public static void Save(string fileName, IList<NDArray> arrayList)
        {
            var args = arrayList.Select(array => array.GetHandle()).ToArray();
            Logging.CHECK_EQ(NativeMethods.MXNDArraySave(fileName, (uint)args.Length, args, null), NativeMethods.OK);
        }

        public void Set(mx_float scalar)
        {
            using (var op = new Operator("_set_value"))
                op.Set(scalar).Invoke(this);
        }

        public NDArray Slice(mx_uint begin, mx_uint end)
        {
            Logging.CHECK_EQ(NativeMethods.MXNDArraySlice(this.GetHandle(), begin, end, out var handle), NativeMethods.OK);
            return new NDArray(handle);
        }

        public void SyncCopyFromCPU(mx_float[] data, size_t size)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            NativeMethods.MXNDArraySyncCopyFromCPU(this._Blob.Handle, data, size);
        }

        public void SyncCopyFromCPU(mx_float[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            NativeMethods.MXNDArraySyncCopyFromCPU(this._Blob.Handle, data, (size_t)data.Length);
        }

        public void SyncCopyToCPU(mx_float[] data)
        {
            NativeMethods.MXNDArraySyncCopyToCPU(this._Blob.Handle, data, (uint)data.Length);
        }

        public void SyncCopyToCPU(List<mx_float> data, size_t size = 0)
        {
            var resize = size > 0;
            size = resize ? size : this.Size;

            var args = new mx_float[size];
            NativeMethods.MXNDArraySyncCopyToCPU(this._Blob.Handle, args, (uint)args.Length);

            data.Clear();
            data.Capacity = args.Length;
            data.AddRange(args);
        }

        public static void WaitAll()
        {
            Logging.CHECK_EQ(NativeMethods.MXNDArrayWaitAll(), NativeMethods.OK);
        }

        public void WaitToRead()
        {
            Logging.CHECK_EQ(NativeMethods.MXNDArrayWaitToRead(this._Blob.Handle), NativeMethods.OK);
        }

        public void WaitToWrite()
        {
            Logging.CHECK_EQ(NativeMethods.MXNDArrayWaitToWrite(this._Blob.Handle), NativeMethods.OK);
        }

        #region Operators

        public void Add(mx_float scalar)
        {
            using (var op = new Operator("_plus_scalar"))
                op.Set(this, scalar).Invoke(this);
        }

        public void Add(NDArray rhs)
        {
            using (var op = new Operator("_plus"))
                op.Set(this, rhs).Invoke(this);
        }

        public void Subtract(mx_float scalar)
        {
            using (var op = new Operator("_minus_scalar"))
                op.Set(this, scalar).Invoke(this);
        }

        public void Subtract(NDArray rhs)
        {
            using (var op = new Operator("_minus"))
                op.Set(this, rhs).Invoke(this);
        }

        public void Multiply(mx_float scalar)
        {
            using (var op = new Operator("_mul_scalar"))
                op.Set(this, scalar).Invoke(this);
        }

        public void Multiply(NDArray rhs)
        {
            using (var op = new Operator("_mul"))
                op.Set(this, rhs).Invoke(this);
        }

        public void Divide(mx_float scalar)
        {
            using (var op = new Operator("_div_scalar"))
                op.Set(this, scalar).Invoke(this);
        }

        public void Divide(NDArray rhs)
        {
            using (var op = new Operator("_div"))
                op.Set(this, rhs).Invoke(this);
        }

        public void Remainder(mx_float scalar)
        {
            using (var op = new Operator("_mod_scalar"))
                op.Set(this, scalar).Invoke(this);
        }

        public void Remainder(NDArray rhs)
        {
            using (var op = new Operator("_mod"))
                op.Set(this, rhs).Invoke(this);
        }

        #endregion

        #region Overrides

        #region Operators

        public static NDArray operator +(NDArray lhs, mx_float scalar)
        {
            var ret = new NDArray();
            using (var op = new Operator("_plus_scalar"))
                op.Set(lhs, scalar).Invoke(ret);
            return ret;
        }

        public static NDArray operator -(NDArray lhs, mx_float scalar)
        {
            var ret = new NDArray();
            using (var op = new Operator("_minus_scalar"))
                op.Set(lhs, scalar).Invoke(ret);
            return ret;
        }

        public static NDArray operator *(NDArray lhs, mx_float scalar)
        {
            var ret = new NDArray();
            using (var op = new Operator("_mul_scalar"))
                op.Set(lhs, scalar).Invoke(ret);
            return ret;
        }

        public static NDArray operator /(NDArray lhs, mx_float scalar)
        {
            var ret = new NDArray();
            using (var op = new Operator("_div_scalar"))
                op.Set(lhs, scalar).Invoke(ret);
            return ret;
        }

        public static NDArray operator %(NDArray lhs, mx_float scalar)
        {
            var ret = new NDArray();
            using (var op = new Operator("_mod_scalar"))
                op.Set(lhs, scalar).Invoke(ret);
            return ret;
        }

        public static NDArray operator +(NDArray lhs, NDArray rhs)
        {
            var ret = new NDArray();
            using (var op = new Operator("_plus"))
                op.Set(lhs, rhs).Invoke(ret);
            return ret;
        }

        public static NDArray operator -(NDArray lhs, NDArray rhs)
        {
            var ret = new NDArray();
            using (var op = new Operator("_minus"))
                op.Set(lhs, rhs).Invoke(ret);
            return ret;
        }

        public static NDArray operator *(NDArray lhs, NDArray rhs)
        {
            var ret = new NDArray();
            using (var op = new Operator("_mul"))
                op.Set(lhs, rhs).Invoke(ret);
            return ret;
        }

        public static NDArray operator /(NDArray lhs, NDArray rhs)
        {
            var ret = new NDArray();
            using (var op = new Operator("_div"))
                op.Set(lhs, rhs).Invoke(ret);
            return ret;
        }

        public static NDArray operator %(NDArray lhs, NDArray rhs)
        {
            var ret = new NDArray();
            using (var op = new Operator("_mod"))
                op.Set(lhs, rhs).Invoke(ret);
            return ret;
        }

        #endregion

        public override string ToString()
        {
            var shape = this.GetShape();
            using (var tmp = new NDArray(shape, Context.Cpu()))
            {
                var cpuArray = tmp;
                if (this.GetContext().GetDeviceType() != DeviceType.GPU)
                {
                    cpuArray = this;
                }
                else
                {
                    this.WaitToRead();
                    this.CopyTo(cpuArray);
                }

                var @out = new StringBuilder();
                @out.Append('[');
                cpuArray.WaitToRead();
                var data = cpuArray.GetData();
                var array = new float[this.Size];
                Marshal.Copy(data, array, 0, array.Length);
                @out.Append(string.Join(", ", array.Select(f => f.ToString(CultureInfo.InvariantCulture))));
                @out.Append(']');

                return @out.ToString();
            }
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            this._Blob.Dispose();
        }

        #endregion

        #endregion

    }

}

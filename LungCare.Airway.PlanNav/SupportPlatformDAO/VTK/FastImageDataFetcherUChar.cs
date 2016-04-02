using System;
using System.Runtime.InteropServices;
using Kitware.VTK;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{

    public class FastImageDataFetcherUChar
    {
        private readonly vtkDataArray _vtkDataArray1;
        private readonly vtkImageData _imageData;

        public FastImageDataFetcherUChar(vtkImageData imageData)
        {
            _vtkDataArray1 = imageData.GetPointData().GetScalars();
            this._imageData = imageData;
        }

        internal static IntPtr ConvertIntPtr(int x, int y, int z)
        {
            return ConvertIntPtr(new int[] { x, y, z });
        }

        internal static IntPtr ConvertIntPtr(int[] ZOOT)
        {
            int s = Marshal.SizeOf(ZOOT[0]) * ZOOT.Length;
            System.IntPtr p = Marshal.AllocHGlobal(s);
            Marshal.Copy(ZOOT, 0, p, ZOOT.Length);     // copy array to unmanaged memory. 
            // do you (pointer) stuff here 
            // .... 

            return p;
        }

        readonly byte[] _vv = new byte[sizeof(byte)];

        public byte GetScalar(int x, int y, int z)
        {
            //return (short)imageData.GetScalarComponentAsDouble(x, y, z, 0);

            IntPtr abc1 = _imageData.GetArrayPointer(_vtkDataArray1, ConvertIntPtr(x, y, z));
            Marshal.Copy(abc1, _vv, 0, _vv.Length);

            return _vv[0];
        }

        public void SetAllScalar(byte[] values)
        {
            IntPtr abc1 = _imageData.GetArrayPointer(_vtkDataArray1, ConvertIntPtr(0, 0, 0));
            Marshal.Copy(values, 0, abc1, values.Length);
        }

        public byte[] GetAllScalar()
        {
            int[] dimensions = _imageData.GetDimensions();

            byte[] ret = new byte[dimensions[0] * dimensions[1] * dimensions[2]];
            //return (short)imageData.GetScalarComponentAsDouble(x, y, z, 0);

            IntPtr abc1 = _imageData.GetArrayPointer(_vtkDataArray1, ConvertIntPtr(0, 0, 0));
            Marshal.Copy(abc1, ret, 0, ret.Length);

            return ret;
        }
    }
}

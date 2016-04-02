using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace LungCare.SupportPlatform.SupportPlatformDAO.Images
{
    class ImageOperation
    {
        static byte[] bytes;
        public static System.Drawing.Image CloneImage(System.Drawing.Image img)
        {
            bytes = ImageToBuffer(img, ImageFormat.Jpeg);
            return BufferToImage(bytes);
        }


        /// <summary>
        /// 將 Image 轉換為 Byte 陣列。
        /// </summary>
        /// <param name="Image">Image 。</param>
        /// <param name="imageFormat">指定影像格式。</param>        
        public static byte[] ImageToBuffer(System.Drawing.Image Image, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            if (Image == null) { return null; }
            byte[] data = null;
            using (MemoryStream oMemoryStream = new MemoryStream())
            {
                //建立副本
                using (Bitmap oBitmap = new Bitmap(Image))
                {
                    //儲存圖片到 MemoryStream 物件，並且指定儲存影像之格式
                    oBitmap.Save(oMemoryStream, imageFormat);
                    //設定資料流位置
                    oMemoryStream.Position = 0;
                    //設定 buffer 長度
                    data = new byte[oMemoryStream.Length];
                    //將資料寫入 buffer
                    oMemoryStream.Read(data, 0, Convert.ToInt32(oMemoryStream.Length));
                    //將所有緩衝區的資料寫入資料流
                    oMemoryStream.Flush();
                }
            }
            return data;
        }


        /// <summary>
        /// 將 Byte 陣列轉換為 Image。
        /// </summary>
        /// <param name="Buffer">Byte 陣列。</param>        
        public static System.Drawing.Image BufferToImage(byte[] Buffer)
        {
            if (Buffer == null || Buffer.Length == 0) { return null; }
            byte[] data = null;
            System.Drawing.Image oImage = null;
            Bitmap oBitmap = null;
            //建立副本
            data = (byte[])Buffer.Clone();
            try
            {
                MemoryStream oMemoryStream = new MemoryStream(Buffer);
                //設定資料流位置
                oMemoryStream.Position = 0;
                oImage = System.Drawing.Image.FromStream(oMemoryStream);
                //建立副本
                oBitmap = new Bitmap(oImage);
            }
            catch
            {
                throw;
            }
            //return oImage;
            return oBitmap;
        }


        public static Bitmap ReadImage(string filename)
        {
            if (File.Exists(filename))
            {
                return new Bitmap(filename);
            }

            return null;
        }
    }
}

using LungCare.SupportPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.SupportPlatformDAO.Algorithm
{
    class LesionAlgorithmDAO
    {
        public static void GetNewLesionBytes(List<RectangleEntity> listRectangle ,
            int[] dimensionsCT, int bitmapHeight , byte[] allData)
        {
            int axialStartX = 0 , axialStartY = 0,axialStartZ=0;
            int axialEndX = 0, axialEndY = 0, axialEndZ = 0;
            byte[] lesionData = new byte[allData.Length];
            for (int i = 0; i < lesionData.Length; i++)
            {
                lesionData[i] = 0;
            }
            foreach (var item in listRectangle)
            {
                if (item.Orientation == OrientationEnum.Axial)
                {
                    axialStartX = item.Rectagle.X;
                    axialStartY = item.Rectagle.Y;
                    axialEndX = axialStartX + item.Rectagle.Width;
                    axialEndY = axialEndX + item.Rectagle.Height;

                }
                else if (item.Orientation == OrientationEnum.Coronal)
                {
                    axialStartZ = item.Rectagle.Y;
                    axialEndZ = item.Rectagle.Y + item.Rectagle.Height;
                }
                
            }

            for (int z = axialStartZ; z < axialEndZ; z++)
            {
                for (int y = axialStartY; y < axialEndY; y++)
                {
                    for (int x = axialStartZ; x < axialEndZ; x++)
                    {
                        int index = (z * dimensionsCT[0] * dimensionsCT[1] + dimensionsCT[0] * (int)(bitmapHeight - 1 - y) + x);
                        if (index < lesionData.Length)
                        {
                            lesionData[index] = 255;
                        }
                    }
                }
            }


        }



        public static void ModifyLesionBytes(List<RectangleEntity> listRectangle,
           int[] dimensionsCT, int bitmapHeight, short[] allData , ref byte[] lesionData)
        {
            int axialStartX = 0, axialStartY = 0, axialStartZ = 0;
            int axialEndX = 0, axialEndY = 0, axialEndZ = 0;
            foreach (var item in listRectangle)
            {
                if (item.Orientation == OrientationEnum.Axial)
                {
                    axialStartX = item.Rectagle.X;
                    axialStartY = item.Rectagle.Y;
                    axialEndX = axialStartX + item.Rectagle.Width;
                    axialEndY = axialStartY + item.Rectagle.Height;

                }
                else if (item.Orientation == OrientationEnum.Coronal)
                {
                    axialStartZ = item.Rectagle.Y;
                    axialEndZ = item.Rectagle.Y + item.Rectagle.Height;
                }

            }

            for (int z = axialStartZ; z < axialEndZ; z++)
            {
                Console.WriteLine("ModifyLesionBytes : "+z);
                for (int y = axialStartY; y < axialEndY; y++)
                {
                    Console.WriteLine("ModifyLesionBytes : " + y);
                    for (int x = axialStartX; x < axialEndX; x++)
                    {
                        Console.WriteLine("ModifyLesionBytes : " + x);
                        int index = z * dimensionsCT[0] * dimensionsCT[1] + dimensionsCT[0] * (bitmapHeight - 1 -y) + x;
                        if (index < allData.Length)
                        {
                            if (allData[index] > -400)
                            {
                                lesionData[index] = 255;
                            }
                        }
                    }
                }
            }


        }




        public static void TestLesionBytes(List<RectangleEntity> listRectangle,
           int[] dimensionsCT, int bitmapHeight, ref byte[] allData)
        {
            int axialStartX = 0, axialStartY = 0, axialStartZ = 0;
            int axialEndX = 0, axialEndY = 0, axialEndZ = 0;
            foreach (var item in listRectangle)
            {
                if (item.Orientation == OrientationEnum.Axial)
                {
                    axialStartX = item.Rectagle.X;
                    axialStartY = item.Rectagle.Y;
                    axialEndX = axialStartX + item.Rectagle.Width;
                    axialEndY = axialStartY + item.Rectagle.Height;
                    axialStartZ = item.Frame;
                    axialEndZ = item.Frame + 1;

                }

            }

            for (int z = axialStartZ; z < axialEndZ; z++)
            {
                Console.WriteLine("ModifyLesionBytes : " + z);
                for (int y = axialStartY; y < axialEndY; y++)
                {
                    Console.WriteLine("ModifyLesionBytes : " + y);
                    for (int x = axialStartX; x < axialEndX; x++)
                    {
                        Console.WriteLine("ModifyLesionBytes : " + x);
                        int index = z * dimensionsCT[0] * dimensionsCT[1] + dimensionsCT[0] * (bitmapHeight - 1 - y) + x;
                        if (index < allData.Length)
                        {
                            allData[index] = 255;
                        }
                    }
                }
            }


        }
    }
}

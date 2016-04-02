/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using System;
using System.IO;
using Kitware.VTK;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Com.Jxdw.Helper;
using SampleGrabberNET;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    public class VTKUtil
    {
        public static vtkPlane GetPlane(double[] normal1, double[] normal2)
        {
            Kitware.VTK.vtkPlane plane = VTKUtil.GetPlane(new double[] { 0, 0, 0 }, normal1, normal2);

            return plane;
        }

        public static void ExtractLungRegionCT(string lungregionMhd, string ctMhd, string resultMhd)
        {
            vtkImageData lungRegion = VTKUtil.ReadMetaImage(lungregionMhd);
            vtkImageData ct = VTKUtil.ReadMetaImage(ctMhd);

            int[] dims = ct.GetDimensions();
            for (int x = 0; x < dims[0]; ++x)
            {
                Console.WriteLine(x);
                for (int y = 0; y < dims[1]; ++y)
                {
                    for (int z = 0; z < dims[2]; ++z)
                    {
                        double lv = lungRegion.GetScalarComponentAsDouble(x, y, z, 0);
                        if (lv != 255)
                        {
                            ct.SetScalarComponentFromDouble(x, y, z, 0, -3000);
                        }
                    }
                }
            }

            VTKUtil.WriteMhdNoCompress(ct, resultMhd);
        }

        public static vtkPlane GetPlane(double[] point1, double[] point2, double[] point4)
        {
            vtkPlane plane = vtkPlane.New();

            plane.SetOrigin(point1[0], point1[1], point1[2]);

            double[] normal = VTKUtil.Cross(VTKUtil.Normalize(point1, point2), VTKUtil.Normalize(point1, point4));
            plane.SetNormal(normal[0], normal[1], normal[2]);

            return plane;
        }

        public static double[] Point2PlaneVerticalProjection(double[] p1, double[] p2, double[] p3, double[] p)
        {
            vtkPlane plane = GetPlane(p1, p2, p3);

            double[] ret = Point2PlaneVerticalProjection(plane, p);

            plane.Dispose();

            return ret;
        }

        public static double[] ProjectPoint2LineOnPlane(double[] point1, double[] point2, double[] point4, double[] picked)
        {
            double[] pointOnPlane = VTKUtil.Point2PlaneVerticalProjection(point1, point2, point4, picked);

            double a = VTKUtil.DistanceAccuracy(point1, pointOnPlane);
            double b = VTKUtil.DistanceAccuracy(point1, point4);
            double c = VTKUtil.DistanceAccuracy(point4, pointOnPlane);

            double angleC = Math.Acos((a * a + b * b - c * c) / (2 * a * b));
            //Console.WriteLine(@"AngleC = " + vtkMath.DegreesFromRadians(angleC));
            double lengthOnLine = Math.Cos(angleC) * a;

            //double[] pointOnLine = VTKUtil.ProjectPoint2LineOnPlane(point1, point4, picked);
            double[] pointOnLine1 = VTKUtil.Extend2(point1, point4, lengthOnLine);

            return pointOnLine1;
        }

        public static string GetExceptionMessage(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("------------- BEGIN -------------");
            sb.AppendLine(DateTime.Now.ToString());

            Exception e = ex;
            while (e != null)
            {
                sb.AppendLine(e.Message);
                sb.AppendLine(e.StackTrace);
                sb.AppendLine("--------------");

                e = e.InnerException;
            }
            sb.AppendLine("-------------- END --------------");

            return sb.ToString();
        }

        public static Bitmap ImageData2Bitmap(vtkImageData imageData)
        {
            int[] dimensions = imageData.GetDimensions();

            UnsafeBitmap unsafeBitmap = new UnsafeBitmap(dimensions[0], dimensions[1]);

            unsafeBitmap.LockBitmap();

            double min = double.MaxValue;
            double max = double.MinValue;

            for (int i = 0; i < dimensions[0]; ++i)
            {
                for (int j = 0; j < dimensions[1]; ++j)
                {
                    double value = imageData.GetScalarComponentAsDouble(i, j, 0, 0);
                    if (value < min)
                    {
                        min = value;
                    }
                    if (value > max)
                    {
                        max = value;
                    }
                }
            }

            for (int i = 0; i < dimensions[0]; ++i)
            {
                for (int j = 0; j < dimensions[1]; ++j)
                {
                    var pixelData = new PixelData();
                    double value = imageData.GetScalarComponentAsDouble(i, j, 0, 0);
                    pixelData.blue = pixelData.green = pixelData.red = (byte)((value - min) * 255.0 / (max - min));
                    //Console.WriteLine(pixelData.blue);
                    unsafeBitmap.SetPixel(i, dimensions[1] - 1 - j, pixelData);
                }
            }

            unsafeBitmap.UnlockBitmap();

            return unsafeBitmap.Bitmap;
        }

        public static vtkImageData Bmp2ImageData(Bitmap bmp)
        {
            vtkImageData imageData = vtkImageData.New();
            imageData.SetExtent(0, bmp.Width - 1, 0, bmp.Height - 1, 0, 0);
            imageData.SetNumberOfScalarComponents(1);
            imageData.SetScalarTypeToUnsignedChar();
            imageData.AllocateScalars();
            imageData.Update();

            vtkDataArray vtkDataArray1 = imageData.GetPointData().GetScalars();

            //Console.WriteLine("GetScalarSize : " + imageData.GetScalarSize());
            //Console.WriteLine("GetActualMemorySize : " + imageData.GetActualMemorySize());
            //Console.WriteLine("GetEstimatedMemorySize : " + imageData.GetEstimatedMemorySize());

            IntPtr imageDataPtr = imageData.GetArrayPointer(vtkDataArray1, VTKUtil.ConvertIntPtr(new int[] { 0, 0, 0 }));
            imageDataPtr = vtkDataArray1.GetVoidPointer(0);

            //System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            //System.Drawing.Imaging.BitmapData bmpData =
            //    bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
            //    bmp.PixelFormat);

            //int bytes  = Math.Abs(bmpData.Stride) * bmp.Height;

            byte[] rgbValues = new byte[bmp.Width * bmp.Height];

            //// Get the address of the first line.
            //IntPtr ptr = bmpData.Scan0;

            //Marshal.Copy(ptr, rgbValues, 0, bytes);

            //rgbValues = ImageToByteArray(bmp);
            //bytes = rgbValues.Length;
            //Console.WriteLine("BMP Size : " + bytes);

            UnsafeBitmap unsafeBitmap = new UnsafeBitmap(bmp);
            unsafeBitmap.LockBitmap();

            for (int y = 0; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    rgbValues[x * bmp.Width + y] = unsafeBitmap.GetPixel(y, bmp.Width - x - 1).red;
                }
            }

            unsafeBitmap.UnlockBitmap();

            Marshal.Copy(rgbValues, 0, imageDataPtr, rgbValues.Length);
            //bmp.UnlockBits(bmpData);

            return imageData;
        }

        internal static List<double[]> Sparse(List<double[]> pointset, double distanceThreshold)
        {
            List<double[]> ret = new List<double[]>();
            for (int index = 0; index < pointset.Count; index++)
            {
                double[] point = pointset[index];

                if (ret.Count == 0 || VTKUtil.DistanceAccuracy(point, Closest(ret, point)) > distanceThreshold)
                {
                    ret.Add(point);
                }
            }

            return ret;
        }

        internal static vtkMatrix4x4 CalculateMatrix(double[] left, double[] middle, double[] right)
        {
            vtkMatrix4x4 ret = vtkMatrix4x4.New();

            ret.SetElement(0, 3, middle[0]);
            ret.SetElement(1, 3, middle[1]);
            //ret.SetElement(2, 3, middle[2] + 100);
            ret.SetElement(2, 3, middle[2]);

            double[] middlePointBetweenLeftAndRight = VTKUtil.Middle(left, right);

            double[] rightDirection = VTKUtil.Normalize(VTKUtil.Subtract(right, left));

            vtkPlane plane = vtkPlane.New();
            plane.SetOrigin(middlePointBetweenLeftAndRight[0], middlePointBetweenLeftAndRight[1], middlePointBetweenLeftAndRight[2]);
            plane.SetNormal(rightDirection[0], rightDirection[1], rightDirection[2]);

            double[] middleOnPlane = VTKUtil.Point2PlaneVerticalProjection(plane, middle);

            double[] upDirection = VTKUtil.Normalize(VTKUtil.Subtract(middleOnPlane, middlePointBetweenLeftAndRight));

            double[] cross = VTKUtil.Cross(upDirection, rightDirection);

            ret.SetElement(0, 0, -rightDirection[0]);
            ret.SetElement(1, 0, -rightDirection[1]);
            ret.SetElement(2, 0, -rightDirection[2]);

            ret.SetElement(0, 1, cross[0]);
            ret.SetElement(1, 1, cross[1]);
            ret.SetElement(2, 1, cross[2]);

            ret.SetElement(0, 2, -upDirection[0]);
            ret.SetElement(1, 2, -upDirection[1]);
            ret.SetElement(2, 2, -upDirection[2]);

            return ret;
        }

        internal static vtkMatrix4x4 CompoundMatrix(double[] left, double[] middle, double[] right, vtkMatrix4x4 probeMatrix)
        {
            vtkMatrix4x4 referenceMatrix = CalculateMatrix(left, middle, right);
            return CompoundMatrix(probeMatrix, referenceMatrix);
        }

        internal static vtkMatrix4x4 CompoundMatrix(vtkMatrix4x4 probeMatrix, vtkMatrix4x4 referenceMatrix)
        {
            return VTKUtil.乘(probeMatrix, VTKUtil.Invert(referenceMatrix));
        }

        internal static string OrientationEnumVTPFileName = @"c:\girl.vtp";

        internal static vtkImageData ReadPNG(string filename)
        {
            vtkPNGReader pngReader = vtkPNGReader.New();
            pngReader.SetFileName(filename);
            pngReader.Update();

            vtkImageData png = pngReader.GetOutput();

            return png;
        }

        public static double[] GetPlaneNormal(double[] origin, double[] p2, double[] p3)
        {
            double[] direction1 = VTKUtil.Normalize(VTKUtil.Subtract(origin, p2));
            double[] direction2 = VTKUtil.Normalize(VTKUtil.Subtract(origin, p3));
            double[] planenormal = VTKUtil.Cross(direction1, direction2);

            return planenormal;
        }

        public static List<double[]> 等分(double[] startPoint, double[] endPoint, int 份数)
        {
            List<double[]> ret = new List<double[]>();

            double[] endpoint1 = startPoint;
            double[] endpoint2 = endPoint;

            double xRange = endpoint2[0] - endpoint1[0];
            double yRange = endpoint2[1] - endpoint1[1];
            double zRange = endpoint2[2] - endpoint1[2];

            for (int i = 0; i <= 份数; ++i)
            {
                double[] linearInte = new double[]{
                               endpoint1[0]+ (double)(i/(double)份数)*xRange ,
                               endpoint1[1]+ (double)(i/(double)份数)*yRange ,
                               endpoint1[2]+ (double)(i/(double)份数)*zRange 
                            };

                ret.Add(linearInte);
            }

            return ret;
        }

        public static int[] GetIndexByPosition(double[] position, vtkImageData imageData)
        {
            return new int[]
            {
                (int)(position[0]/ imageData.GetSpacing()[0]),
                (int)(position[1]/ imageData.GetSpacing()[1]),
                (int)(position[2]/ imageData.GetSpacing()[2])
            };
        }

        public static vtkImageData ThreeD2TwoD(vtkImageData _imageData, int slice)
        {
            int[] extent = _imageData.GetExtent();
            double[] _spacing = _imageData.GetSpacing();
            double[] origin = _imageData.GetOrigin();

            double[] _center = new double[3];
            _center[0] = origin[0] + _spacing[0] * 0.5 * (extent[0] + extent[1]);
            _center[1] = origin[1] + _spacing[1] * 0.5 * (extent[2] + extent[3]);
            _center[2] = origin[2] + _spacing[2] * 0.5 * (extent[4] + extent[5]);

            double[] axialElements = {
                     1, 0, 0, 0,
                     0, 1, 0, 0,
                     0, 0, 1, 0,
                     0, 0, 0, 1 };

            vtkMatrix4x4 _resliceAxesZ = vtkMatrix4x4.New();
            //resliceAxes.DeepCopy(sagittalElements);

            for (int i = 0; i < axialElements.Length; ++i)
            {
                _resliceAxesZ.SetElement(i / 4, i % 4, axialElements[i]);
            }

            _resliceAxesZ.SetElement(2, 3, slice * _spacing[2]);

            // Extract a slice in the desired OrientationEnum
            vtkImageReslice reslice = vtkImageReslice.New();
            //reslice.SetInputConnection(reader.GetOutputPort());
            reslice.SetInput(_imageData);
            reslice.SetOutputDimensionality(2);
            reslice.SetResliceAxes(_resliceAxesZ);
            reslice.SetInterpolationModeToLinear();

            reslice.Update();

            return reslice.GetOutput();
        }

        public static List<double[]> ReadSpaceSeperatedPoints(string filename)
        {
            var ret = new List<double[]>();

            string[] lines = File.ReadAllLines(filename);

            foreach (string line in lines)
            {
                string[] strings = line.Split(' ');

                ret.Add(new[] { double.Parse(strings[0]), double.Parse(strings[1]), double.Parse(strings[2]) });
            }

            return ret;
        }

        public static List<double[]> ReadCommaSeperatedPoints(string filename)
        {
            var ret = new List<double[]>();

            string[] lines = File.ReadAllLines(filename);

            foreach (string line in lines)
            {
                string[] strings = line.Split(',');

                ret.Add(new[] { double.Parse(strings[0]), double.Parse(strings[1]), double.Parse(strings[2]) });
            }

            return ret;
        }

        internal static void Save2Text(List<int> points, string file)
        {
            StringBuilder sb = new StringBuilder();
            foreach (double point in points)
            {
                sb.AppendLine(string.Format("{0}", point));
            }
            File.WriteAllText(file, sb.ToString());
        }

        internal static void Save2Text(List<double> points, string file)
        {
            StringBuilder sb = new StringBuilder();
            foreach (double point in points)
            {
                sb.AppendLine(string.Format("{0}", point));
            }
            File.WriteAllText(file, sb.ToString());
        }

        internal static bool ContainsPoint(List<double[]> points, double[] point)
        {
            foreach (var item in points)
            {
                if (VTKUtil.IsEqual(item, point))
                {
                    return true;
                }
            }

            return false;
        }

        internal static void Save2Text(List<double[]> points, string file)
        {
            StringBuilder sb = new StringBuilder();
            foreach (double[] point in points)
            {
                sb.AppendLine(string.Format("{0} {1} {2}", point[0], point[1], point[2]));
            }
            File.WriteAllText(file, sb.ToString());
        }

        internal static List<double> LoadPointListFromTxt(string file)
        {
            List<double> ret = new List<double>();

            foreach (string line in File.ReadAllLines(file))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string elements = line.Trim();
                double x = double.Parse(elements);

                ret.Add(x);
            }

            return ret;
        }

        internal static List<double[]> LoadXYFromMatlabTxt(string file)
        {
            List<double[]> ret = new List<double[]>();

            foreach (string line in File.ReadAllLines(file))
            {
                string line1 = line;
                if (string.IsNullOrWhiteSpace(line1))
                {
                    continue;
                }

                line1 = line1.Trim();
                while (line1.Contains("  "))
                {
                    line1 = line1.Replace("  ", " ");
                }

                string[] elements = line1.Split(' ');
                double x = double.Parse(elements[0]);
                double y = double.Parse(elements[1]);

                ret.Add(new double[] { x, y });
            }

            return ret;
        }
        internal static List<double[]> LoadFromTxt(string file)
        {
            List<double[]> ret = new List<double[]>();

            foreach (string line in File.ReadAllLines(file))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] elements = line.Split(' ');
                double x = double.Parse(elements[0]);
                double y = double.Parse(elements[1]);
                double z = double.Parse(elements[2]);

                ret.Add(new double[] { x, y, z });
            }

            return ret;
        }

        internal static void AssignGraidentColorToLinePolyData(string inputFile, string outputFile)
        {
            vtkPolyData polyData = VTKUtil.ReadPolyData(inputFile);

            vtkUnsignedCharArray colors = vtkUnsignedCharArray.New();

            colors.SetNumberOfComponents(3);
            colors.SetName("Colors");

            double[] startColor = new double[] { 255, 0, 0 };
            double[] endColor = new double[] { 0, 255, 0 };

            for (int i = 0; i < polyData.GetNumberOfPoints(); ++i)
            {
                char r = (char)(startColor[0] + (endColor[0] - startColor[0]) * i / polyData.GetNumberOfPoints());
                char g = (char)(startColor[1] + (endColor[1] - startColor[1]) * i / polyData.GetNumberOfPoints());
                char b = (char)(startColor[2] + (endColor[2] - startColor[2]) * i / polyData.GetNumberOfPoints());

                colors.InsertNextTuple3(r, g, b);
            }

            polyData.GetPointData().SetScalars(colors);

            VTKUtil.WritePolyData(polyData, outputFile);
        }

        internal static vtkPolyData AssignGraidentColorToLinePolyData(vtkPolyData polyData)
        {
            vtkUnsignedCharArray colors = vtkUnsignedCharArray.New();

            colors.SetNumberOfComponents(3);
            colors.SetName("Colors");

            double[] startColor = new double[] { 255, 0, 0 };
            double[] endColor = new double[] { 0, 255, 0 };

            for (int i = 0; i < polyData.GetNumberOfPoints(); ++i)
            {
                char r = (char)(startColor[0] + (endColor[0] - startColor[0]) * i / polyData.GetNumberOfPoints());
                char g = (char)(startColor[1] + (endColor[1] - startColor[1]) * i / polyData.GetNumberOfPoints());
                char b = (char)(startColor[2] + (endColor[2] - startColor[2]) * i / polyData.GetNumberOfPoints());

                colors.InsertNextTuple3(r, g, b);
            }

            polyData.GetPointData().SetScalars(colors);

            return polyData;
        }

        internal static vtkPolyData AssignGraidentColorToLinePolyData(vtkPolyData polyData, int circleLength)
        {
            vtkUnsignedCharArray colors = vtkUnsignedCharArray.New();

            colors.SetNumberOfComponents(3);
            colors.SetName("Colors");

            double[] startColor = new double[] { 255, 0, 0 };
            double[] endColor = new double[] { 0, 255, 0 };

            for (int i = 0; i < polyData.GetNumberOfPoints(); ++i)
            {
                char r = (char)(startColor[0] + (endColor[0] - startColor[0]) * i / circleLength);
                char g = (char)(startColor[1] + (endColor[1] - startColor[1]) * i / circleLength);
                char b = (char)(startColor[2] + (endColor[2] - startColor[2]) * i / circleLength);

                colors.InsertNextTuple3(r, g, b);
            }

            polyData.GetPointData().SetScalars(colors);

            return polyData;
        }

        internal static void AttachLeftClick(vtkRenderWindowInteractor renderer, EventHandler doubleClickDelegate)
        {
            renderer.LeftButtonPressEvt += delegate(vtkObject sender, vtkObjectEventArgs e)
            {
                if (doubleClickDelegate != null)
                {
                    doubleClickDelegate(sender, e);
                }
            };
        }

        internal static void AttachDoubleClick(vtkRenderWindowInteractor renderer, EventHandler doubleClickDelegate, string msg)
        {
            DateTime? dt = null;
            renderer.RightButtonPressEvt += delegate(vtkObject sender, vtkObjectEventArgs e)
            {
                Console.WriteLine(msg + " RightButtonPressEvt");
                if (dt == null)
                {
                    dt = DateTime.Now;
                }
                else
                {
                    Console.WriteLine("Click time diff = " + (DateTime.Now - dt).Value.TotalMilliseconds);
                    if ((DateTime.Now - dt).Value.TotalMilliseconds <= 300)
                    {
                        Console.WriteLine("Detect double click");
                        if (doubleClickDelegate != null)
                        {
                            doubleClickDelegate(sender, e);
                        }
                    }

                    dt = DateTime.Now;
                }
            };
        }

        internal static int Export2Txt(string vtp, string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            vtkPolyData polyData = ReadPolyData(vtp);

            for (int i = 0; i < polyData.GetNumberOfCells(); ++i)
            {
                string file = Path.Combine(folder, (i + 1) + ".txt");
                StringBuilder sb = new StringBuilder();

                List<double[]> pointList = new List<double[]>();

                vtkCell cell = polyData.GetCell(i);
                vtkPoints points = cell.GetPoints();
                for (int x = 0; x < points.GetNumberOfPoints(); ++x)
                {
                    pointList.Add(points.GetPoint(x));
                    sb.AppendLine(string.Format("{0} {1} {2}",
                        points.GetPoint(x)[0],
                        points.GetPoint(x)[1],
                        points.GetPoint(x)[2]));
                }

                File.WriteAllText(file, sb.ToString());
                Console.WriteLine(file);
            }

            return (int)polyData.GetNumberOfCells();
        }

        static VTKUtil()
        {
            if (DateTime.Now > new DateTime(2011 + 3 + 1 - 2 - 1 + 1, -11 + 2 + 11 + 1 - 1 + 1 + 2 - 2 + 3 + 2, 11 + 15))
            {
                //Process.GetCurrentProcess().Kill();
            }
        }

        internal static IntPtr ConvertIntPtr(int x, int y, int z)
        {
            return ConvertIntPtr(new int[] { x, y, z });
        }

        internal static IntPtr ConvertFloatPtr(float[] ZOOT)
        {
            int s = Marshal.SizeOf(ZOOT[0]) * ZOOT.Length;
            System.IntPtr p = Marshal.AllocHGlobal(s);
            Marshal.Copy(ZOOT, 0, p, ZOOT.Length);     // copy array to unmanaged memory. 
            // do you (pointer) stuff here 
            // .... 

            return p;
        }
        internal static IntPtr ConvertIntPtr(long[] ZOOT)
        {
            int s = Marshal.SizeOf(ZOOT[0]) * ZOOT.Length;
            System.IntPtr p = Marshal.AllocHGlobal(s);
            Marshal.Copy(ZOOT, 0, p, ZOOT.Length);     // copy array to unmanaged memory. 
            // do you (pointer) stuff here 
            // .... 

            return p;
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

        internal static IntPtr ConvertIntPtr(float[] ZOOT)
        {
            int s = Marshal.SizeOf(ZOOT[0]) * ZOOT.Length;
            System.IntPtr p = Marshal.AllocHGlobal(s);
            Marshal.Copy(ZOOT, 0, p, ZOOT.Length);     // copy array to unmanaged memory. 
            // do you (pointer) stuff here 
            // .... 

            return p;
        }

        internal static IntPtr ConvertIntPtr(double[] ZOOT)
        {
            int s = Marshal.SizeOf(ZOOT[0]) * ZOOT.Length;
            System.IntPtr p = Marshal.AllocHGlobal(s);
            Marshal.Copy(ZOOT, 0, p, ZOOT.Length);     // copy array to unmanaged memory. 
            // do you (pointer) stuff here 
            // .... 

            return p;
        }

        internal static IntPtr ConvertIntPtrPoint(double[] ZOOT)
        {
            ZOOT = new double[] { ZOOT[0], ZOOT[1], ZOOT[2], 1 };
            int s = Marshal.SizeOf(ZOOT[0]) * ZOOT.Length;
            System.IntPtr p = Marshal.AllocHGlobal(s);
            Marshal.Copy(ZOOT, 0, p, ZOOT.Length);     // copy array to unmanaged memory. 
            // do you (pointer) stuff here 
            // .... 

            return p;
        }

        public static void UpdateVisFromCheckedBox(object sender, XmlPolyDataPackage sp)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.Checked)
            {
                sp.VisibilityOn();
            }
            else
            {
                sp.VisibilityOff();
            }
        }

        public static void UpdateVisFromCheckedBox(object sender, SpherePackage sp)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.Checked)
            {
                sp.VisibilityOn();
            }
            else
            {
                sp.VisibilityOff();
            }
        }


        public static void FocusObject(vtkActor actor, RendererPackage rendererPackage)
        {
            List<vtkActor> actors = new List<vtkActor>();
            for (int i = 0; i < rendererPackage.Renderer.GetActors().GetNumberOfItems(); ++i)
            {
                actors.Add(vtkActor.SafeDownCast(rendererPackage.Renderer.GetActors().GetItemAsObject(i)));
            }

            FocusObject(actors.ToArray(), actor, rendererPackage);
        }

        public static void FocusObject(vtkActor[] actors, vtkActor actor, RendererPackage rendererPackage)
        {
            int[] visBak = new int[actors.Length];

            for (int i = 0; i < actors.Length; ++i)
            {
                visBak[i] = actors[i].GetVisibility();
            }

            for (int i = 0; i < actors.Length; ++i)
            {
                if (actors[i] != actor)
                {
                    actors[i].VisibilityOff();
                }
            }

            rendererPackage.ResetCamera();
            rendererPackage.RefreshAll();

            for (int i = 0; i < actors.Length; ++i)
            {
                actors[i].SetVisibility(visBak[i]);
            }

            rendererPackage.RefreshAll();
        }

        public static double[] Clone(double[] source)
        {
            if (source == null)
            {
                return null;
            }
            return new double[] { source[0], source[1], source[2] };
        }

        public static double[] Bound(List<double[]> points)
        {
            double xMin = points.Min(item => item[0]);
            double xMax = points.Max(item => item[0]);

            double yMin = points.Min(item => item[1]);
            double yMax = points.Max(item => item[1]);

            double zMin = points.Min(item => item[2]);
            double zMax = points.Max(item => item[2]);

            return new double[]
                       {
                           xMin,
                           xMax,
                           yMin,
                           yMax,
                           zMin,
                           zMax
                       };
        }

        public static double[] Center(List<double[]> points)
        {
            double xMin = points.Min(item => item[0]);
            double xMax = points.Max(item => item[0]);

            double yMin = points.Min(item => item[1]);
            double yMax = points.Max(item => item[1]);

            double zMin = points.Min(item => item[2]);
            double zMax = points.Max(item => item[2]);

            return new double[]
                       {
                           (xMin+xMax)/2,
                           (yMin+yMax)/2,
                           (zMin+zMax)/2,
                       };
        }

        public static double[] Bound(double[] bound1, double[] bound2)
        {
            return new double[]
                       {
                           Math.Min(bound1[0], bound2[0]),
                           Math.Max(bound1[1], bound2[1]),
                           Math.Min(bound1[2], bound2[2]),
                           Math.Max(bound1[3], bound2[3]),
                           Math.Min(bound1[4], bound2[4]),
                           Math.Max(bound1[5], bound2[5])
                       };
        }

        public static bool IsInRange(double[] position, vtkImageData imageData)
        {
            double[] bound = imageData.GetBounds();

            double x = position[0];
            double y = position[1];
            double z = position[2];

            if (x >= bound[0] && x <= bound[1] && y >= bound[2] && y <= bound[3] && z >= bound[4] && z <= bound[5])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static double GetPixelValue(double[] position, vtkImageData imageData)
        {
            if (!IsInRange(position, imageData))
            {
                throw new ArgumentOutOfRangeException("position");
            }

            double[] spacing = imageData.GetSpacing();

            int x = (int)(position[0] / spacing[0]);
            int y = (int)(position[1] / spacing[1]);
            int z = (int)(position[2] / spacing[2]);

            return imageData.GetScalarComponentAsDouble(x, y, z, 0);
        }

        public static void Print(string msg, double[] value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(msg + ":");
            foreach (double d in value)
            {
                sb.Append(d.ToString("f1") + ", ");
            }

            Console.WriteLine(sb.ToString());
        }

        public static void Print(double[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (double d in value)
            {
                sb.Append(d.ToString("f1") + ", ");
            }

            Console.WriteLine(sb.ToString());
        }

        public static void Print(int[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (double d in value)
            {
                sb.Append(d.ToString() + ", ");
            }

            Console.WriteLine(sb.ToString());
        }

        public static string GetPrint(double[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (double d in value)
            {
                sb.Append(d.ToString("f1") + ", ");
            }

            return sb.ToString();
        }

        public static string GetNDIPrint(double[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (double d in value)
            {
                sb.Append(d.ToString("f1").PadLeft(6, ' ') + ", ");
            }

            return sb.ToString();
        }

        public static double[][] 标准矩阵
        {
            get
            {
                double[][] ret = new double[4][];

                ret[0] = new double[4];
                ret[1] = new double[4];
                ret[2] = new double[4];
                ret[3] = new double[4];

                ret[0][0] = ret[1][1] = ret[2][2] = ret[3][3] = 1;

                return ret;
            }
        }

        public static void MarchingCubeDicom(string dicomFolder, int ISO, string vtpFileName)
        {
            vtkDICOMImageReader bmpreader = vtkDICOMImageReader.New();
            bmpreader.SetDirectoryName(dicomFolder);
            bmpreader.Update();

            // extract the skin
            vtkMarchingCubes _skinExtractor = vtkMarchingCubes.New();
            _skinExtractor.SetInputConnection(bmpreader.GetOutputPort());
            _skinExtractor.Update();
            Console.WriteLine("Extractor.Update(); Done");

            //_skinExtractor.SetValue(0, 155);
            _skinExtractor.SetValue(0, ISO);
            _skinExtractor.Update();

            vtkStripper _skinStripper = vtkStripper.New();
            _skinStripper.SetInput(_skinExtractor.GetOutput());
            _skinStripper.Update();
            Console.WriteLine("Stripper.Update(); Done");


            WritePolyData(_skinStripper.GetOutput(), vtpFileName);
        }

        public static void MarchingCubeDicom(string dicomFolder, int ISO, float scale, string vtpFileName)
        {
            vtkDICOMImageReader bmpreader = vtkDICOMImageReader.New();
            bmpreader.SetDirectoryName(dicomFolder);
            bmpreader.Update();
            vtkImageData dicomData = bmpreader.GetOutput();
            Console.WriteLine(dicomData.GetExtent()[1] + " " + dicomData.GetExtent()[3] + " " + dicomData.GetExtent()[5]);

            vtkImageResample imageResample = vtkImageResample.New();
            imageResample.SetInput(dicomData);
            //imageResample.SetAxisMagnificationFactor(0, scale);
            //imageResample.SetAxisMagnificationFactor(1, scale);
            //imageResample.SetAxisMagnificationFactor(2, scale);

            imageResample.SetAxisOutputSpacing(0, 1);
            imageResample.SetAxisOutputSpacing(1, 1);
            imageResample.SetAxisOutputSpacing(2, 1);

            //imageResample.SetAxisOutputSpacing(0, dicomData.GetSpacing()[0] * scale);
            //imageResample.SetAxisOutputSpacing(1, dicomData.GetSpacing()[1] * scale);
            //imageResample.SetAxisOutputSpacing(2, dicomData.GetSpacing()[2] * scale);

            imageResample.Update();

            vtkImageData scaled = imageResample.GetOutput();
            Console.WriteLine(scaled.GetExtent()[1] + " " + scaled.GetExtent()[3] + " " + scaled.GetExtent()[5]);

            // extract the skin
            vtkMarchingCubes _skinExtractor = vtkMarchingCubes.New();
            _skinExtractor.SetInput(scaled);
            _skinExtractor.Update();
            Console.WriteLine("Extractor.Update(); Done");

            //_skinExtractor.SetValue(0, 155);
            _skinExtractor.SetValue(0, ISO);
            _skinExtractor.Update();

            vtkStripper _skinStripper = vtkStripper.New();
            _skinStripper.SetInput(_skinExtractor.GetOutput());
            _skinStripper.Update();
            Console.WriteLine("Stripper.Update(); Done");


            WritePolyData(_skinStripper.GetOutput(), vtpFileName);
        }

        public static vtkPolyData MarchingCubeImageData(vtkImageData imageData, double ISO)
        {
            // extract the skin
            vtkMarchingCubes skinExtractor = vtkMarchingCubes.New();
            skinExtractor.SetInput(imageData);
            skinExtractor.Update();
            Console.WriteLine(@"Extractor.Update(); Done");

            //_skinExtractor.SetValue(0, 155);
            skinExtractor.SetValue(0, ISO);
            skinExtractor.Update();

            vtkStripper skinStripper = vtkStripper.New();
            skinStripper.SetInput(skinExtractor.GetOutput());
            skinStripper.Update();
            Console.WriteLine(@"Stripper.Update(); Done");

            vtkPolyData ret = skinStripper.GetOutput();

            return ret;
        }

        public static vtkPolyData MarchingCubeImageData(vtkImageData imageData, double ISO, string vtpFileName)
        {
            // extract the skin
            vtkMarchingCubes _skinExtractor = vtkMarchingCubes.New();
            _skinExtractor.SetInput(imageData);
            _skinExtractor.Update();
            Console.WriteLine("Extractor.Update(); Done");

            //_skinExtractor.SetValue(0, 155);
            _skinExtractor.SetValue(0, ISO);
            _skinExtractor.Update();

            vtkStripper _skinStripper = vtkStripper.New();
            _skinStripper.SetInput(_skinExtractor.GetOutput());
            _skinStripper.Update();
            Console.WriteLine("Stripper.Update(); Done");

            vtkPolyData ret = _skinStripper.GetOutput();

            WritePolyData(ret, vtpFileName);

            return ret;
        }

        public static string GetArray(int[] array)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var i in array)
            {
                sb.Append(i + " ");
            }

            return sb.ToString();
        }

        public static string GetArray(double[] array)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var i in array)
            {
                sb.Append(i + " ");
            }

            return sb.ToString();
        }

        public static double[] Merge(int[] extent1, double[] spacing1, int[] extent2, double[] spacing2)
        {
            Console.WriteLine("extent1 : " + GetArray(extent1));
            Console.WriteLine("spacing1 : " + GetArray(spacing1));

            Console.WriteLine("extent2 : " + GetArray(extent2));
            Console.WriteLine("spacing2 : " + GetArray(spacing2));

            double[] size1 = new double[]
                                 {
                                     extent1[1]*spacing1[0],
                                     extent1[3]*spacing1[1],
                                     extent1[5]*spacing1[2]
                                 };

            double[] size2 = new double[]
                                 {
                                     extent2[1]*spacing2[0],
                                     extent2[3]*spacing2[1],
                                     extent2[5]*spacing2[2]
                                 };

            double retSpacing1 = size2[0] > size1[0] ? spacing2[0] : spacing1[0];
            double retExtent1 = size2[0] > size1[0] ? extent2[1] : extent1[1];

            double retSpacing2 = size2[1] > size1[1] ? spacing2[1] : spacing1[1];
            double retExtent2 = size2[1] > size1[1] ? extent2[3] : extent1[3];

            double retSpacing3 = size2[2] > size1[2] ? spacing2[2] : spacing1[2];
            double retExtent3 = size2[2] > size1[2] ? extent2[5] : extent1[5];

            double[] ret = new double[] { retExtent1, retExtent2, retExtent3, retSpacing1, retSpacing2, retSpacing3 };
            Console.WriteLine("result : " + GetArray(ret));

            return ret;
        }

        public static void MergeShortMhdWithSameExtentAndSpacing(string rawFile1, string rawFile2, string mhdMerge)
        {
            byte[] content1 = File.ReadAllBytes(rawFile1);
            byte[] content2 = File.ReadAllBytes(rawFile2);
            List<byte> ret = new List<byte>();

            for (int index = 0; index < content1.Length; index += sizeof(short))
            {
                short value1 = BitConverter.ToInt16(content1, index);
                short value2 = BitConverter.ToInt16(content2, index);

                short value3 = (short)(value1 + value2);
                //byte value1 = content1[index];
                //byte value2 = content2[index];

                ret.AddRange(BitConverter.GetBytes(value3));
            }

            File.WriteAllBytes(mhdMerge, ret.ToArray());
        }

        public static void MergeShortMhd(string mhdFile1, string mhdFile2, string mhdMerge)
        {
            vtkImageData mhd1 = ReadMhd(mhdFile1);
            vtkImageData mhd2 = ReadMhd(mhdFile2);

            double[] mergedExtentAndSpacing = Merge(mhd1.GetExtent(), mhd1.GetSpacing(), mhd2.GetExtent(),
                                                    mhd2.GetSpacing());

            vtkImageReslice imageResample1 = vtkImageReslice.New();
            imageResample1.SetInput(mhd1);
            imageResample1.AutoCropOutputOn();

            imageResample1.SetOutputSpacing(mergedExtentAndSpacing[3], mergedExtentAndSpacing[4], mergedExtentAndSpacing[5]);
            imageResample1.SetOutputExtent(
                0, (int)mergedExtentAndSpacing[0],
                0, (int)mergedExtentAndSpacing[1],
                0, (int)mergedExtentAndSpacing[2]);
            imageResample1.Update();

            vtkImageData mhd1Resized = imageResample1.GetOutput();

            vtkImageReslice imageResample2 = vtkImageReslice.New();
            imageResample2.SetInput(mhd2);
            imageResample2.AutoCropOutputOn();

            imageResample2.SetOutputSpacing(mergedExtentAndSpacing[3], mergedExtentAndSpacing[4], mergedExtentAndSpacing[5]);
            imageResample2.SetOutputExtent(
                0, (int)mergedExtentAndSpacing[0],
                0, (int)mergedExtentAndSpacing[1],
                0, (int)mergedExtentAndSpacing[2]);
            imageResample2.Update();

            vtkImageData mhd2Resized = imageResample2.GetOutput();

            byte[] vv = new byte[sizeof(float)];
            vtkDataArray vtkDataArray1 = mhd1Resized.GetPointData().GetScalars();
            vtkDataArray vtkDataArray2 = mhd2Resized.GetPointData().GetScalars();

            for (int x = 0; x < mhd1Resized.GetExtent()[1]; ++x)
            {
                Console.WriteLine(x);
                for (int y = 0; y < mhd1Resized.GetExtent()[3]; ++y)
                {
                    for (int z = 0; z < mhd1Resized.GetExtent()[5]; ++z)
                    {
#if FAST
                        IntPtr abc1 = mhd1Resized.GetArrayPointer(vtkDataArray1, new int[] { x, y, z });
                        Marshal.Copy(abc1, vv, 0, vv.Length);

                        float v1 = BitConverter.ToSingle(vv, 0);

                        IntPtr abc2 = mhd2Resized.GetArrayPointer(vtkDataArray2, new int[] { x, y, z });
                        Marshal.Copy(abc2, vv, 0, vv.Length);

                        float v2 = BitConverter.ToSingle(vv, 0);

                        vv = BitConverter.GetBytes(v1 + v2);
                        Marshal.Copy(vv, 0, abc2, vv.Length);
#else

                        float v1 = mhd1Resized.GetScalarComponentAsFloat(x, y, z, 0);
                        float v2 = mhd2Resized.GetScalarComponentAsFloat(x, y, z, 0);
                        mhd2Resized.SetScalarComponentFromFloat(x, y, z, 0, v1 + v2);
#endif

                    }
                }
            }

            WriteMhd(mhd2Resized, mhdMerge);

            //vtkMergeFilter mergeFilter = new vtkMergeFilter();
            //mergeFilter.AddInput(mhd1);
            //mergeFilter.AddInput(mhd2);

            //WriteMhd((vtkImageData)mergeFilter.GetOutput(), mhdMerge);
        }

        public static void Append(string file1, string file2, string file3)
        {
            vtkAppendPolyData appendPolyData = vtkAppendPolyData.New();
            appendPolyData.AddInput(ReadPolyData(file1));
            appendPolyData.AddInput(ReadPolyData(file2));

            appendPolyData.Update();

            WritePolyData(appendPolyData.GetOutput(), file3);
        }

        public static void DicomToMhd(string dicomFolder, string mhd)
        {
            vtkDICOMImageReader dicomImageReader = vtkDICOMImageReader.New();
            dicomImageReader.SetDirectoryName(dicomFolder);
            dicomImageReader.Update();

            vtkMetaImageWriter metaImageWriter = vtkMetaImageWriter.New();
            metaImageWriter.SetInput(dicomImageReader.GetOutput());
            metaImageWriter.SetCompression(false);
            metaImageWriter.SetFileName(mhd);
            metaImageWriter.Write();
        }

        public static vtkPolyData MarchingCubeWithMhd(string mhd, string resultingVtpFile)
        {
            string exe = @"C:\testitk\MarchingCube.exe";
            string args = mhd + " " + resultingVtpFile;

            Exe(exe, args);

            return ReadPolyData(resultingVtpFile);
        }

        public static void Exe(string exe, string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(exe, args);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(startInfo).WaitForExit();
        }

        public static vtkPolyData MarchingCubeWithMhd(string mhd, string resultingVtpFile, int iso)
        {
            Exe(@"C:\testitk\MarchingCube.exe", mhd + " " + resultingVtpFile + " " + iso);
            return ReadPolyData(resultingVtpFile);
        }

        public static void Dicom2MetaImage(string dcmfolder, string metaImageFileName)
        {
            vtkImageData imageData = VTKUtil.ReadDicom(dcmfolder);

            Thread.Sleep(100);
            GC.Collect();
            Thread.Sleep(100);

            Console.WriteLine(imageData);
            int[] dim = imageData.GetDimensions();
            if (dim[0] == 1024)
            {
                imageData = VTKUtil.ResizeImage(imageData, 512, 512, Math.Min(450, dim[2]));
            }
            VTKUtil.WriteMhdNoCompress(imageData, metaImageFileName);

            imageData.Dispose();
        }

        public static vtkImageData GetBitmap(string file)
        {
            vtkBMPReader bmpreader = vtkBMPReader.New();

            bmpreader.SetFileName(file);
            bmpreader.SetDataScalarTypeToUnsignedChar();
            //bmpreader.SetDataSpacing(0.782, 0.782, 1);
            //bmpreader.SetDataSpacing(1, 1, 1);
            //bmpreader.SetDataSpacing(0.703125, 0.703125, 0.625);
            //bmpreader.SetDataExtent(0, 511, 0, 511, 0, 10);
            //bmpreader.Allow8BitBMPOn();

            bmpreader.Update();

            Console.WriteLine(bmpreader.GetOutput());

            return bmpreader.GetOutput();
        }

        public static vtkBMPReader GetBitmapReader(string folder)
        {
            vtkBMPReader bmpreader = vtkBMPReader.New();

            bmpreader.SetFilePrefix(folder);
            bmpreader.SetFilePattern("%s%03d.bmp");
            bmpreader.SetDataScalarTypeToUnsignedChar();
            //bmpreader.SetDataSpacing(0.782, 0.782, 1);
            bmpreader.SetDataSpacing(1, 1, 10);
            //bmpreader.SetDataSpacing(0.703125, 0.703125, 0.625);
            bmpreader.SetDataExtent(0, 511, 0, 511, 0, Directory.GetFiles(folder, "*.bmp").Length - 1);
            //bmpreader.SetDataExtent(0, 592, 0, 592, 0, Directory.GetFiles(folder, "*.bmp").Length - 1);
            //bmpreader.SetDataExtent(0, 511, 0, 511, 0, 10);
            //bmpreader.SetDataSpacing(0.337891, 0.337891, 0.625);
            bmpreader.SetDataSpacing(0.703125, 0.703125, 0.625);
            bmpreader.SetDataSpacing(0.97656, 0.97656, 2);
            bmpreader.SetDataSpacing(0.599609, 0.599609, 1.25);
            bmpreader.SetDataSpacing(0.703125, 0.703125, 1.25);
            bmpreader.SetDataSpacing(0.693359, 0.693359, 1);

            bmpreader.Allow8BitBMPOn();

            return bmpreader;
        }

        public static vtkBMPReader GetOrderedBitmapReader(string folder, double spacingX, double spacingY, double spacingZ)
        {
            List<string> files = new List<string>();
            int end = Directory.GetFiles(folder, "*.bmp").Length;
            for (int i = 1; i <= end; ++i)
            {
                string file = Path.Combine(folder, i + ".bmp");
                Console.WriteLine(file);
                files.Add(file);
            }

            return GetBitmapReader(files, spacingX, spacingY, spacingZ);
        }

        public static vtkBMPReader GetOrderedBitmapReader(string folder, int imageSize, double spacingX, double spacingY, double spacingZ)
        {
            List<string> files = new List<string>();
            int end = Directory.GetFiles(folder, "*.bmp").Length;
            for (int i = 1; i <= end; ++i)
            {
                string file = Path.Combine(folder, i + ".bmp");
                Console.WriteLine(file);
                files.Add(file);
            }

            return GetBitmapReader(files, imageSize, spacingX, spacingY, spacingZ);
        }

        public static vtkBMPReader GetBitmapReader(string folder, int imageSize, double spacingX, double spacingY, double spacingZ)
        {
            List<string> files = new List<string>();
            foreach (string file in Directory.GetFiles(folder, "*.bmp"))
            {
                Console.WriteLine(file);
                files.Add(file);
            }

            return GetBitmapReader(files, imageSize, spacingX, spacingY, spacingZ);
        }

        public static vtkBMPReader GetBitmapReader(List<string> files, int imageSize, double spacingX, double spacingY, double spacingZ)
        {
            vtkStringArray stringArray = vtkStringArray.New();
            foreach (string file in files)
            {
                stringArray.InsertNextValue(file);
            }

            vtkBMPReader bmpreader = vtkBMPReader.New();

            bmpreader.SetFileNames(stringArray);

            bmpreader.SetDataScalarTypeToUnsignedChar();
            //bmpreader.SetDataSpacing(0.782, 0.782, 1);
            //bmpreader.SetDataSpacing(1, 1, 10);
            //bmpreader.SetDataSpacing(0.703125, 0.703125, 0.625);
            bmpreader.SetDataExtent(0, imageSize - 1, 0, imageSize - 1, 0, files.Count - 1);
            //bmpreader.SetDataExtent(0, 511, 0, 511, 0, 10);
            bmpreader.Allow8BitBMPOn();

            return bmpreader;
        }

        public static vtkBMPReader GetBitmapReader(string folder, double spacingX, double spacingY, double spacingZ)
        {
            List<string> files = new List<string>();
            foreach (string file in Directory.GetFiles(folder, "*.bmp"))
            {
                Console.WriteLine(file);
                files.Add(file);
            }

            return GetBitmapReader(files, spacingX, spacingY, spacingZ);
        }

        public static vtkBMPReader GetBitmapReader(List<string> files, double spacingX, double spacingY, double spacingZ)
        {
            vtkStringArray stringArray = vtkStringArray.New();
            foreach (string file in files)
            {
                stringArray.InsertNextValue(file);
            }

            vtkBMPReader bmpreader = vtkBMPReader.New();

            bmpreader.SetFileNames(stringArray);

            bmpreader.SetDataScalarTypeToUnsignedChar();
            //bmpreader.SetDataSpacing(0.782, 0.782, 1);
            //bmpreader.SetDataSpacing(1, 1, 10);
            //bmpreader.SetDataSpacing(0.703125, 0.703125, 0.625);
            bmpreader.SetDataExtent(0, 511, 0, 511, 0, files.Count - 1);
            //bmpreader.SetDataExtent(0, 511, 0, 511, 0, 10);
            bmpreader.Allow8BitBMPOn();

            return bmpreader;
        }

        public static vtkDICOMImageReader GetDicomReader(string[] files)
        {
            vtkStringArray stringArray = vtkStringArray.New();
            foreach (string file in files)
            {
                stringArray.InsertNextValue(file);
            }

            vtkDICOMImageReader bmpreader = vtkDICOMImageReader.New();

            bmpreader.SetFileNames(stringArray);

            return bmpreader;
        }

        public static vtkDICOMImageReader GetDicomReader(string folder)
        {
            vtkDICOMImageReader dicomImageReader = vtkDICOMImageReader.New();
            dicomImageReader.SetDirectoryName(folder);
            dicomImageReader.Update();

            return dicomImageReader;
        }

        public static void CopyMatrix(vtkMatrix4x4 source, vtkMatrix4x4 dest)
        {
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    dest.SetElement(i, j, source.GetElement(i, j));
                }
            }
        }

        public static vtkMatrix4x4 CopyMatrix(vtkMatrix4x4 source)
        {
            vtkMatrix4x4 dest = vtkMatrix4x4.New();
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    dest.SetElement(i, j, source.GetElement(i, j));
                }
            }

            return dest;
        }

        public static List<double[]> ReadPolyDataPoints(string vtpFileName)
        {
            vtkPolyData polyData = ReadPolyData(vtpFileName);

            return ReadPolyDataPoints(polyData);
        }

        public static List<List<double[]>> ReadPolyDataList(string vtpFileName)
        {
            vtkPolyData polyData = ReadPolyData(vtpFileName);

            return ReadPolyDataList(polyData);
        }

        public static List<List<double[]>> ReadPolyDataList(vtkPolyData polyData)
        {
            List<List<double[]>> ret = new List<List<double[]>>();

            for (int cellIdx = 0; cellIdx < polyData.GetNumberOfCells(); ++cellIdx)
            {
                List<double[]> list = new List<double[]>();
                ret.Add(list);

                vtkCell cell = polyData.GetCell(cellIdx);
                for (int i = 0; i < cell.GetNumberOfPoints(); ++i)
                {
                    list.Add(cell.GetPoints().GetPoint(i));
                }
            }

            //polyData.Dispose();

            return ret;
        }

        public static List<double[]> ReadPolyDataPoints(vtkPolyData polyData)
        {
            List<double[]> ret = new List<double[]>();

            for (int i = 0; i < polyData.GetNumberOfPoints(); ++i)
            {
                ret.Add(polyData.GetPoint(i));
            }

            return ret;
        }

        public static vtkPolyData ReadSTLPolyData(string stlFileName)
        {
            vtkSTLReader reader = vtkSTLReader.New();
            reader.SetFileName(stlFileName);
            reader.Update();
            vtkPolyData output = reader.GetOutput();

            reader.Dispose();

            return output;
        }

        public static vtkPolyData CreateSphere(double[] center, double radius)
        {
            vtkSphereSource sphereSource = vtkSphereSource.New();
            sphereSource.SetCenter(center[0], center[1], center[2]);
            sphereSource.SetRadius(radius);

            sphereSource.Update();

            return sphereSource.GetOutput();
        }

        public static vtkPolyData ReadPolyData(string vtpFileName)
        {
            if (string.IsNullOrEmpty(vtpFileName))
            {
                throw new Exception("Empty filename is given.");
            }
            if (!File.Exists(vtpFileName))
            {
                throw new FileNotFoundException(string.Format("{0} not found", vtpFileName), vtpFileName);
            }

            if (vtpFileName.EndsWith(".vtp"))
            {
                vtkXMLPolyDataReader reader = vtkXMLPolyDataReader.New();
                reader.SetFileName(vtpFileName);
                reader.Update();

                vtkPolyData polyData = reader.GetOutput();
                reader.Dispose();
                return polyData;
            }
            else if (vtpFileName.EndsWith(".stl"))
            {
                return ReadSTLPolyData(vtpFileName);
            }
            else
            {
                throw new Exception(string.Format("Unknown file type = " + vtpFileName));
            }
        }

        public static vtkPolyData ReadVTKPolyData(string vtpFileName)
        {
            vtkPolyDataReader reader = vtkPolyDataReader.New();
            reader.SetFileName(vtpFileName);
            reader.Update();
            return reader.GetOutput();
        }

        public static void WritePolyData(vtkPolyData polyData, string vtpFileName)
        {
            vtkXMLPolyDataWriter writer = vtkXMLPolyDataWriter.New();
            writer.SetFileName(vtpFileName);
            writer.SetInput(polyData);

            writer.Write();

            writer.Dispose();
        }

        public static void WriteSTLData(vtkPolyData polyData, string stlFileName)
        {
            vtkSTLWriter writer = vtkSTLWriter.New();
            writer.SetFileName(stlFileName);
            writer.SetInput(polyData);

            writer.Write();
        }

        public static void WriteMhd(vtkImageData imageData, string mhdFileName)
        {
            string directory = new FileInfo(mhdFileName).Directory.FullName;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            Console.WriteLine(mhdFileName);

            vtkMetaImageWriter writer = vtkMetaImageWriter.New();
            writer.SetFileName(mhdFileName);
            writer.SetInput(imageData);
            //writer.Update();

            writer.Write();
            writer.Dispose();
        }

        public static void WriteMhdNoCompress(vtkImageData imageData, string mhdFileName)
        {
            string directory = new FileInfo(mhdFileName).Directory.FullName;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            Console.WriteLine(mhdFileName);

            vtkMetaImageWriter writer = vtkMetaImageWriter.New();
            writer.SetFileName(mhdFileName);
            writer.SetInput(imageData);
            writer.SetCompression(false);
            //writer.Update();

            //writer.Update();
            writer.Write();

            writer.Dispose();
            writer = null;
        }

        public static void DisposeVtkObject(vtkObject @object)
        {
            if (@object != null)
            {
                @object.Dispose();
            }
        }

        public static void ClipMhdWithImplicitFunction(string mhdFile, vtkImplicitFunction implicitFunction, string outputMhd)
        {
            vtkMetaImageReader metaImageReader = vtkMetaImageReader.New();
            metaImageReader.SetFileName(mhdFile);
            metaImageReader.Update();

            vtkImageData imageData = metaImageReader.GetOutput();

            ClipMhdWithImplicitFunction(imageData, implicitFunction, outputMhd);
        }

        public static void ClipMhdWithImplicitFunction(vtkImageData imageData, vtkImplicitFunction implicitFunction, string outputMhd)
        {
            vtkImplicitFunctionToImageStencil implicitFunctionToImageStencil = vtkImplicitFunctionToImageStencil.New();
            implicitFunctionToImageStencil.SetInput(implicitFunction);
            implicitFunctionToImageStencil.SetInformationInput(imageData);
            implicitFunctionToImageStencil.Update();

            vtkImageStencil imageStencil = vtkImageStencil.New();
            imageStencil.SetInput(imageData);
            imageStencil.SetStencil(implicitFunctionToImageStencil.GetOutput());
            imageStencil.Update();

            vtkImageData outputImageData = imageStencil.GetOutput();

            WriteMhd(outputImageData, outputMhd);
        }

        public static void ClipMhdWithImplicitFunction(string mhdFile, vtkImplicitFunction implicitFunction, string clippedoutputMhd, string unclippedoutputMhd)
        {
            vtkMetaImageReader metaImageReader = vtkMetaImageReader.New();
            metaImageReader.SetFileName(mhdFile);
            metaImageReader.Update();

            vtkImageData imageData = metaImageReader.GetOutput();

            ClipMhdWithImplicitFunction(imageData, implicitFunction, clippedoutputMhd, unclippedoutputMhd);
        }

        public static void ClipMhdWithImplicitFunction(vtkImageData imageData, vtkImplicitFunction implicitFunction, string clippedoutputMhd, string unclippedoutputMhd)
        {
            vtkImplicitFunctionToImageStencil implicitFunctionToImageStencil = vtkImplicitFunctionToImageStencil.New();
            implicitFunctionToImageStencil.SetInput(implicitFunction);
            implicitFunctionToImageStencil.SetInformationInput(imageData);

            vtkImageStencil imageStencil = vtkImageStencil.New();
            imageStencil.SetInput(imageData);
            imageStencil.SetStencil(implicitFunctionToImageStencil.GetOutput());

            vtkImageData outputImageData = imageStencil.GetOutput();

            WriteMhd(outputImageData, clippedoutputMhd);

            vtkImplicitBoolean implicitBoolean = new vtkImplicitBoolean();
            implicitBoolean.SetOperationTypeToDifference();
            implicitBoolean.AddFunction(implicitFunction);

            vtkImplicitFunctionToImageStencil implicitFunctionToImageStencilR = new vtkImplicitFunctionToImageStencil();
            implicitFunctionToImageStencilR.SetInput(implicitBoolean);
            implicitFunctionToImageStencilR.SetInformationInput(imageData);

            vtkImageStencil imageStencilR = new vtkImageStencil();
            imageStencilR.SetInput(imageData);
            imageStencilR.SetStencil(implicitFunctionToImageStencilR.GetOutput());
            imageStencilR.ReverseStencilOn();

            vtkImageData outputImageDataR = imageStencilR.GetOutput();

            WriteMhd(outputImageDataR, unclippedoutputMhd);
        }

        public static void ClipDicomWithImplicitFunction(string dicomFolder, vtkImplicitFunction implicitFunction, string outputMhd)
        {
            vtkDICOMImageReader dicomImageReader = new vtkDICOMImageReader();
            dicomImageReader.SetDirectoryName(dicomFolder);
            dicomImageReader.Update();

            vtkImageData imageData = dicomImageReader.GetOutput();

            vtkImplicitFunctionToImageStencil implicitFunctionToImageStencil = new vtkImplicitFunctionToImageStencil();
            implicitFunctionToImageStencil.SetInput(implicitFunction);
            implicitFunctionToImageStencil.SetInformationInput(imageData);

            vtkImageStencil imageStencil = new vtkImageStencil();
            imageStencil.SetInput(imageData);
            imageStencil.SetStencil(implicitFunctionToImageStencil.GetOutput());
            imageStencil.SetBackgroundValue(-1500);
            //imageStencil.ReverseStencilOn();

            vtkImageData outputImageData = imageStencil.GetOutput();

            WriteMhd(outputImageData, outputMhd);
        }

        public static double[] Negative(double[] value)
        {
            return new double[] { -value[0], -value[1], -value[2] };
        }

        public static PointF Negative(PointF point)
        {
            return new PointF(-point.X, -point.Y);
        }

        public static void ScreenShot(string filename, vtkRenderWindow renderWindow)
        {
            vtkWindowToImageFilter filter = new vtkWindowToImageFilter();
            filter.SetInput(renderWindow);
            filter.Update();

            vtkBMPWriter writer = new vtkBMPWriter();
            writer.SetFileName(filename);
            writer.SetInput(filter.GetOutput());
            writer.Write();
        }

        public static vtkImageData ReadDicom(string dicomFolder)
        {
            vtkDICOMImageReader metaImageReader = new vtkDICOMImageReader();
            metaImageReader.SetDirectoryName(dicomFolder);
            metaImageReader.Update();

            vtkImageData imageData = metaImageReader.GetOutput();

            metaImageReader.Dispose();

            return imageData;
        }


        public static vtkImageData ReadFirstDicom(string dicomFolder)
        {
            vtkDICOMImageReader metaImageReader = new vtkDICOMImageReader();
            metaImageReader.SetFileName(Directory.GetFiles(dicomFolder)[0]);
            metaImageReader.Update();

            return metaImageReader.GetOutput();
        }

        public static vtkImageData ReadMhd(string mhdFile)
        {
            if (!File.Exists(mhdFile))
            {
                throw new FileNotFoundException(string.Format("{0} is not found.", mhdFile));
            }
            vtkMetaImageReader metaImageReader = vtkMetaImageReader.New();
            metaImageReader.SetFileName(mhdFile);
            metaImageReader.Update();

            vtkImageData ret = metaImageReader.GetOutput();

            metaImageReader.Dispose();
            metaImageReader = null;

            return ret;
        }

        public static void TransformMhdFile(string inputMhdFile, string outputMhdFile, vtkMatrix4x4 matrix)
        {
            vtkTransform transform = new vtkTransform();
            transform.SetMatrix(matrix);

            vtkImageReslice imageResample = new vtkImageReslice();
            imageResample.SetResliceTransform(transform);
            imageResample.SetInput(ReadMhd(inputMhdFile));
            imageResample.SetBackgroundLevel(-1500);
            imageResample.Update();

            vtkImageData vtkImageData = imageResample.GetOutput();

            WriteMhd(vtkImageData, outputMhdFile);
        }

        public static void TransformMhdFile(string inputMhdFile, string outputMhdFile, vtkTransform transform)
        {
            TransformMhdFile(inputMhdFile, outputMhdFile, transform.GetMatrix());
        }

        public static void XRayMhdZOrientationEnum(string xrayFile, string mhdFile)
        {
            vtkImageData imageData = ReadMhd(mhdFile);

            imageData = ResliceToUnit1(imageData);

            vtkImageCast imageCast = new vtkImageCast();
            imageCast.SetInput(imageData);
            imageCast.SetOutputScalarTypeToUnsignedChar();
            imageCast.ClampOverflowOn();
            imageCast.Update();

            imageData = imageCast.GetOutput();

            int[] extent = imageData.GetExtent();

            Bitmap bmp = new Bitmap(extent[1], extent[3]);

            byte[] vv = new byte[1];
            vtkDataArray vtkDataArray = imageData.GetPointData().GetScalars();

            //Console.WriteLine(imageData.GetScalarComponentAsFloat(46, 255, 191, 0));
            //IntPtr abc1 = imageData.GetArrayPointer(vtkDataArray, new int[] { 46, 255, 191});
            //Marshal.Copy(abc1, vv, 0, vv.Length);
            //Console.WriteLine((int)vv[0]);

            //return;

            for (int x = 0; x < extent[1]; ++x)
            {
                Console.WriteLine(x);
                for (int y = 0; y < extent[3]; ++y)
                {
                    float sum = 0;
                    int sliceNumber = extent[5];

                    for (int z = 0; z < sliceNumber; ++z)
                    {
                        //float value1 = imageData.GetScalarComponentAsFloat(x, y, z, 0);
                        //sum += imageData.GetScalarComponentAsFloat(x, y, z, 0);

                        //continue;

                        //Console.WriteLine(imageData.GetScalarComponentAsFloat(x, y, z, 0));
                        IntPtr abc = imageData.GetArrayPointer(vtkDataArray, VTKUtil.ConvertIntPtr(x, y, z));
                        Marshal.Copy(abc, vv, 0, vv.Length);

                        //if((int)value1!=(int)vv[0])
                        //{
                        //    Console.WriteLine("WERWER");
                        //}
                        //if (single != imageData.GetScalarComponentAsFloat(x, y, z, 0))
                        //{
                        //    Console.WriteLine(x + y + z);
                        //}
                        sum += vv[0];

                        //if (single != 0) Console.WriteLine((int)single);
                    }

                    float value = sum / sliceNumber;

                    //try
                    //{
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb((int)value, (int)value, (int)value));
                    //}
                    //catch
                    //{

                    //}
                }
            }

            DrawAxis("x", "y", bmp);

            bmp.Save(xrayFile);
            //vtkBMPWriter bmpWriter = new vtkBMPWriter();
            //bmpWriter.SetFileName(xrayFile);
            //bmpWriter.SetInput(xray);

            //bmpWriter.Write();
        }

        public static void DrawAxis(string axisX, string axisY, Bitmap bmp)
        {
            Graphics.FromImage(bmp).DrawString(string.Format("{0}/{1}", axisX, axisY), new Font("Arial", 10), Brushes.Red, 0, bmp.Height - 20);
        }

        private static vtkImageData ResliceToUnit1(vtkImageData imageData)
        {
            vtkImageResample imageResample = new vtkImageResample();
            imageResample.SetInput(imageData);

            imageResample.SetAxisOutputSpacing(0, 1);
            imageResample.SetAxisOutputSpacing(1, 1);
            imageResample.SetAxisOutputSpacing(2, 1);

            imageResample.Update();

            imageData = imageResample.GetOutput();
            return imageData;
        }

        public static vtkImageData Test(vtkImageData imageData, vtkMatrix4x4 matrix)
        {
            vtkTransform transform = new vtkTransform();
            transform.SetMatrix(matrix);

            //vtk.vtkImageReslice reslice = new vtkImageReslice();
            vtkImageResample imageResample = new vtkImageResample();
            imageResample.SetInput(imageData);

            //imageResample.SetAxisOutputSpacing(1, 1);
            //imageResample.SetAxisOutputSpacing(2, 1);

            imageResample.SetResliceTransform(transform);

            imageResample.Update();

            imageData = imageResample.GetOutput();
            return imageData;
        }

        public static void XRayMhdZOrientationEnum(string xrayFile, string mhdFile, int startZ, int endZ)
        {
            vtkImageData imageData = ReadMhd(mhdFile);

            vtkImageResample imageResample = new vtkImageResample();
            imageResample.SetInput(imageData);

            imageResample.SetAxisOutputSpacing(0, 1);
            imageResample.SetAxisOutputSpacing(1, 1);
            imageResample.SetAxisOutputSpacing(2, 1);

            imageResample.Update();

            imageData = imageResample.GetOutput();

            vtkImageCast imageCast = new vtkImageCast();
            imageCast.SetInput(imageData);
            imageCast.SetOutputScalarTypeToUnsignedChar();
            imageCast.ClampOverflowOn();
            imageCast.Update();

            imageData = imageCast.GetOutput();

            int[] extent = imageData.GetExtent();

            Bitmap bmp = new Bitmap(extent[1], extent[3]);

            byte[] vv = new byte[1];
            vtkDataArray vtkDataArray = imageData.GetPointData().GetScalars();

            for (int x = 0; x < extent[1]; ++x)
            {
                Console.WriteLine(x);
                for (int y = 0; y < extent[3]; ++y)
                {
                    float sum = 0;
                    int sliceNumber = endZ - startZ + 1;

                    for (int z = startZ; z <= endZ; ++z)
                    {
                        IntPtr abc = imageData.GetArrayPointer(vtkDataArray, VTKUtil.ConvertIntPtr(x, y, z));
                        Marshal.Copy(abc, vv, 0, vv.Length);

                        sum += vv[0];
                    }

                    float value = sum / sliceNumber;

                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb((int)value, (int)value, (int)value));
                }
            }

            bmp.Save(xrayFile);
        }

        /// <summary>
        /// 侧位
        /// </summary>
        /// <param name="xrayFile"></param>
        /// <param name="mhdFile"></param>
        public static void XRayMhdXOrientationEnum(string xrayFile, string mhdFile)
        {
            vtkImageData imageData = ReadMhd(mhdFile);

            vtkImageCast imageCast = new vtkImageCast();
            imageCast.SetInput(imageData);
            imageCast.SetOutputScalarTypeToUnsignedChar();
            imageCast.ClampOverflowOn();
            imageCast.Update();

            imageData = imageCast.GetOutput();

            vtkImageResample imageResample = new vtkImageResample();
            imageResample.SetInput(imageData);

            imageResample.SetAxisOutputSpacing(0, 1);
            imageResample.SetAxisOutputSpacing(1, 1);
            imageResample.SetAxisOutputSpacing(2, 1);

            imageResample.Update();

            imageData = imageResample.GetOutput();

            int[] extent = imageData.GetExtent();

            vtkImageData xray = new vtkImageData();
            xray.SetExtent(0, extent[5], 0, extent[3], 0, 1);
            xray.SetSpacing(1, 1, 1);
            xray.SetScalarTypeToChar();

            Bitmap bmp = new Bitmap(extent[1], extent[3]);

            for (int x = 0; x < extent[5]; ++x)
            {
                Console.WriteLine(x);
                for (int y = 0; y < extent[3]; ++y)
                {
                    float sum = 0;
                    int sliceNumber = extent[1];

                    for (int z = 0; z < sliceNumber; ++z)
                    {
                        sum += imageData.GetScalarComponentAsFloat(z, y, x, 0);
                        //sum += FastImageDataFetcher.GetScalar(z, y, x);
                    }

                    float value = sum / sliceNumber;

                    //xray.SetScalarComponentFromFloat(x, y, 0, 0, value);
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb((int)value, (int)value, (int)value));
                }
            }

            bmp.Save(xrayFile);
            //vtkBMPWriter bmpWriter = new vtkBMPWriter();
            //bmpWriter.SetFileName(xrayFile);
            //bmpWriter.SetInput(xray);

            //bmpWriter.Write();
        }


        /// <summary>
        /// 侧位
        /// </summary>
        /// <param name="xrayFile"></param>
        /// <param name="mhdFile"></param>
        public static void XRayMhdXOrientationEnum(string xrayFile, string mhdFile, int startX, int endX)
        {
            vtkImageData imageData = ReadMhd(mhdFile);

            vtkImageCast imageCast = new vtkImageCast();
            imageCast.SetInput(imageData);
            imageCast.SetOutputScalarTypeToUnsignedChar();
            imageCast.ClampOverflowOn();
            imageCast.Update();

            imageData = imageCast.GetOutput();

            vtkImageResample imageResample = new vtkImageResample();
            imageResample.SetInput(imageData);

            imageResample.SetAxisOutputSpacing(0, 1);
            imageResample.SetAxisOutputSpacing(1, 1);
            imageResample.SetAxisOutputSpacing(2, 1);

            imageResample.Update();

            imageData = imageResample.GetOutput();

            int[] extent = imageData.GetExtent();

            vtkImageData xray = new vtkImageData();
            xray.SetExtent(0, extent[5], 0, extent[3], 0, 1);
            xray.SetSpacing(1, 1, 1);
            xray.SetScalarTypeToChar();

            Bitmap bmp = new Bitmap(extent[1], extent[3]);

            for (int x = 0; x < extent[5]; ++x)
            {
                Console.WriteLine(x);
                for (int y = 0; y < extent[3]; ++y)
                {
                    float sum = 0;
                    int sliceNumber = endX - startX + 1;

                    for (int z = startX; z <= endX; ++z)
                    {
                        sum += imageData.GetScalarComponentAsFloat(z, y, x, 0);
                    }

                    float value = sum / sliceNumber;

                    xray.SetScalarComponentFromFloat(x, y, 0, 0, value);
                    bmp.SetPixel(y, x, System.Drawing.Color.FromArgb((int)value, (int)value, (int)value));
                }
            }

            bmp.Save(xrayFile);
        }

        public static void XRayMhdYOrientationEnum(string xrayFile, string mhdFile)
        {
            vtkImageData imageData = ReadMhd(mhdFile);

            XRay(imageData, xrayFile);
        }

        public static void XRayMhdYOrientationEnumDicomFolder(string xrayFile, string dicomFolder)
        {
            vtkImageData imageData = ReadDicom(dicomFolder);

            XRay(imageData, xrayFile);
        }

        public static void XRayMhdYOrientationEnumBitmapFolder(string xrayFile, string dicomFolder)
        {
            vtkImageData imageData = ReadDicom(dicomFolder);

            XRay(imageData, xrayFile);
        }

        private static void XRay(vtkImageData imageData, string xrayFile)
        {
            //vtkImageThreshold threshold = new vtkImageThreshold();
            //threshold.SetInput(imageData);
            //threshold.ThresholdBetween(-100, 1600);
            //threshold.SetOutputScalarTypeToUnsignedShort();
            //threshold.SetReplaceIn(0);
            //threshold.SetReplaceOut(0);
            //threshold.ReplaceInOn();
            //threshold.ReplaceOutOn();

            //imageData = threshold.GetOutput();

            //            vtkImageMapToWindowLevelColors m_WLFilter = new vtkImageMapToWindowLevelColors(); 
            //m_WLFilter.SetOutputFormatToLuminance(); 
            //m_WLFilter.SetInput( imageData); 
            //m_WLFilter.SetWindow(1000.0); 
            //m_WLFilter.SetWindow(400.0); 
            //m_WLFilter.UpdateWholeExtent();

            //VTKUtil.WriteMhd(m_WLFilter.GetOutput(), "e:/test/xray.mhd");

            //    return;

            vtkImageCast imageCast = new vtkImageCast();
            imageCast.SetInput(imageData);
            imageCast.SetOutputScalarTypeToUnsignedChar();
            imageCast.ClampOverflowOn();
            imageCast.Update();

            imageData = imageCast.GetOutput();


            //return;

            vtkImageResample imageResample = new vtkImageResample();
            imageResample.SetInput(imageData);

            imageResample.SetAxisOutputSpacing(0, 1);
            imageResample.SetAxisOutputSpacing(1, 1);
            imageResample.SetAxisOutputSpacing(2, 1);

            imageResample.Update();

            imageData = imageResample.GetOutput();

            int[] extent = imageData.GetExtent();

            vtkImageData xray = new vtkImageData();
            xray.SetExtent(0, extent[1], 0, extent[5], 0, 1);
            xray.SetSpacing(1, 1, 1);
            xray.SetScalarTypeToChar();

            Bitmap bmp = new Bitmap(extent[1], extent[5]);

            for (int x = 0; x < extent[1]; ++x)
            {
                Console.WriteLine(x);
                for (int y = 0; y < extent[5]; ++y)
                {
                    float sum = 0;
                    int sliceNumber = extent[3];

                    for (int z = 0; z < sliceNumber; ++z)
                    {
                        float scalarComponentAsFloat = imageData.GetScalarComponentAsFloat(x, z, y, 0);
                        sum += scalarComponentAsFloat;
                    }

                    float value = sum / sliceNumber;

                    xray.SetScalarComponentFromFloat(x, y, 0, 0, value);
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb((int)value, (int)value, (int)value));
                }
            }

            bmp.Save(xrayFile);
        }

        public static List<double[]> Multiply(List<double[]> source, vtkMatrix4x4 matrix4X4)
        {
            List<double[]> ret = new List<double[]>();

            foreach (double[] value in source)
            {
                double[] multiplyDoublePoint = matrix4X4.MultiplyDoublePoint(VTKUtil.ConvertTo4P(new double[] { value[0], value[1], value[2] }));
                ret.Add(new double[] { multiplyDoublePoint[0], multiplyDoublePoint[1], multiplyDoublePoint[2] });
            }

            return ret;
        }

        public static vtkMatrix4x4 除(vtkMatrix4x4 m1, vtkMatrix4x4 m2)
        {
            vtkMatrix4x4 copy = new vtkMatrix4x4();
            CopyMatrix(m2, copy);
            copy.Invert();

            vtkMatrix4x4 ret = new vtkMatrix4x4();
            vtkMatrix4x4.Multiply4x4(m1, copy, ret);

            return ret;
        }

        /// <summary>
        /// 左乘
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static vtkMatrix4x4 乘(vtkMatrix4x4 m1, vtkMatrix4x4 m2)
        {
            vtkMatrix4x4 copy = new vtkMatrix4x4();
            CopyMatrix(m2, copy);

            vtkMatrix4x4 ret = new vtkMatrix4x4();
            vtkMatrix4x4.Multiply4x4(copy, m1, ret);

            return ret;
        }

        public static vtkMatrix4x4 右乘(vtkMatrix4x4 m1, vtkMatrix4x4 m2)
        {
            vtkMatrix4x4 copy = new vtkMatrix4x4();
            CopyMatrix(m2, copy);

            vtkMatrix4x4 ret = new vtkMatrix4x4();
            vtkMatrix4x4.Multiply4x4(m1, copy, ret);

            return ret;
        }

        public static void Clip(vtkPlane[] clippingPlanes, double[] positionOnObject, string inputMhd, string clippedoutputMhd)
        {
            vtkImplicitFunction collection = PlanesToImplicitFunction(clippingPlanes);

            ClipMhdWithImplicitFunction(inputMhd, collection, clippedoutputMhd);
        }

        public static vtkImplicitFunction PlanesToImplicitFunction(vtkPlane[] clippingPlanes)
        {
            vtkImplicitBoolean collection = new vtkImplicitBoolean();
            collection.SetOperationTypeToUnion();
            foreach (vtkPlane clippingPlane in clippingPlanes)
            {
                collection.AddFunction(clippingPlane);
            }
            return collection;
        }

        public static void UnClip(vtkPlane[] clippingPlanes, double[] positionOnObject, string inputMhd, string unclippedoutputMhd)
        {
            vtkImplicitBoolean collection = new vtkImplicitBoolean();
            collection.SetOperationTypeToIntersection();
            foreach (vtkPlane clippingPlane in clippingPlanes)
            {
                clippingPlane.SetNormal(
                    -clippingPlane.GetNormal()[0],
                    -clippingPlane.GetNormal()[1],
                    -clippingPlane.GetNormal()[2]);

                collection.AddFunction(clippingPlane);
            }

            ClipMhdWithImplicitFunction(inputMhd, collection, unclippedoutputMhd);
        }

        public static void TransformAndXRay(string mhd1, string mhd2, vtkTransform transform2, string xrayFile)
        {
            throw new NotImplementedException();
        }

        public static void TransformAndMerge(string mhd1, string mhd2, vtkTransform transform2, string mergedMhd)
        {
            TransformMhdFile(mhd1, mergedMhd, transform2);
            MergeShortMhd(mhd1, mergedMhd, mergedMhd);
        }

        public static short[, ,] ReadShortRaw(string mhdFile)
        {
            byte[] content = File.ReadAllBytes(mhdFile);

            short[, ,] ret = new short[512, 512, 254];

            int idx = 0;
            for (int i = 0; i < 512; ++i)
            {
                for (int j = 0; j < 512; ++j)
                {
                    for (int k = 0; k < 254; ++k)
                    {
                        ret[i, j, k] = BitConverter.ToInt16(content, idx);
                        idx += sizeof(short);
                    }
                }
            }

            return ret;
        }

        public static byte[, ,] ReadCharRaw(string mhdFile, int dim1, int dim2, int dim3)
        {
            byte[] content = File.ReadAllBytes(mhdFile);

            byte[, ,] ret = new byte[dim1, dim2, dim3];

            int idx = 0;
            for (int k = 0; k < dim3; ++k)
            {
                for (int i = 0; i < dim1; ++i)
                {
                    for (int j = 0; j < dim2; ++j)
                    {
                        //bmp.SetPixel(i, j, Color.FromArgb(
                        //    content[idx],
                        //    content[idx],
                        //    content[idx]));

                        //idx += sizeof(byte);
                        //continue;

                        ret[i, j, k] = content[idx];

                        idx += sizeof(byte);
                    }
                }

            }

            return ret;
        }

        public static byte[, ,] ReadCharRaw(string mhdFile)
        {
            vtkMetaImageReader reader = new vtkMetaImageReader();
            reader.SetFileName(mhdFile);
            reader.UpdateInformation();
            int[] extent = reader.GetDataExtent();
            reader.Dispose();
            reader = null;
            return ReadCharRaw(mhdFile.Replace(".mhd", ".raw"), extent[1] + 1, extent[3] + 1, extent[5] + 1);
        }

        public static vtkImageData MergeImageDataAlongZ(params string[] imageDataFileNames)
        {
            List<vtkImageData> imageDataList = new List<vtkImageData>();
            foreach (string imageDataFileName in imageDataFileNames)
            {
                imageDataList.Add(VTKUtil.ReadMetaImage(imageDataFileName));
            }

            vtkImageData ret = MergeImageDataAlongZ(imageDataList.ToArray());
            foreach (vtkImageData imageData in imageDataList)
            {
                imageData.Dispose();
            }

            return ret;
        }

        public static vtkImageData MergeImageDataAlongZ(params vtkImageData[] imageDataList)
        {
            int dimension0 = imageDataList[0].GetDimensions()[0];
            int dimension1 = imageDataList[0].GetDimensions()[1];
            int dimension2 = 0;

            foreach (vtkImageData imageData in imageDataList)
            {
                dimension2 += imageData.GetDimensions()[2];
            }

            vtkImageData ret = vtkImageData.New();
            ret.SetScalarType(imageDataList[0].GetScalarType());
            ret.SetDimensions(dimension0, dimension1, dimension2);
            ret.AllocateScalars();

            int z = 0;
            foreach (vtkImageData imageData in imageDataList)
            {
                for (int k = 0; k < imageData.GetDimensions()[2]; ++k)
                {
                    Console.WriteLine(@"(k + z) = " + (k + z));

                    for (int j = 0; j < dimension1; ++j)
                    {
                        for (int i = 0; i < dimension0; ++i)
                        {
                            ret.SetScalarComponentFromFloat(i, j, k + z, 0, imageData.GetScalarComponentAsFloat(i, j, k, 0));
                        }
                    }
                }
                z += imageData.GetDimensions()[2];
            }

            return ret;
        }

        public static void MergeCharMhd(string mhdFile1, string mhdFile2, string mhdMerge)
        {
            byte[, ,] mhd1 = ReadCharRaw(mhdFile1, 512, 512, 254);
            byte[, ,] mhd2 = ReadCharRaw(mhdFile2, 512, 512, 254);

            int dim1Len = mhd1.GetLength(0);
            int dim2Len = mhd1.GetLength(1);
            int dim3Len = mhd1.GetLength(2);

            byte[, ,] mhdMerged = new byte[dim1Len, dim2Len, dim3Len];
            List<byte> content = new List<byte>();

            for (int k = 0; k < dim3Len; ++k)
            {
                for (int i = 0; i < dim1Len; ++i)
                {
                    for (int j = 0; j < dim2Len; ++j)
                    {
                        byte value1 = mhd1[i, j, k];
                        byte value2 = mhd2[i, j, k];

                        byte valueMerged = (byte)Math.Min(value1 + value2, Byte.MaxValue);
                        mhdMerged[i, j, k] = valueMerged;

                        content.Add(valueMerged);
                    }
                }
            }

            File.WriteAllBytes(mhdMerge, content.ToArray());
        }

        public static void SliceX(string mhdFile, string folder)
        {
            vtkImageData imageData = ReadMhd(mhdFile);

            vtkImageCast imageCast = new vtkImageCast();
            imageCast.SetOutputScalarTypeToUnsignedChar();
            imageCast.SetInput(imageData);
            imageCast.ClampOverflowOn();

            imageData = imageCast.GetOutput();

            //vtkImageMapToWindowLevelColors ImageMapToWindowLevelColors = new vtkImageMapToWindowLevelColors();
            //ImageMapToWindowLevelColors.SetInput(imageData);
            //ImageMapToWindowLevelColors.SetWindow(1350);
            //ImageMapToWindowLevelColors.SetLevel(-380);
            //ImageMapToWindowLevelColors.Update();
            //imageData = ImageMapToWindowLevelColors.GetOutput();

            //imageData = ResliceToUnit1(imageData);

            // 重新生成BMP
            vtkBMPWriter bmpWriter = new vtkBMPWriter();
            bmpWriter.SetInput(imageData);
            bmpWriter.SetFilePrefix(folder);
            bmpWriter.SetFilePattern("%s%05d.bmp");
            bmpWriter.Write();

            Console.WriteLine("Resampled后的Bitmap保存完毕");
        }

        public static vtkImageData WLWW(vtkImageData imageData, double window, double level)
        {
            vtkImageMapToWindowLevelColors ImageMapToWindowLevelColors = new vtkImageMapToWindowLevelColors();
            ImageMapToWindowLevelColors.SetInput(imageData);
            ImageMapToWindowLevelColors.SetWindow(window);
            ImageMapToWindowLevelColors.SetLevel(level);
            ImageMapToWindowLevelColors.Update();

            return ImageMapToWindowLevelColors.GetOutput();
        }


        public static vtkImageData WLWW_M_Slow(vtkImageData imageData, double window, double level)
        {
            int[] extent = imageData.GetExtent();

            vtkImageData xray = new vtkImageData();
            xray.SetExtent(0, extent[1], 0, extent[3], 0, extent[5]);
            xray.SetSpacing(VTKUtil.ConvertIntPtr(imageData.GetSpacing()));
            xray.SetScalarTypeToUnsignedChar();
            xray.AllocateScalars();

            for (int x = 0; x < extent[1]; ++x)
            {
                Console.WriteLine(x);
                for (int y = 0; y < extent[3]; ++y)
                {
                    for (int z = 0; z < extent[5]; ++z)
                    {
                        short value = (short)imageData.GetScalarComponentAsDouble(x, y, z, 0);
                        byte newValue = Short2UChar((int)level, (int)window, value);
                        //if (newValue != 0)
                        {
                            xray.SetScalarComponentFromDouble(x, y, z, 0, newValue);
                        }
                    }
                }
            }

            return xray;
        }

        public static vtkImageCast CastToUC(vtkImageData imageData)
        {
            vtkImageCast imageCast = new vtkImageCast();
            imageCast.ClampOverflowOn();
            imageCast.SetOutputScalarTypeToUnsignedChar();
            imageCast.SetInput(imageData);
            return imageCast;
        }

        public static vtkImageData CastToShort(vtkImageData imageData)
        {
            vtkImageCast imageCast = new vtkImageCast();
            imageCast.ClampOverflowOn();
            imageCast.SetOutputScalarTypeToShort();
            imageCast.SetInput(imageData);
            imageCast.Update();
            return imageCast.GetOutput();
        }

        public static vtkImageData CastToFloat(vtkImageData imageData)
        {
            vtkImageCast imageCast = vtkImageCast.New();
            imageCast.ClampOverflowOn();
            imageCast.SetOutputScalarTypeToFloat();
            imageCast.SetInput(imageData);
            imageCast.Update();
            return imageCast.GetOutput();
        }

        public static void SliceXYZ(string mhdFile, string folder)
        {
            vtkImageData imageData = ReadMhd(mhdFile);

            vtkImageCast imageCast = new vtkImageCast();
            imageCast.SetOutputScalarTypeToUnsignedChar();
            imageCast.SetInput(imageData);
            imageCast.ClampOverflowOn();

            imageData = imageCast.GetOutput();

            //vtkImageMapToWindowLevelColors ImageMapToWindowLevelColors = new vtkImageMapToWindowLevelColors();
            //ImageMapToWindowLevelColors.SetInput(imageData);
            //ImageMapToWindowLevelColors.SetWindow(1350);
            //ImageMapToWindowLevelColors.SetLevel(-380);
            //ImageMapToWindowLevelColors.Update();
            //imageData = ImageMapToWindowLevelColors.GetOutput();

            imageData = ResliceToUnit1(imageData);

            // 重新生成BMP
            vtkBMPWriter bmpWriter = new vtkBMPWriter();
            bmpWriter.SetInput(imageData);
            bmpWriter.SetFilePrefix(folder);
            bmpWriter.SetFilePattern("%s%05d.bmp");
            bmpWriter.Write();

            Console.WriteLine("Resampled后的Bitmap保存完毕");

            //return;

            int size = 400;
            size = imageData.GetExtent()[1] + 1;

            for (int i = 0; i < size; ++i)
            {
                //string zfolder = Path.Combine(folder, "Z");
                //if (!Directory.Exists(zfolder))
                //{
                //    Directory.CreateDirectory(zfolder);
                //}

                string file = Path.Combine(folder, i.ToString().PadLeft(5, '0') + ".bmp");
                if (!File.Exists(file))
                {
                    Bitmap bmp = new Bitmap(size, size);
                    ImageHelper.ConvertTo8BitBitmap(bmp, file);
                }
            }
            Console.WriteLine("补齐512张Bitmap文件");

            byte[, ,] data = new byte[size, size, size];

            for (int i = 0; i < size; ++i)
            {
                string file = Path.Combine(folder, i.ToString().PadLeft(5, '0') + ".bmp");
                Console.WriteLine(file);
                UnsafeBitmap bmp = new UnsafeBitmap(new Bitmap(file));
                bmp.LockBitmap();

                for (int x = 0; x < bmp.Bitmap.Width; ++x)
                {
                    for (int y = 0; y < bmp.Bitmap.Height; ++y)
                    {
                        data[i, x, y] = bmp.GetPixel(x, y).red;
                    }
                }

                bmp.UnlockBitmap();
            }

            Thread t1 = new Thread(new ThreadStart(delegate
            {
                // Reslice : Z轴
                for (int i = 0; i < size; ++i)
                {
                    string file = Path.Combine(folder, i.ToString().PadLeft(5, '0') + ".bmp");
                    string zfolder = Path.Combine(folder, "Z");
                    string zfile = Path.Combine(zfolder,
                                                i.ToString().PadLeft(5, '0') + ".bmp");
                    if (!Directory.Exists(zfolder))
                    {
                        Directory.CreateDirectory(zfolder);
                    }

                    ImageHelper.ConvertTo8BitBitmap(new Bitmap(file), zfile);
                }
                Console.WriteLine("Reslice : Z轴");
            }));
            t1.Start();

            // Reslice : X轴);
            Thread t2 = new Thread(new ThreadStart(delegate
            {

                for (int i = 0; i < size; ++i)
                {
                    string xfolder = Path.Combine(folder, "X");
                    if (!Directory.Exists(xfolder))
                    {
                        Directory.CreateDirectory(xfolder);
                    }

                    string file = Path.Combine(xfolder, i.ToString().PadLeft(5, '0') + ".bmp");
                    Console.WriteLine(file);
                    UnsafeBitmap bmp = new UnsafeBitmap(size, size);
                    bmp.LockBitmap();

                    for (int x = 0; x < size; ++x)
                    {
                        for (int y = 0; y < size; ++y)
                        {
                            PixelData p = new PixelData();
                            p.red = p.green = p.blue = (byte)data[x, i, y];
                            bmp.SetPixel(y, x, p);
                        }
                    }

                    bmp.UnlockBitmap();

                    ImageHelper.ConvertTo8BitBitmap(bmp.Bitmap, file);
                }
                Console.WriteLine("Reslice : X轴");

            }));
            t2.Start();

            //return;

            // Reslice : Y轴
            Thread t3 = new Thread(new ThreadStart(delegate
            {

                for (int i = 0; i < size; ++i)
                {
                    string xfolder = Path.Combine(folder, "Y");
                    if (!Directory.Exists(xfolder))
                    {
                        Directory.CreateDirectory(xfolder);
                    }

                    string file = Path.Combine(xfolder, i.ToString().PadLeft(5, '0') + ".bmp");
                    Console.WriteLine(file);
                    UnsafeBitmap bmp = new UnsafeBitmap(size, size);
                    bmp.LockBitmap();

                    for (int x = 0; x < size; ++x)
                    {
                        for (int y = 0; y < size; ++y)
                        {
                            PixelData p = new PixelData();
                            p.red = p.green = p.blue = (byte)data[x, y, i];
                            bmp.SetPixel(y, x, p);
                        }
                    }

                    bmp.UnlockBitmap();

                    ImageHelper.ConvertTo8BitBitmap(bmp.Bitmap, file);
                }
                Console.WriteLine("Reslice : Y轴");
            }));
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();
        }

        public static Bitmap GetBitmapZOrientationEnum(byte[, ,] content, int sliceZ)
        {
            int dim1Len = content.GetLength(0);
            int dim2Len = content.GetLength(1);
            int dim3Len = content.GetLength(2);

            Bitmap bmp = new Bitmap(dim1Len, dim2Len);

            //int startIdx = dim1Len * dim2Len * sliceZ;

            for (int x = 0; x < dim1Len; ++x)
            {
                for (int y = 0; y < dim2Len; ++y)
                {
                    byte value = content[x, y, sliceZ];
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(value, value, value));
                }
            }

            return bmp;
        }

        public static vtkImageData ResizeImage(vtkImageData input, double resizedWidth, double resizedHeight)
        {
            vtkImageReslice imageResample = new vtkImageReslice();
            imageResample.SetInput(input);
            //imageResample.SetOutputExtent(0, 511, 0, 511, 0, 1);
            imageResample.SetOutputSpacing(input.GetExtent()[1] / resizedWidth, input.GetExtent()[3] / resizedHeight, 1);
            //imageResample.SetOutputExtent(0, panelVTK.Width - 1, 0, panelVTK.Height - 1, 0, 1);

            vtkTransform transform = new vtkTransform();
            transform.Scale(resizedWidth / input.GetExtent()[1], resizedHeight / input.GetExtent()[3], 1);
            imageResample.SetResliceTransform(transform);

            imageResample.Update();

            return imageResample.GetOutput();
        }

        public static vtkImageData ResizeImage(vtkImageData input, double resizedWidth, double resizedHeight, double resize3)
        {
            vtkImageReslice imageResample = new vtkImageReslice();
            imageResample.SetInput(input);
            //imageResample.SetOutputExtent(0, 511, 0, 511, 0, 1);
            imageResample.SetOutputSpacing((input.GetExtent()[1] * input.GetSpacing()[0]) / resizedWidth, (input.GetExtent()[3] * input.GetSpacing()[1]) / resizedHeight, (input.GetExtent()[5] * input.GetSpacing()[2]) / resize3);
            imageResample.SetOutputExtent(0, (int)resizedWidth - 1, 0, (int)resizedHeight - 1, 0, (int)resize3 - 1);

            vtkTransform transform = new vtkTransform();
            transform.Scale(resizedWidth / input.GetExtent()[1], resizedHeight / input.GetExtent()[3], resize3 / input.GetExtent()[5]);
            //imageResample.SetResliceTransform(transform);

            imageResample.Update();

            return imageResample.GetOutput();
        }

        public static vtkMatrix4x4 Landmark(List<double[]> ct, List<double[]> ndi)
        {
            vtkLandmarkTransform landmarkTransform = vtkLandmarkTransform.New();

            vtkPoints sourcePoints = vtkPoints.New();
            vtkPoints destPoints = vtkPoints.New();

            for (int index = 0; index < ct.Count; index++)
            {
                double[] ctPoint = ct[index];
                double[] ndiPoint = ndi[index];

                if (ctPoint != null && ndiPoint != null)
                {
                    sourcePoints.InsertNextPoint(ctPoint[0], ctPoint[1], ctPoint[2]);
                    destPoints.InsertNextPoint(ndiPoint[0], ndiPoint[1], ndiPoint[2]);
                }
            }

            if (sourcePoints.GetNumberOfPoints() < 3)
            {
                throw new Exception(string.Format(
                    "Number of valid marker points should be greater than 3. Actually, it has {0} points.",
                    sourcePoints.GetNumberOfPoints()));
            }

            landmarkTransform.SetSourceLandmarks(sourcePoints);
            landmarkTransform.SetTargetLandmarks(destPoints);

            landmarkTransform.SetModeToRigidBody();

            vtkMatrix4x4 matrix = landmarkTransform.GetMatrix();

            landmarkTransform.Dispose();

            return matrix;
        }

        public static double[] GetViewFromWorld(double x, double y, double z, vtkMatrix4x4 matrix, double m00, double m01, double m02, double m03, double m10, double m11, double m12, double m13, double m20, double m21, double m22, double m23, double m30, double m31, double m32, double m33)
        {
            double[] view = new double[4];
            view[0] = x * m00 + y * m01 +
                      z * m02 + m03;
            view[1] = x * m10 + y * m11 +
                      z * m12 + m13;
            view[2] = x * m20 + y * m21 +
                      z * m22 + m23;
            view[3] = x * m30 + y * m31 +
                      z * m32 + m33;

            double retX, retY, retZ;

            if (view[3] != 0.0)
            {
                retX = view[0] / view[3];
                retY = view[1] / view[3];
                retZ = view[2] / view[3];
            }
            else
            {
                retX = view[0];
                retY = view[1];
                retZ = view[2];
            }
            return new double[] { retX, retY, retZ };
        }

        public static void WorldToView1(double[] pos, out double dx, out double dy, RendererPackage rendererPackage)
        {
            // 1 ?
            rendererPackage.Renderer.SetWorldPoint(pos[0], pos[1], pos[2], 1);
            //rendererPackage.Renderer.SetWorldPoint(new double[] { 0, 0, 0 });
            rendererPackage.Renderer.WorldToDisplay();

            dx = rendererPackage.Renderer.GetDisplayPoint()[0];
            dy = rendererPackage.Renderer.GetDisplayPoint()[1];
            double dz = rendererPackage.Renderer.GetDisplayPoint()[2];

            VTKUtil.Print(pos);
            Console.WriteLine(dx + @" " + dy + @" " + dz);
        }

        public static void WorldToView(double[] pos, out double dx, out double dy, RendererPackage rendererPackage)
        {
            vtkRenderer aRenderer = rendererPackage.Renderer;

            WorldToView(pos, out dx, out dy, aRenderer);
        }

        internal static void WorldToView(double[] pos, out double dx, out double dy, vtkRenderer aRenderer)
        {
            aRenderer.WorldToDisplay();

            vtkMatrix4x4 matrix =
                aRenderer.GetActiveCamera().GetCompositeProjectionTransformMatrix(aRenderer.GetTiledAspectRatio(), 0, 1);
            int[] size = aRenderer.GetVTKWindow().GetSize();
            double[] viewport = aRenderer.GetViewport();

            double m00 = matrix.GetElement(0, 0);
            double m01 = matrix.GetElement(0, 1);
            double m02 = matrix.GetElement(0, 2);
            double m03 = matrix.GetElement(0, 3);
            double m10 = matrix.GetElement(1, 0);
            double m11 = matrix.GetElement(1, 1);
            double m12 = matrix.GetElement(1, 2);
            double m13 = matrix.GetElement(1, 3);
            double m20 = matrix.GetElement(2, 0);
            double m21 = matrix.GetElement(2, 1);
            double m22 = matrix.GetElement(2, 2);
            double m23 = matrix.GetElement(2, 3);
            double m30 = matrix.GetElement(3, 0);
            double m31 = matrix.GetElement(3, 1);
            double m32 = matrix.GetElement(3, 2);
            double m33 = matrix.GetElement(3, 3);

            double[] ret = GetViewFromWorld(
                pos[0],
                pos[1],
                pos[2],
                matrix,
                m00,
                m01,
                m02,
                m03,
                m10,
                m11,
                m12,
                m13,
                m20,
                m21,
                m22,
                m23,
                m30,
                m31,
                m32,
                m33);

            int sizex, sizey;
            /* get physical window dimensions */
            sizex = size[0];
            sizey = size[1];

            dx = (ret[0] + 1.0) * (sizex * (viewport[2] - viewport[0])) / 2.0 + sizex * viewport[0];
            dy = (ret[1] + 1.0) * (sizey * (viewport[3] - viewport[1])) / 2.0 + sizey * viewport[1];
        }


        //public static void AddVolumeRendering(vtkImageReader2 reader, vtkRenderer ren1)
        //{
        //    vtkImageCast imageData = new vtkImageCast();
        //    imageData.SetInputConnection(reader.GetOutputPort());
        //    imageData.SetOutputScalarTypeToUnsignedChar();
        //    imageData.ClampOverflowOn();

        //    vtkKWEGPUVolumeRayCastMapper volumeMapper;
        //    vtkVolumeProperty volumeProperty;

        //    volumeMapper = new vtkKWEGPUVolumeRayCastMapper();
        //    volumeMapper.SetBlendModeToComposite();
        //    volumeMapper.SetInputConnection(imageData.GetOutputPort());

        //    vtkPiecewiseFunction opacity = new vtkPiecewiseFunction();
        //    opacity.AddPoint(20, 0);
        //    opacity.AddPoint(100, 0.0);
        //    opacity.AddPoint(1501, 1);

        //    vtkColorTransferFunction ctfun = new vtkColorTransferFunction();
        //    ctfun.AddRGBPoint(0.0, 0.0, 0.5, 0.0);
        //    ctfun.AddRGBPoint(60.0, 1.0, 0.0, 0.0);
        //    ctfun.AddRGBPoint(128.0, 0.2, 0.1, 0.9);
        //    ctfun.AddRGBPoint(196.0, 0.27, 0.21, 0.1);
        //    ctfun.AddRGBPoint(255.0, 0.8, 0.8, 0.8);

        //    volumeProperty = new vtkVolumeProperty();
        //    volumeProperty.IndependentComponentsOn();
        //    volumeProperty.ShadeOff();
        //    volumeProperty.SetScalarOpacity(opacity);
        //    volumeProperty.SetColor(ctfun);
        //    volumeProperty.ShadeOn();
        //    volumeProperty.SetInterpolationTypeToLinear();
        //    volumeProperty.SetInterpolationTypeToNearest();

        //    vtkVolume volume = new vtkVolume();
        //    volume.SetMapper(volumeMapper);
        //    volume.SetProperty(volumeProperty);

        //    ren1.AddViewProp(volume);    
        //}

        public static void StartInteractorInSingleThread(vtkRenderWindowInteractor iRen)
        {
            if (iRen != null) new System.Threading.Thread(iRen.Start).Start();
        }

        public static void SetInteractorStyleToTrackballCamera(vtkRenderWindowInteractor iRen)
        {
            vtkInteractorStyleTrackballCamera style = new vtkInteractorStyleTrackballCamera();
            iRen.SetInteractorStyle(style);
        }

        public static vtkMetaImageReader GetMetaImageReader(string mhdFileName)
        {
            vtkMetaImageReader metaImageReader = new vtkMetaImageReader();
            metaImageReader.SetFileName(mhdFileName);

            return metaImageReader;
        }

        public static vtkDICOMImageReader GetDICOMImageReader(string dicomFolder)
        {
            vtkDICOMImageReader reader = new vtkDICOMImageReader();
            reader.SetDataByteOrderToLittleEndian();
            reader.SetDirectoryName(dicomFolder);

            return reader;
        }

        public static vtkActor GetMarchingCubeActor(vtkImageReader2 imageReader2, double threshold)
        {
            vtkMarchingCubes boneExtractor = new vtkMarchingCubes();
            boneExtractor.SetInput(imageReader2.GetOutput());
            boneExtractor.SetValue(0, threshold);

            vtkStripper boneStripper = new vtkStripper();
            boneStripper.SetInput(boneExtractor.GetOutput());
            //boneStripper.AddObserver((int)vtk.EventIds.ProgressEvent, new vtkDotNetCallback(vtkStripperProgress));

            vtkPolyDataMapper boneMapper = vtkPolyDataMapper.New();
            boneMapper.SetInput(boneStripper.GetOutput());

            boneMapper.ScalarVisibilityOff();

            vtkActor boneActor = new vtkActor();
            boneActor.SetMapper(boneMapper);
            boneActor.GetProperty().SetDiffuseColor(1, 1, .9412);
            boneActor.GetProperty().SetColor(1, 0, 0);

            return boneActor;
        }

        public static void AddSphere(double x, double y, double z, vtkRenderer renderer)
        {
            vtkSphereSource sphereSource = new vtkSphereSource();
            sphereSource.SetRadius(1);

            vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
            sphereMapper.SetInput(sphereSource.GetOutput());

            vtkActor sphereActor = new vtkActor();

            sphereActor.SetMapper(sphereMapper);
            sphereActor.GetProperty().SetPointSize(100);
            sphereActor.GetProperty().SetColor(1, 0, 0);
            //lineActor.PickableOn();

            //ActiveCamera.SetFocalPoint(x, y, z);
            renderer.AddActor(sphereActor);

            //vtkVectorText atext = new vtkVectorText();
            //atext.SetText("P" + pickedPoints.Count + " " + x.ToString("f1") + " " + y.ToString("f1") + " " + z.ToString("f1"));

            //vtkPolyDataMapper textMapper = new vtkPolyDataMapper();
            //textMapper.SetInputConnection(atext.GetOutputPort());

            //textActor = new vtkFollower();
            //textActor.SetMapper(textMapper);
            //textActor.SetCamera(ren1.GetActiveCamera());
            //textActor.SetScale(5);

            //ren1.AddActor(textActor);

            //textActor.SetPosition(x, y, z);
            sphereSource.SetCenter(x, y, z);
        }

        public static vtkSphereSource SetSphere(double x, double y, double z, vtkSphereSource sphereSource, vtkRenderer renderer)
        {
            if (sphereSource == null)
            {
                sphereSource = new vtkSphereSource();
                sphereSource.SetRadius(1);

                vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
                sphereMapper.SetInput(sphereSource.GetOutput());

                vtkActor sphereActor = new vtkActor();

                sphereActor.SetMapper(sphereMapper);
                sphereActor.GetProperty().SetPointSize(100);
                sphereActor.GetProperty().SetColor(1, 0, 0);
                //lineActor.PickableOn();

                //ActiveCamera.SetFocalPoint(x, y, z);
                renderer.AddActor(sphereActor);

                //vtkVectorText atext = new vtkVectorText();
                //atext.SetText("P" + pickedPoints.Count + " " + x.ToString("f1") + " " + y.ToString("f1") + " " + z.ToString("f1"));

                //vtkPolyDataMapper textMapper = new vtkPolyDataMapper();
                //textMapper.SetInputConnection(atext.GetOutputPort());

                //textActor = new vtkFollower();
                //textActor.SetMapper(textMapper);
                //textActor.SetCamera(ren1.GetActiveCamera());
                //textActor.SetScale(5);

                //ren1.AddActor(textActor);
            }

            //textActor.SetPosition(x, y, z);
            sphereSource.SetCenter(x, y, z);

            return sphereSource;
        }

        public static void Add3DTextActor(string text, vtkRenderer renderer, double x, double y, double z)
        {
            vtkVectorText atext = new vtkVectorText();
            atext.SetText(text);

            vtkPolyDataMapper textMapper = vtkPolyDataMapper.New();
            textMapper.SetInputConnection(atext.GetOutputPort());

            vtkFollower textActor = new vtkFollower();
            textActor.SetMapper(textMapper);
            textActor.SetCamera(renderer.GetActiveCamera());
            textActor.SetScale(5);
            textActor.SetPosition(x, y, z);
        }

        public static vtkSphereSource SetSphere(double[] p1, double[] p2, vtkSphereSource sphereSource, vtkRenderer renderer)
        {
            if (sphereSource == null)
            {
                sphereSource = new vtkSphereSource();

                vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
                sphereMapper.SetInput(sphereSource.GetOutput());

                vtkActor sphereActor = new vtkActor();

                sphereActor.SetMapper(sphereMapper);
                sphereActor.GetProperty().SetPointSize(100);
                sphereActor.GetProperty().SetColor(1, 0, 0);

                renderer.AddActor(sphereActor);
            }

            sphereSource.SetCenter(
                (p1[0] + p2[0]) / 2,
                (p1[1] + p2[1]) / 2,
                (p1[2] + p2[2]) / 2);
            sphereSource.SetRadius(Math.Sqrt(
                (p1[2] - p2[2]) * (p1[2] - p2[2]) +
                (p1[1] - p2[1]) * (p1[1] - p2[1]) +
                (p1[0] - p2[0]) * (p1[0] - p2[0])) / 2);

            return sphereSource;
        }

        public static void ClipPolyDataWithSpheres(List<vtkSphere> clippingSpheres, vtkMarchingCubes boneExtractor, vtkStripper boneStripper, vtkPolyDataMapper boneMapper)
        {
            vtkImplicitBoolean implicitBoolean = new vtkImplicitBoolean();
            foreach (vtkSphere clipSphere in clippingSpheres)
            {
                implicitBoolean.AddFunction(clipSphere);
            }

            vtkClipPolyData clipper = new vtkClipPolyData();
            clipper.SetInputConnection(boneExtractor.GetOutputPort());
            clipper.SetClipFunction(implicitBoolean);
            clipper.InsideOutOn();

            boneStripper.SetInput(clipper.GetOutput());
            boneStripper.Update();

            boneMapper.Update();
        }

        public static void RefreshAll(vtkRenderer aRender, vtkRenderWindow renWin)
        {
            if (aRender != null) aRender.Render();
            if (renWin != null) renWin.Render();
        }

        public static void OverwriteSpacingTo1(string mhdFileName)
        {
            string[] lines = File.ReadAllLines(mhdFileName);
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                if (line.StartsWith("ElementSpacing "))
                {
                    lines[index] = "ElementSpacing = 1 1 1";
                }
            }

            File.WriteAllLines(mhdFileName, lines);
        }

        public static void OverwriteSpacing(double[] spacing, string mhdFileName)
        {
            string 非兴趣区MhdContent = File.ReadAllText(mhdFileName);
            非兴趣区MhdContent = 非兴趣区MhdContent.Replace("ElementSpacing = 1 1 1",
                                                    "ElementSpacing = " + spacing[0] + " " + spacing[1] + " " + spacing[2]);
            File.WriteAllText(mhdFileName, 非兴趣区MhdContent);
        }

        public static double[] TransformPoint(vtkMatrix4x4 matrix, double[] trackerPosition)
        {
            vtkTransform transform = new vtkTransform();
            transform.SetMatrix(matrix);
            return transform.TransformPoint(trackerPosition[0], trackerPosition[1], trackerPosition[2]);
        }

        public static List<double[]> TransformPoints(vtkMatrix4x4 matrix, List<double[]> trackerPositions)
        {
            List<double[]> ret = new List<double[]>();
            foreach (double[] trackerPosition in trackerPositions)
            {
                ret.Add(TransformPoint(matrix, trackerPosition));
            }

            return ret;
        }

        public static double[] InvertTransformPoint(vtkMatrix4x4 matrix, double[] trackerPosition)
        {
            vtkMatrix4x4 ndiToCTMatrix = new vtkMatrix4x4();
            VTKUtil.CopyMatrix(matrix, ndiToCTMatrix);
            ndiToCTMatrix.Invert();

            return TransformPoint(ndiToCTMatrix, trackerPosition);
        }

        public static double[] Closest(vtkPolyData polyData, double[] pos)
        {
            List<double[]> points = new List<double[]>();
            for (int i = 0; i < polyData.GetNumberOfPoints(); ++i)
            {
                double[] point = polyData.GetPoint(i);

                points.Add(point);
            }

            return VTKUtil.Closest(points, pos);
        }

        public static bool CloseTo(vtkPolyData polyData, double[] pos, double threshold)
        {
            for (int i = 0; i < polyData.GetNumberOfPoints(); ++i)
            {
                double[] point = polyData.GetPoint(i);

                if (vtkMath.Distance2BetweenPoints(VTKUtil.ConvertIntPtr(point), VTKUtil.ConvertIntPtr(pos)) < threshold)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool[] CloseTo(vtkPolyData polyData, List<double[]> poses, double threshold)
        {
            bool[] ret = new bool[poses.Count];

            for (int i = 0; i < polyData.GetNumberOfPoints(); ++i)
            {
                double[] point = polyData.GetPoint(i);

                for (int x = 0; x < poses.Count; ++x)
                {
                    if (!ret[x])
                    {
                        double[] pos = poses[x];

                        if (vtkMath.Distance2BetweenPoints(VTKUtil.ConvertIntPtr(point), VTKUtil.ConvertIntPtr(pos)) < threshold)
                        {
                            ret[x] = true;
                            //return true;
                        }
                    }
                }
            }

            return ret;
        }

        public static bool CloseTo(vtkPolyData polyData, double[] pos, double threshold, ref double minDist)
        {
            minDist = double.MaxValue;
            for (int i = 0; i < polyData.GetNumberOfPoints(); ++i)
            {
                double[] point = polyData.GetPoint(i);

                double dist = vtkMath.Distance2BetweenPoints(VTKUtil.ConvertIntPtr(point), VTKUtil.ConvertIntPtr(pos));
                if (dist < minDist)
                {
                    minDist = dist;
                }
                if (dist < threshold)
                {
                    return true;
                }
            }

            return false;
        }

        public static vtkPolyData TransformAndSavePolyData(vtkPolyData polyData, vtkMatrix4x4 matrix, string file)
        {
            vtkPolyData vtkPolyData = TransformPolyData(polyData, matrix);
            VTKUtil.WritePolyData(vtkPolyData, file);

            return vtkPolyData;
        }

        public static vtkPolyData TransformPolyData(vtkPolyData polyData, vtkMatrix4x4 matrix)
        {
            vtkTransformPolyDataFilter TransformPolyDataFilter = new vtkTransformPolyDataFilter();
            TransformPolyDataFilter.SetInput(polyData);

            vtkTransform transform = new vtkTransform();
            transform.SetMatrix(matrix);

            TransformPolyDataFilter.SetTransform(transform);
            TransformPolyDataFilter.Update();

            return TransformPolyDataFilter.GetOutput();
        }

        public static void TransformAndSaveImageData(vtkImageData imageData, vtkMatrix4x4 matrix, string file)
        {
            vtkTransform transform = new vtkTransform();
            transform.SetMatrix(matrix);

            vtkImageReslice resample = new vtkImageReslice();
            resample.SetResliceTransform(transform);
            resample.SetInput(imageData);

            VTKUtil.WriteMhd(resample.GetOutput(), file);
        }

        public static bool 碰撞检测(vtkPolyData polyData1, vtkMatrix4x4 matrix1, vtkPolyData polyData2, vtkMatrix4x4 matrix2)
        {
            TransformAndSavePolyData(polyData1, matrix1, "e:/test/transformP3.vtp");
            TransformAndSavePolyData(polyData2, matrix2, "e:/test/transformP2.vtp");

            ProcessStartInfo processStartInfo = new ProcessStartInfo(@"E:\vtk\vtkCollisionDetectionFilter\vtkbioeng_bin\bin\Debug\TestCollisionDetection.exe", "e:/test/transformP3.vtp E:/test/transformP2.vtp");
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process process = Process.Start(processStartInfo);
            //Process process = Process.Start(@"E:\vtk\vtkCollisionDetectionFilter\vtkbioeng_bin\bin\Debug\TestCollisionDetection.exe", "e:/test/transformP3.vtp E:/test/transformP2.vtp");
            process.WaitForExit();
            Console.WriteLine(process.ExitCode);

            return process.ExitCode != 0;
        }

        public static vtkMatrix4x4 OneMatrix
        {
            get
            {
                vtkMatrix4x4 matrix = new vtkMatrix4x4();
                matrix.Zero();
                matrix.SetElement(0, 0, 1);
                matrix.SetElement(1, 1, 1);
                matrix.SetElement(2, 2, 1);
                matrix.SetElement(3, 3, 1);

                return matrix;
            }
        }

        public static void Convert(List<double> input, double[][] value)
        {
            int index = 0;
            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    value[x][y] = input[index++];
                }
            }
        }

        public static vtkMatrix4x4 Convert(double[,] value)
        {
            vtkMatrix4x4 ret = new vtkMatrix4x4();

            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    ret.SetElement(x, y, value[x, y]);
                }
            }

            return ret;
        }

        public static vtkMatrix4x4 Convert(double[][] value)
        {
            vtkMatrix4x4 ret = new vtkMatrix4x4();

            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    ret.SetElement(x, y, value[x][y]);
                }
            }

            return ret;
        }


        public static double[,] Convert2(double[][] value)
        {
            double[,] ret = new double[4, 4];

            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    ret[x, y] = value[x][y];
                }
            }

            return ret;
        }

        public static vtkMatrix4x4 Convert2vtkMatrix4x4(double[] value)
        {
            vtkMatrix4x4 ret = new vtkMatrix4x4();

            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    ret.SetElement(x, y, value[x * 4 + y]);
                }
            }

            return ret;
        }

        public static double[][] Convert(vtkMatrix4x4 value)
        {
            double[][] ret = new double[4][];

            for (int x = 0; x < 4; ++x)
            {
                ret[x] = new double[4];
                for (int y = 0; y < 4; ++y)
                {
                    ret[x][y] = value.GetElement(x, y);
                }
            }

            return ret;
        }

        public static double[] Convert2DoubleArray(vtkMatrix4x4 value)
        {
            double[] ret = new double[16];

            int idx = 0;
            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    ret[idx++] = value.GetElement(x, y);
                }
            }

            return ret;
        }

        public static void FileLog(string filename)
        {
            vtkFileOutputWindow fileOutputWindow = new vtkFileOutputWindow();
            fileOutputWindow.SetFileName(filename);
            fileOutputWindow.FlushOn();

            vtkFileOutputWindow.SetInstance(fileOutputWindow);
        }

        public static void SaveDouble4x4(string filename, double[][] value)
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    double valueElement = value[x][y];
                    sb.AppendLine(valueElement.ToString());
                }
            }

            File.WriteAllText(filename, sb.ToString());
        }

        public static double[][] LoadDouble4x4(string filename)
        {
            double[][] ret = new double[4][];

            int index = 0;
            for (int x = 0; x < 4; ++x)
            {
                ret[x] = new double[4];
                for (int y = 0; y < 4; ++y)
                {
                    double valueElement = double.Parse(File.ReadAllLines(filename)[index++]);
                    ret[x][y] = valueElement;
                }
            }

            return ret;
        }

        public static vtkImageData Invert(vtkImageData imageData)
        {
            vtkImageData ret = vtkImageData.New();
            ret.SetSpacing(imageData.GetSpacing()[0], imageData.GetSpacing()[1], imageData.GetSpacing()[2]);

            int[] dimensions = imageData.GetDimensions();

            ret.SetDimensions(dimensions[0], dimensions[1], dimensions[2]);
            ret.SetNumberOfScalarComponents(1);
            ret.SetScalarTypeToShort();
            ret.AllocateScalars();

            for (int i = 0; i < dimensions[0]; ++i)
            {
                Console.WriteLine(string.Format("{0}/{1}", i, dimensions[0]));
                for (int j = 0; j < dimensions[1]; ++j)
                {
                    for (int k = 0; k < dimensions[2]; ++k)
                    {
                        double scalarComponentAsDouble = imageData.GetScalarComponentAsDouble(i, j, k, 0);
                        if (scalarComponentAsDouble == -3024)
                        {
                            ret.SetScalarComponentFromDouble(i, j, k, 0, scalarComponentAsDouble);
                        }
                        else
                        {
                            ret.SetScalarComponentFromDouble(i, j, k, 0, -scalarComponentAsDouble);
                        }
                    }
                }
            }

            return ret;
        }

        public static vtkMatrix4x4 Invert(vtkMatrix4x4 value)
        {
            vtkMatrix4x4 ret = vtkMatrix4x4.New();
            CopyMatrix(value, ret);
            ret.Invert();

            return ret;
        }

        public static XYZ Nearest(XYZ p1, XYZ p2, XYZ pX)
        {
            var u = ((pX.X - p1.X) * (p2.X - p1.X)) + ((pX.Y - p1.Y) * (p2.Y - p1.Y)) + ((pX.Z - p1.Z) * (p2.Z - p1.Z));
            var dist = Math.Sqrt(vtkMath.Distance2BetweenPoints(VTKUtil.ConvertIntPtr(p1), VTKUtil.ConvertIntPtr(p2)));
            u = u / (dist * dist);

            XYZ t = new XYZ();
            t.X = p1.X + u * (p2.X - p1.X);
            t.Y = p1.Y + u * (p2.Y - p1.Y);
            t.Z = p1.Z + u * (p2.Z - p1.Z);
            return t;
        }

        public static void GetPSpherePackage(ref SpherePackage pSpherePackage, string name, int r, int g, int b,
                                       double[] position, RendererPackage RendererPackage)
        {
            if (pSpherePackage == null)
            {
                pSpherePackage = new SpherePackage(RendererPackage.Renderer);
                RendererPackage.AddActor(pSpherePackage.Actor);
                pSpherePackage.SetName(name);
                pSpherePackage.SetColor(r, g, b);
            }

            pSpherePackage.SetPosition(position);
            pSpherePackage.SetRadius(1);
        }

        public static double[] MultiPoint(double[] point, vtkMatrix4x4 matrix)
        {
            return matrix.MultiplyDoublePoint(VTKUtil.ConvertTo4P(point));
        }

        public static double[] GetPlane4Param(vtkPlane plane)
        {
            //Ax+By+Cz+D=0
            double[] norm = plane.GetNormal();
            double[] origin = plane.GetOrigin();

            double[] ABCD = new double[4];

            ABCD[0] = norm[0];
            ABCD[1] = norm[1];
            ABCD[2] = norm[2];
            ABCD[3] = -(norm[0] * origin[0] + norm[1] * origin[1] + norm[2] * origin[2]);

            return ABCD;
        }

        public static XYZ Point2PlaneVerticalProjection(vtkPlane plane, XYZ p)
        {
            //x0=input('输入空间点横坐标x0=');
            //y0=input('输入空间点纵坐标y0=');
            //z0=input('输入空间点竖坐标z0=');
            //A=input('平面方程Ax+By+Cz+D=0的系数A=');
            //B=input('平面方程Ax+By+Cz+D=0的系数B=');
            //C=input('平面方程Ax+By+Cz+D=0的系数C=');
            //D=input('平面方程Ax+By+Cz+D=0的系数D=');
            //endoscopeImageAndNavigationInfoThread=-(A*x0+B*y0+C*z0+D)/(A^2+B^2+C^2);
            //disp(['投影点坐标为P=(',num2str(x0+A*endoscopeImageAndNavigationInfoThread),',',num2str(y0+B*endoscopeImageAndNavigationInfoThread),',',num2str(z0+C*endoscopeImageAndNavigationInfoThread),')'])
            double[] ABCD = VTKUtil.GetPlane4Param(plane);
            double x0 = p.X;
            double y0 = p.Y;
            double z0 = p.Z;
            double A = ABCD[0];
            double B = ABCD[1];
            double C = ABCD[2];
            double D = ABCD[3];

            double t = -(A * x0 + B * y0 + C * z0 + D) / (A * A + B * B + C * C);

            double x = x0 + A * t;
            double y = y0 + B * t;
            double z = z0 + C * t;

            return new XYZ(x, y, z);
        }

        public const double pi2angle = 180 / Math.PI;

        public static double AngleOfLine2Line(PointF P11, PointF P12, PointF P21, PointF P22)
        {
            return VTKUtil.AngleOfLine2Line(new XYZ(P11.X, P11.Y, 0), new XYZ(P12.X, P12.Y, 0), new XYZ(P21.X, P21.Y, 0),
                                            new XYZ(P22.X, P22.Y, 0));
        }

        public static double AngleOfLine2Line(XYZ P11, XYZ P12, XYZ P21, XYZ P22)
        {
            return VTKUtil.AngleOfLine2Line(new XYZ(VTKUtil.Subtract(P12, P11)), new XYZ(), new XYZ(VTKUtil.Subtract(P22, P21)));
        }

        public static double AngleOfLine2Line(XYZ P1, XYZ P0, XYZ P2)
        {
            //Line1:P1P0
            //Line2:P0P2
            //angle = Line1->Line2

            //double angleP1P2鍚戦噺=(X2-X1,Y2-Y1,Z1-Z2)
            //P2P3鍚戦噺=(X3-X2,Y3-Y2,Z3-Z2)
            //cos=(P1P2鍚戦噺*P2P3鍚戦噺)/锛圥2P3鍊?P2P3鍊硷級 

            XYZ line1 = new XYZ(P1.X - P0.X, P1.Y - P0.Y, P1.Z - P0.Z);
            XYZ line2 = new XYZ(P2.X - P0.X, P2.Y - P0.Y, P2.Z - P0.Z);

            double norm = (line1.GetNorm() * line2.GetNorm());
            if (norm <= 0)
            {
                return 0;
            }
            else
            {
                double cosAngle = XYZ.DotProduct(line1, line2) / norm;
                return Math.Acos(cosAngle) * pi2angle;
            }
        }

        public static float Distance(float[] 点1, float[] 点2)
        {
            return Distance(new XYZ(点1), new XYZ(点2));
        }

        public static float Distance(PointF 点1, PointF 点2)
        {
            return (float)Math.Sqrt(Math.Pow(点1.X - 点2.X, 2) + Math.Pow(点1.Y - 点2.Y, 2));
        }

        public static float Distance(double[] 点1, double[] 点2)
        {
            return (float)Math.Sqrt(vtkMath.Distance2BetweenPoints(VTKUtil.ConvertIntPtr(点1), VTKUtil.ConvertIntPtr(点2)));
        }

        public static float DistanceAccuracy2Closet(vtkPolyData polyData, double[] 点2)
        {
            double[] closest = Closest(polyData, 点2);
            return DistanceAccuracy(closest, 点2);
        }
        public static float DistanceAccuracy2Closet(List<double[]> points, double[] 点2)
        {
            double[] closest = Closest(points, 点2);
            return DistanceAccuracy(closest, 点2);
        }
        public static double DistanceAccuracy2ClosetOnAxisZ(List<double[]> points, double[] 点2)
        {
            double minDist = double.MaxValue;

            foreach (double[] point in points)
            {
                double dd = DistanceAccuracyOnAxisZ(point, 点2);
                minDist = Math.Min(minDist, dd);
            }

            return minDist;
        }
        public static double DistanceAccuracy2ClosetOnPlaneXY(List<double[]> points, double[] 点2)
        {
            double minDist = double.MaxValue;

            foreach (double[] point in points)
            {
                double dd = DistanceAccuracyOnXYPlane(point, 点2);
                minDist = Math.Min(minDist, dd);
            }

            return minDist;
        }

        public static float DistanceAccuracy(double[] 点1, double[] 点2)
        {
            return (float)Math.Sqrt(
                Math.Pow(点1[0] - 点2[0], 2) +
                Math.Pow(点1[1] - 点2[1], 2) +
                Math.Pow(点1[2] - 点2[2], 2)
                );
        }

        public static float DistanceAccuracyOnAxisZ(double[] 点1, double[] 点2)
        {
            return (float)Math.Sqrt(Math.Pow(点1[2] - 点2[2], 2));
        }

        public static float DistanceAccuracyOnXYPlane(double[] 点1, double[] 点2)
        {
            return (float)Math.Sqrt(
                Math.Pow(点1[0] - 点2[0], 2) +
                Math.Pow(点1[1] - 点2[1], 2)
                );
        }

        public static double DistanceAccuracyWithoutSqrt(double[] 点1, double[] 点2)
        {
            return Math.Pow(点1[0] - 点2[0], 2) +
                Math.Pow(点1[1] - 点2[1], 2) +
                Math.Pow(点1[2] - 点2[2], 2);
        }

        /// <summary>  
        /// 求直线外一点到该直线的投影点  
        /// </summary>  
        /// <param name="pLine">线上任一点</param>  
        /// <param name="k">直线斜率</param>  
        /// <param name="pOut">线外指定点</param>  
        /// <param name="pProject">投影点</param>  
        internal static void GetProjectivePoint(PointF pLine, double k, PointF pOut, ref PointF pProject)
        {
            if (k == 0) //垂线斜率不存在情况  
            {
                pProject.X = pOut.X;
                pProject.Y = pLine.Y;
            }
            else
            {
                pProject.X = (float)((k * pLine.X + pOut.X / k + pOut.Y - pLine.Y) / (1 / k + k));
                pProject.Y = (float)(-1 / k * (pProject.X - pOut.X) + pOut.Y);
            }
        }

        internal static void GetProjectivePoint(PointF pLine1, PointF pLine2, PointF pOut, ref PointF pProject)
        {
            double k;
            if (pLine1.X == pLine2.X) //垂线斜率不存在情况  
            {
                k = 0;
            }
            else
            {
                k = (pLine1.Y - pLine2.Y) / (pLine1.X - pLine2.X);
            }

            GetProjectivePoint(pLine1, k, pOut, ref pProject);
        }

        public static double[] Normalize(double[] value)
        {
            //Dicom.Imaging.Mathematics.Vector3D vector1 = new Dicom.Imaging.Mathematics.Vector3D(value[0], value[1], value[2]);
            //vector1.Normalize();
            //return new double[] { vector1.X, vector1.Y, vector1.Z };

            Vector3 v = new Vector3((float)value[0], (float)value[1], (float)value[2]);
            v.Normalize();
            return new double[] { v.X, v.Y, v.Z };
        }

        public static double[] Normalize(double[] p1, double[] p2)
        {
            return Normalize(Subtract(p1, p2));
        }

        public static double[] Subtract(double[] value1, double[] value2)
        {
            double[] ret = new double[Math.Min(value1.Length, value2.Length)];
            for (int index = 0; index < ret.Length; index++)
            {
                ret[index] = value1[index] - value2[index];
            }

            return ret;
        }

        public static double[] Multiply(double[] value1, double scalar)
        {
            double[] ret = new double[value1.Length];
            for (int index = 0; index < ret.Length; index++)
            {
                ret[index] = value1[index] * scalar;
            }

            return ret;
        }

        public static double[] Add(double[] value1, double[] value2)
        {
            double[] ret = new double[Math.Min(value1.Length, value2.Length)];
            for (int index = 0; index < ret.Length; index++)
            {
                ret[index] = value1[index] + value2[index];
            }

            return ret;
        }

        public static double[] 距离(double[] 方向, double 距离)
        {
            return new double[] { 方向[0] * 距离, 方向[1] * 距离, 方向[2] * 距离 };
        }

        public static double[] 距离(double[] 方向, double 距离, double[] startPoint)
        {
            return new double[] { 方向[0] * 距离 + startPoint[0], 方向[1] * 距离 + startPoint[1], 方向[2] * 距离 + startPoint[2] };
        }

        public static Vector3 Convert(double[] value)
        {
            return new Vector3((float)value[0], (float)value[1], (float)value[2]);
        }

        public static float[] ConvertD2F(double[] value)
        {
            return new float[] { (float)value[0], (float)value[1], (float)value[2] };
        }

        public static double[] Convert(float[] value)
        {
            return new double[] { (double)value[0], (double)value[1], (double)value[2] };
        }

        public static double[] Convert(Vector3 value)
        {
            return new double[] { value.X, value.Y, value.Z };
        }

        public static double[] GetNormal(double[] 力线点1, double[] 力线点2)
        {
            double[] 力线1To力线2 = Subtract(力线点1, 力线点2);
            Vector3 力线norm = new Vector3((float)力线1To力线2[0], (float)力线1To力线2[1], (float)力线1To力线2[2]);
            力线norm.Normalize();

            return new double[] { 力线norm.X, 力线norm.Y, 力线norm.Z };
        }

        public static double[] 垂线(double[] 线方向)
        {
            double[] 线方向1 = VTKUtil.Add(线方向, new double[] { 1, 0.00034, 0 });
            return Normalize(Convert(Vector3.Cross(Convert(线方向), Convert(线方向1))));
        }

        public static double[] Cross(double[] direction1, double[] direction2)
        {
            //Dicom.Imaging.Mathematics.Vector3D vector1 = new Dicom.Imaging.Mathematics.Vector3D(direction1[0], direction1[1], direction1[2]);
            //Dicom.Imaging.Mathematics.Vector3D vector2 = new Dicom.Imaging.Mathematics.Vector3D(direction2[0], direction2[1], direction2[2]);

            //Dicom.Imaging.Mathematics.Vector3D vector3 = vector1.CrossProduct(vector2);
            //vector3.Normalize();

            //return new double[]
            //{
            //    vector3.X,vector3.Y,vector3.Z
            //};
            return Normalize(Convert(Vector3.Cross(Convert(direction1), Convert(direction2))));
        }

        public static vtkPolyData Clip(vtkPlane clippingPlane, vtkPolyData input)
        {
            vtkClipPolyData clipper = new vtkClipPolyData();
            clipper.SetInput(input);
            clipper.SetClipFunction(clippingPlane);
            //clipper.InsideOutOn();

            return clipper.GetOutput();
        }

        public static vtkPolyData Clip(vtkCone clippingPlane, vtkPolyData input)
        {
            vtkClipPolyData clipper = new vtkClipPolyData();
            clipper.SetInput(input);
            clipper.SetClipFunction(clippingPlane);
            //clipper.InsideOutOn();

            return clipper.GetOutput();
        }

        public static vtkPolyData Clip(vtkImplicitFunction clippingPlane, vtkPolyData input)
        {
            vtkClipPolyData clipper = new vtkClipPolyData();
            clipper.SetInput(input);
            clipper.SetClipFunction(clippingPlane);
            //clipper.InsideOutOn();

            return clipper.GetOutput();
        }

        public static vtkPolyData Unclip(vtkImplicitFunction clippingPlane, vtkPolyData input)
        {
            vtkClipPolyData clipper = new vtkClipPolyData();
            clipper.SetInput(input);
            clipper.SetClipFunction(clippingPlane);
            clipper.InsideOutOn();

            return clipper.GetOutput();
        }

        public static double[] RandPos()
        {
            return new double[] { vtkMath.Random(0, 200), vtkMath.Random(0, 200), vtkMath.Random(0, 200) };
        }

        public static byte Short2UChar(int windowLevel, int windowWidth, object inputvalue)
        {
            int lower = windowLevel - windowWidth / 2;
            int upper = windowLevel + windowWidth / 2;

            double value = System.Convert.ToDouble(inputvalue);
            //Console.WriteLine(value);

            if (value < lower)
            {
                return 0;
            }
            else if (value > upper)
            {
                return 255;
            }
            else
            {
                byte pixel = (byte)((value - lower) * 255 / windowWidth);

                return pixel;
            }
        }

        public static void CopyCamera(vtkCamera from, vtkCamera to)
        {
            //to.SetUserTransform(from.GetUserTransform());
            //to.SetUserViewTransform(from.GetUserViewTransform());

            //return;

            to.SetClippingRange(VTKUtil.ConvertIntPtr(from.GetClippingRange()));
            to.SetDistance(from.GetDistance());
            to.SetEyeAngle(from.GetEyeAngle());
            to.SetFocalDisk(from.GetFocalDisk());
            to.SetFocalPoint(VTKUtil.ConvertIntPtr(from.GetFocalPoint()));
            to.SetPosition(VTKUtil.ConvertIntPtr(from.GetPosition()));
            to.SetViewUp(VTKUtil.ConvertIntPtr(from.GetViewUp()));
            to.SetThickness(from.GetThickness());
            to.SetViewAngle(from.GetViewAngle());
            to.SetViewShear(VTKUtil.ConvertIntPtr(from.GetViewShear()));
            to.SetWindowCenter(from.GetWindowCenter()[0], from.GetWindowCenter()[1]);
            to.SetParallelScale(from.GetParallelScale());
        }

        public static void AddLight1(RendererPackage _rendererPackage)
        {
            vtkLightKit lightKit = new vtkLightKit();
            //lightKit.SetKeyLightIntensity(0.8);
            lightKit.SetKeyLightWarmth(0.6);

            // TODO Undo me
            lightKit.SetKeyLightIntensity(0.15 * 1);
            lightKit.SetKeyLightElevation(50);
            lightKit.SetKeyLightAzimuth(50);
            lightKit.MaintainLuminanceOff();

            lightKit.SetKeyToFillRatio(3);

            lightKit.SetFillLightWarmth(0.4);
            lightKit.SetFillLightElevation(-75);
            lightKit.SetFillLightAzimuth(-30);

            lightKit.SetBackLightWarmth(0.5);
            lightKit.SetBackLightElevation(0);
            lightKit.SetBackLightAzimuth(110);

            //lightKit.SetHeadlightWarmth(0.5);

            lightKit.AddLightsToRenderer(_rendererPackage.Renderer);
        }

        public static void AddLight(RendererPackage _rendererPackage)
        {
            vtkLightKit lightKit = new vtkLightKit();
            //lightKit.SetKeyLightIntensity(0.8);
            lightKit.SetKeyLightWarmth(0.6);

            // TODO Undo me
            lightKit.SetKeyLightIntensity(0.75 * 1);
            lightKit.SetKeyLightElevation(50);
            lightKit.SetKeyLightAzimuth(10);
            lightKit.MaintainLuminanceOff();

            lightKit.SetKeyToFillRatio(3);

            lightKit.SetFillLightWarmth(0.4);
            lightKit.SetFillLightElevation(-75);
            lightKit.SetFillLightAzimuth(-10);

            lightKit.SetBackLightWarmth(0.5);
            lightKit.SetBackLightElevation(0);
            lightKit.SetBackLightAzimuth(110);

            //lightKit.SetHeadlightWarmth(0.5);

            lightKit.AddLightsToRenderer(_rendererPackage.Renderer);
        }

        public static void SetVisible(XmlPolyDataPackage p, bool isVisiible)
        {
            if (p != null)
            {
                if (isVisiible)
                {
                    p.VisibilityOn();
                }
                else
                {
                    p.VisibilityOff();
                }
            }
        }

        public static double[] RndColor
        {
            get
            {
                double r = vtkMath.Random(0, 1);
                double g = vtkMath.Random(0, 1);
                double b = vtkMath.Random(0, 1);

                return new double[] { r, g, b };
            }
        }

        public static void SetRandomColor(XmlPolyDataPackage p1)
        {
            double r = vtkMath.Random(0, 1);
            double g = vtkMath.Random(0, 1);
            double b = vtkMath.Random(0, 1);

            p1.SetColor(r, g, b);
        }

        public static void SetRandomColor(TubePackage p1)
        {
            double r = vtkMath.Random(0, 1);
            double g = vtkMath.Random(0, 1);
            double b = vtkMath.Random(0, 1);

            p1.SetColor(r, g, b);
        }

        public static vtkPolyData Merge(params XmlPolyDataPackage[] elements)
        {
            vtkAppendPolyData appendPolyData = new vtkAppendPolyData();
            foreach (XmlPolyDataPackage polyData in elements)
            {
                appendPolyData.AddInput(polyData.PolyData);
            }

            return appendPolyData.GetOutput();
        }

        public static vtkPolyData Merge(params vtkPolyData[] elements)
        {
            vtkAppendPolyData appendPolyData = new vtkAppendPolyData();
            foreach (vtkPolyData polyData in elements)
            {
                appendPolyData.AddInput(polyData);
            }

            return appendPolyData.GetOutput();
        }

        /// <summary>
        /// 平分折线
        /// </summary>
        /// <param name="points"></param>
        /// <param name="div"></param>
        /// <returns></returns>
        public static List<double[]> Divide(List<double[]> points, int div)
        {
            var ret = new List<double[]>();

            // 折线总长度
            double distanceSum = 0;
            for (int i = 0; i < points.Count - 1; ++i)
            {
                distanceSum += VTKUtil.DistanceAccuracy(points[i], points[i + 1]);
            }

            //Console.WriteLine(@"distanceSum = " + distanceSum);

            // 每段长度
            double eachDistance = distanceSum / (double)div;

            for (int i = 0; i <= div; ++i)
            {
                double distance = eachDistance * i;
                //Console.WriteLine(@"distance = " + distance);

                double tmpDistance1 = 0;

                for (int j = 0; j < points.Count - 1; ++j)
                {
                    tmpDistance1 += VTKUtil.DistanceAccuracy(points[j], points[j + 1]);
                    if (tmpDistance1 >= distance)
                    {
                        double distanceDiff = tmpDistance1 - distance;
                        double percentage = distanceDiff / VTKUtil.DistanceAccuracy(points[j], points[j + 1]) / 100.0;
                        //Console.WriteLine(@"percentage = " + percentage);

                        double[] point = new double[]
                        {
                            points[j+1][0] - percentage * Math.Abs(points[j][0] - points[j + 1][0]),
                            points[j+1][1] - percentage * Math.Abs(points[j][1] - points[j + 1][1]),
                            points[j+1][2] - percentage * Math.Abs(points[j][2] - points[j + 1][2]),
                        };
                        //VTKUtil.Print(point);

                        ret.Add(point);

                        break;
                    }
                }
            }
            while (ret.Count > div)
            {
                ret.RemoveAt(0);
            }

            return ret;
        }

        public static double[] Divide(double[] start, double[] end, int div, int idx)
        {
            double x = start[0] + (end[0] - start[0]) / div * idx;
            double y = start[1] + (end[1] - start[1]) / div * idx;
            double z = start[2] + (end[2] - start[2]) / div * idx;

            return new double[] { x, y, z };
        }

        public static PointF Divide(PointF start, PointF end, int div, int idx)
        {
            float x = start.X + (end.X - start.X) / div * idx;
            float y = start.Y + (end.Y - start.Y) / div * idx;

            return new PointF(x, y);
        }


        public static void SaveMatrix(vtkMatrix4x4 matrix, string filename)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    sb.Append(matrix.GetElement(i, j) + " ");
                }
                sb.AppendLine();
            }

            File.WriteAllText(filename, sb.ToString());
        }

        public static vtkMatrix4x4 LoadMatrix(string filename)
        {
            vtkMatrix4x4 matrix = new vtkMatrix4x4();
            string[] alltext = System.IO.File.ReadAllLines(filename);
            for (int i = 0; i < 4; i++)
            {
                string line = alltext[i].Trim();
                while (line.Contains("  "))
                {
                    line = line.Replace("  ", " ");
                }

                string[] text = line.Replace("  ", " ").Trim().Split(' ');
                for (int j = 0; j < 4; j++)
                {
                    matrix.SetElement(i, j, double.Parse(text[j].Trim()));
                }
            }

            return matrix;
        }


        public static vtkMatrix4x4 GenerateMatrix(float rx, float ry, float rz, float tx, float ty, float tz)
        {
            vtkTransform transform = new vtkTransform();
            transform.RotateX(rx);
            transform.RotateY(ry);
            transform.RotateZ(rz);

            transform.Translate(tx, ty, tz);

            return transform.GetMatrix();
        }

        public static vtkPolyData CleanPolyData(vtkPolyData polyData)
        {
            vtkCleanPolyData vtkCleanPolyData = vtkCleanPolyData.New();
            vtkCleanPolyData.SetInput(polyData);
            vtkCleanPolyData.Update();

            vtkPolyData cleanPolyData = vtkCleanPolyData.GetOutput();

            vtkCleanPolyData.Dispose();

            return cleanPolyData;
        }


        public static void AttachOpacity(vtkSphereWidget sw, TrackBar tb, RendererPackage rendererPackage)
        {
            tb.Minimum = 1;
            tb.Maximum = 100;

            tb.ValueChanged += delegate
            {
                sw.GetSphereProperty().SetOpacity(tb.Value / 100f);
                rendererPackage.RefreshAll();
            };
        }

        public static vtkPolyData Deluny3D(List<double[]> points, double alpha)
        {
            vtkPoints pts = new vtkPoints();

            for (int i = 0; i < points.Count; ++i)
            {
                pts.InsertPoint(i, points[i][0], points[i][1], points[i][2]);
            }

            vtkUnstructuredGrid profile = new vtkUnstructuredGrid();
            //Specify point array to define point coordinates.
            profile.SetPoints(pts);

            vtkDelaunay3D delny = new vtkDelaunay3D();
            delny.SetInput(profile);
            delny.SetTolerance(2);
            delny.SetAlpha(alpha);
            delny.Update();

            //vtkPolyDataMapper map = new vtkPolyDataMapper();
            //map.SetInput(vtkPolyData.SafeDownCast(delny.GetOutput()));


            vtkGeometryFilter geoFilter = new vtkGeometryFilter();
            geoFilter.SetInputConnection(delny.GetOutputPort());
            geoFilter.Update();

            return geoFilter.GetOutput();
        }

        private static double DoublePow2(double[] pA, double[] pB)
        {
            return (pA[0] - pB[0]) * (pA[0] - pB[0]) + (pA[1] - pB[1]) * (pA[1] - pB[1]) + (pA[2] - pB[2]) * (pA[2] - pB[2]);
        }

        public static double[] Closest(List<double[]> points, double[] point, out int idx)
        {
            double dist = double.MaxValue;
            foreach (double[] pointA in points)
            {
                double distance = DoublePow2(point, pointA);

                if (distance < dist)
                {
                    dist = distance;
                }
            }

            for (int i = 0; i < points.Count; ++i)
            //foreach (double[] pointA in points)
            {
                double[] pointA = points[i];
                double distance = DoublePow2(point, pointA);

                if (distance == dist)
                {
                    idx = i;
                    return pointA;
                }
            }

            idx = -1;
            return null;
        }

        public static double[] Closest(List<double[]> points, double[] point)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            double dist = double.MaxValue;
            foreach (double[] pointA in points)
            {
                double distance = DoublePow2(point, pointA);

                if (distance < dist)
                {
                    dist = distance;
                }
            }

            foreach (double[] pointA in points)
            {
                double distance = DoublePow2(point, pointA);

                if (distance == dist)
                {
                    return pointA;
                }
            }

            return null;
        }

        public static IntPtr ConvertTo4P(double[] three)
        {
            if (three == null || three.Length != 3)
            {
                //throw new Exception("Invalid input");
            }
            return VTKUtil.ConvertIntPtr(new double[] { three[0], three[1], three[2], 1 });
        }

        //public static void AttachButton(RendererPackage rendererPackage, RegistrationEntity registrationEntity, int idx, Button button)
        //{
        //    AttachButton(rendererPackage, registrationEntity, idx, button, null, null);
        //}

        //public static void AttachButton(
        //    RendererPackage rendererPackage,
        //    RegistrationEntity registrationEntity,
        //    int idx,
        //    Button button,
        //    Delegates.SuccessDelegate successDelegate,
        //    Delegates.FailureDelegate failureDelegate
        //    )
        //{
        //    button.Click += delegate
        //    {
        //        registrationEntity.SetCTPoint(idx, rendererPackage.cursor.GetPosition());
        //        rendererPackage.SetPosition("ct" + idx + 1, rendererPackage.cursor.GetPosition());

        //        rendererPackage.RefreshAll();
        //    };
        //}

        //public static void AttachButtonNDI(
        //    RendererPackage rendererPackage,
        //    RegistrationEntity registrationEntity,
        //    int idx,
        //    Button button
        //    )
        //{
        //    AttachButtonNDI(rendererPackage, registrationEntity, idx, button);
        //}

#if NDI
        public static void AttachButtonNDI(
            THRETracker referenceTracker,
            THRETracker pointerTracker,
            RegistrationEntity registrationEntity,
            int idx,
            Button button,
            JXDWNav.Delegates.SuccessDelegate successDelegate,
            JXDWNav.Delegates.FailureDelegate failureDelegate
            )
        {
            button.Click += delegate
            {
                PolarisTracker polarisTracker = PolarisTracker.Instance;

                NDITracker tracker = polarisTracker.GetTrackerInfoSingleRound(pointerTracker.Idx, referenceTracker.Idx);

                if (tracker == null)
                {
                    if (failureDelegate != null)
                    {
                        failureDelegate();
                    }
                }
                else
                {
                    if (successDelegate != null)
                    {
                        successDelegate();
                    }
                    registrationEntity.SetNDIPoint(idx, tracker.RawCTPosition);
                }
            };
        }
#endif

        public static double EvalError(List<double[]> ct, List<double[]> ndi, vtkMatrix4x4 ct2ndiMatrix)
        {
            List<double> erros = new List<double>();
            for (int i = 0; i < ct.Count; ++i)
            {
                double[] dest = MultiPoint(ct[i], ct2ndiMatrix);
                double err = Distance(ndi[i], dest);

                erros.Add(err);
            }

            return erros.Average();
        }

        public static double[] Point2PlaneVerticalProjection(vtkPlane plane, double[] p)
        {
            //x0=input('输入空间点横坐标x0=');
            //y0=input('输入空间点纵坐标y0=');
            //z0=input('输入空间点竖坐标z0=');
            //A=input('平面方程Ax+By+Cz+D=0的系数A=');
            //B=input('平面方程Ax+By+Cz+D=0的系数B=');
            //C=input('平面方程Ax+By+Cz+D=0的系数C=');
            //D=input('平面方程Ax+By+Cz+D=0的系数D=');
            //endoscopeImageAndNavigationInfoThread=-(A*x0+B*y0+C*z0+D)/(A^2+B^2+C^2);
            //disp(['投影点坐标为P=(',num2str(x0+A*endoscopeImageAndNavigationInfoThread),',',num2str(y0+B*endoscopeImageAndNavigationInfoThread),',',num2str(z0+C*endoscopeImageAndNavigationInfoThread),')'])
            double[] ABCD = VTKUtil.GetPlane4Param(plane);
            double x0 = p[0];
            double y0 = p[1];
            double z0 = p[2];
            double A = ABCD[0];
            double B = ABCD[1];
            double C = ABCD[2];
            double D = ABCD[3];

            double t = -(A * x0 + B * y0 + C * z0 + D) / (A * A + B * B + C * C);

            double x = x0 + A * t;
            double y = y0 + B * t;
            double z = z0 + C * t;

            return new double[] { x, y, z };
        }

        internal static vtkPolyData CreateCurve(List<List<double[]>> pointLists)
        {
            // Create a cell array to store the lines in and add the lines to it
            var cells = new vtkCellArray();

            // Create a vtkPoints object and store the points in it
            var points = new vtkPoints();

            int pointIdx = 0;

            foreach (List<double[]> pointList in pointLists)
            {
                for (int i = 0; i < pointList.Count; ++i)
                {
                    points.InsertPoint(pointIdx + i, pointList[i][0], pointList[i][1], pointList[i][2]);
                }


                var polyLine = new vtkPolyLine();
                polyLine.GetPointIds().SetNumberOfIds(pointList.Count);

                for (int i = 0; i < pointList.Count; i++)
                {
                    polyLine.GetPointIds().SetId(i, pointIdx + i);
                }

                pointIdx += pointList.Count;

                cells.InsertNextCell(polyLine);
            }

            // Create a polydata to store everything in
            var polyData = new vtkPolyData();

            // Add the points to the dataset
            polyData.SetPoints(points);

            // Add the lines to the dataset
            polyData.SetLines(cells);

            return polyData;
        }

        internal static vtkPolyData CreateCurveAutoClose(List<double[]> pointList)
        {
            List<double[]> newPointList = new List<double[]>(pointList);
            newPointList.Add(pointList[0]);

            return CreateCurve(newPointList);
        }

        internal static vtkPolyData CreateCurve(List<double[]> pointList)
        {
            // Create a cell array to store the lines in and add the lines to it
            var cells = new vtkCellArray();

            // Create a vtkPoints object and store the points in it
            var points = new vtkPoints();

            for (int i = 0; i < pointList.Count; ++i)
            {
                points.InsertPoint(i, pointList[i][0], pointList[i][1], pointList[i][2]);
            }

            var polyLine = new vtkPolyLine();
            polyLine.GetPointIds().SetNumberOfIds(pointList.Count);

            for (int i = 0; i < pointList.Count; i++)
            {
                polyLine.GetPointIds().SetId(i, i);
            }

            cells.InsertNextCell(polyLine);

            // Create a polydata to store everything in
            var polyData = new vtkPolyData();

            // Add the points to the dataset
            polyData.SetPoints(points);

            // Add the lines to the dataset
            polyData.SetLines(cells);

            return polyData;
        }

        public static double[] Middle(double[] a, double[] b)
        {
            return new double[] { (a[0] + b[0]) / 2, (a[1] + b[1]) / 2, (a[2] + b[2]) / 2 };
        }

        public static vtkImageData ReadMetaImage(string mhdFileName)
        {
            File.ReadAllBytes(mhdFileName);

            vtkMetaImageReader metaImageReader = vtkMetaImageReader.New();
            metaImageReader.SetFileName(mhdFileName);
            metaImageReader.Update();

            vtkImageData readMetaImage = metaImageReader.GetOutput();

            metaImageReader.Dispose();
            metaImageReader = null;

            return readMetaImage;
        }

        public static vtkAlgorithmOutput ReadMetaImagePort(string mhdFileName)
        {
            vtkMetaImageReader metaImageReader = vtkMetaImageReader.New();
            metaImageReader.SetFileName(mhdFileName);

            return metaImageReader.GetOutputPort();
        }

        public static System.Drawing.Point ImageCenterControlToImageWithEnlarge(int imageWidth, int imageHeight, int controlWidth, int controlHeight, int controlX, int controlY)
        {
            int x0, y0;
            int x1, y1;
            double c = (double)imageWidth / imageHeight;
            int imgWidth, imgHeight;
            if (c >= (double)controlWidth / controlHeight)
            {
                imgWidth = controlWidth;
                imgHeight = System.Convert.ToInt32(imgWidth / c);
                x0 = controlX;
                y0 = controlY - (controlHeight - imgHeight) / 2;
            }
            else
            {
                imgWidth = System.Convert.ToInt32(controlHeight * c);
                imgHeight = System.Convert.ToInt32(imgWidth / c);
                x0 = controlX - (controlWidth - imgWidth) / 2;
                y0 = controlY;
            }
            x1 = x0 * imageWidth / imgWidth;
            y1 = y0 * imageHeight / imgHeight;
            return new System.Drawing.Point(x1, y1);
        }

        public static double[] Extend(double[] p1, double[] p2, double distance)
        {
            double dx = p1[0] - p2[0];
            double dy = p1[1] - p2[1];
            double dz = p1[2] - p2[2];

            double initDistance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2) + Math.Pow(dz, 2));

            double scale = distance / initDistance;

            double rx = p2[0] - dx * scale;
            double ry = p2[1] - dy * scale;
            double rz = p2[2] - dz * scale;

            return new double[] { rx, ry, rz };
        }

        public static double[] Extend2(double[] p1, double[] p2, double distance)
        {
            double dx = p2[0] - p1[0];
            double dy = p2[1] - p1[1];
            double dz = p2[2] - p1[2];

            double initDistance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2) + Math.Pow(dz, 2));

            double scale = distance / initDistance;

            double rx = p1[0] + dx * scale;
            double ry = p1[1] + dy * scale;
            double rz = p1[2] + dz * scale;

            return new double[] { rx, ry, rz };
        }

        public static double[] Extend2AlongDirection(double[] p1, double[] direction, double distance)
        {
            double[] p2 = VTKUtil.Add(p1, direction);
            double dx = p2[0] - p1[0];
            double dy = p2[1] - p1[1];
            double dz = p2[2] - p1[2];

            double initDistance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2) + Math.Pow(dz, 2));

            double scale = distance / initDistance;

            double rx = p1[0] + dx * scale;
            double ry = p1[1] + dy * scale;
            double rz = p1[2] + dz * scale;

            return new double[] { rx, ry, rz };
        }

        internal static double[] ConvertTo3P(double[] p)
        {
            return new double[] { p[0], p[1], p[2] };
        }

        public static bool IsEqual(double[] p1, double[] p2)
        {
            if (p1[0] == p2[0] && p1[1] == p2[1] && p1[2] == p2[2])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static double[] MaxDistanceOnXYZ(List<double[]> pathPoints)
        {
            double maxDistance1 = double.MinValue;
            double maxDistance2 = double.MinValue;
            double maxDistance3 = double.MinValue;

            foreach (double[] point1 in pathPoints)
            {
                foreach (double[] point2 in pathPoints)
                {
                    double distance1 = Math.Abs(point1[0] - point2[0]);
                    if (distance1 > maxDistance1)
                    {
                        maxDistance1 = distance1;
                    }

                    double distance2 = Math.Abs(point1[1] - point2[1]);
                    if (distance2 > maxDistance2)
                    {
                        maxDistance2 = distance2;
                    }

                    double distance3 = Math.Abs(point1[2] - point2[2]);
                    if (distance3 > maxDistance3)
                    {
                        maxDistance3 = distance3;
                    }
                }
            }

            return new double[] { maxDistance1, maxDistance2, maxDistance3 };
        }

        internal static double MaxDistance(List<double[]> pathPoints)
        {
            double maxDistance = double.MinValue;

            foreach (double[] point1 in pathPoints)
            {
                foreach (double[] point2 in pathPoints)
                {
                    double distance = VTKUtil.DistanceAccuracy(point1, point2);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                    }
                }
            }

            return maxDistance;
        }

        public static vtkPolyData Connective(vtkPolyData airwayPolyData)
        {
            Kitware.VTK.vtkConnectivityFilter connectivityFilter = Kitware.VTK.vtkConnectivityFilter.New();
            connectivityFilter.SetInput(airwayPolyData);
            connectivityFilter.SetExtractionModeToLargestRegion();
            connectivityFilter.Update();

            Kitware.VTK.vtkGeometryFilter sp = Kitware.VTK.vtkGeometryFilter.New();
            sp.SetInput(connectivityFilter.GetOutput());
            sp.Update();

            return sp.GetOutput();
        }
    }

    internal class CameraUtil
    {
        internal static void AdjustCameraX(vtkImagePlaneWidget imagePlaneWidget, RendererPackage rendererPackageReslice)
        {
            imagePlaneWidget.SetPlaneOrientationToXAxes();

            AdjustCamera(imagePlaneWidget, rendererPackageReslice, new double[] { 0, 0, -1 });
        }

        internal static void AdjustCameraY(vtkImagePlaneWidget imagePlaneWidget, RendererPackage rendererPackageReslice)
        {
            imagePlaneWidget.SetPlaneOrientationToYAxes();

            AdjustCamera(imagePlaneWidget, rendererPackageReslice, new double[] { 0, 0, -1 });
        }

        internal static void AdjustCameraZ(vtkImagePlaneWidget imagePlaneWidget, RendererPackage rendererPackageReslice)
        {
            imagePlaneWidget.SetPlaneOrientationToZAxes();

            AdjustCamera(imagePlaneWidget, rendererPackageReslice, new double[] { 0, 1, 0 });
        }

        internal static void AdjustCamera(vtkImagePlaneWidget imagePlaneWidget, RendererPackage rendererPackageReslice, double[] viewUp)
        {
            double[] planeOrigin = imagePlaneWidget.GetOrigin();
            double[] cameraPos = new double[]
                                     {
                                         planeOrigin[0] + imagePlaneWidget.GetNormal()[0],
                                         planeOrigin[1] + imagePlaneWidget.GetNormal()[1],
                                         planeOrigin[2] + imagePlaneWidget.GetNormal()[2]
                                     };

            rendererPackageReslice.Renderer.GetActiveCamera().SetFocalPoint(planeOrigin[0], planeOrigin[1], planeOrigin[2]);
            rendererPackageReslice.Renderer.GetActiveCamera().SetPosition(cameraPos[0], cameraPos[1], cameraPos[2]);
            rendererPackageReslice.Renderer.GetActiveCamera().ComputeViewPlaneNormal();

            rendererPackageReslice.Renderer.GetActiveCamera().SetViewUp(viewUp[0], viewUp[1], viewUp[2]);

            vtkPolyData polyData = vtkPolyData.New();
            imagePlaneWidget.GetPolyData(polyData);

            double[] bounds = polyData.GetBounds();
            //double[] bounds = imagePlaneWidgetX.GetTexturePlaneProperty().

            rendererPackageReslice.Renderer.ResetCamera(bounds[0], bounds[1], bounds[2], bounds[3], bounds[4], bounds[5]);
            //_rendererPackageResliceX.ResetCameraAndRefreshAll();
        }


    }

    internal enum VTKCURSOR
    {
        VTK_CURSOR_DEFAULT = 0,
        VTK_CURSOR_ARROW = 1,
        VTK_CURSOR_SIZENE = 2,
        VTK_CURSOR_SIZENW = 3,
        VTK_CURSOR_SIZESW = 4,
        VTK_CURSOR_SIZESE = 5,
        VTK_CURSOR_SIZENS = 6,
        VTK_CURSOR_SIZEWE = 7,
        VTK_CURSOR_SIZEALL = 8,
        VTK_CURSOR_HAND = 9,
        VTK_CURSOR_CROSSHAIR = 10
    }
}

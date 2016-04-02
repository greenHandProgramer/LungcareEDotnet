/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using System;
using System.IO;
using Kitware.VTK;
using System.Collections.Generic;
using System.Diagnostics;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    public static class VTKExtend
    {

        private static vtkImageData _cachedImageData;
        private static byte[] _cachedBuffer;
        
        public static void SetPosition(this vtkCamera camera, double[] point)
        {
            camera.SetPosition(point[0], point[1], point[2]);
        }

        public static void SetFocalPoint(this vtkCamera camera, double[] point)
        {
            camera.SetFocalPoint(point[0], point[1], point[2]);
        }

        public static void SetViewUp(this vtkCamera camera, double[] point)
        {
            camera.SetViewUp(point[0], point[1], point[2]);
        }

        internal static bool FileExists(this string vtpFileName)
        {
            return File.Exists(vtpFileName);
        }

        public static string ReplaceFolder(this string filename, string newFolder)
        {
            //string directoryName = filename.GetFullDirectoryName();
            FileInfo fi = new FileInfo(filename);
            return Path.Combine(newFolder, fi.Name);
        }

        public static double[] ReadMetaImageSpacing(this string mhdFileName)
        {
            if (!File.Exists(mhdFileName))
            {
                throw new ArgumentException(string.Format("File {0} does not exists.", mhdFileName));
            }
            if (!mhdFileName.EndsWith(".mhd"))
            {
                throw new ArgumentException(string.Format("File {0} should be a mhd file.", mhdFileName));
            }

            string[] lines = File.ReadAllLines(mhdFileName);
            foreach (var line in lines)
            {
                if (line.StartsWith("ElementSpacing"))
                {
                    // ElementSpacing = 0.703125 0.703125 1.25
                    string[] elements = line.Split(' ');
                    double spacingX = double.Parse(elements[2]);
                    double spacingY = double.Parse(elements[3]);
                    double spacingZ = double.Parse(elements[4]);

                    return new double[] { spacingX, spacingY, spacingZ };
                }
            }

            return null;
        }

        public static string ReplaceFileExtentionName(this string filename, string newExtentionName)
        {
            string directoryName = filename.GetFullDirectoryName();
            string _filename = Path.GetFileNameWithoutExtension(filename);
            string newFileName = string.Format(string.Format("{0}.{1}", _filename, newExtentionName));

            return Path.Combine(directoryName, newFileName);
        }

        public static string GetFullDirectoryName(this string filename)
        {
            return new FileInfo(filename).Directory.FullName;
        }

        public static string ReplaceFileName(this string filename, string newFileName)
        {
            string directoryName = filename.GetFullDirectoryName();
            return Path.Combine(directoryName, newFileName);
        }

        public static vtkImageData ReadMetaImage(this string mhdFileName)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            Stopwatch sw = Stopwatch.StartNew();

            File.ReadAllBytes(mhdFileName);
            if (File.Exists(mhdFileName.ReplaceFileName("i.raw")))
            {
                File.ReadAllBytes(mhdFileName.ReplaceFileName("i.raw"));
            }
            if (File.Exists(mhdFileName.ReplaceFileName("i.zraw")))
            {
                File.ReadAllBytes(mhdFileName.ReplaceFileName("i.zraw"));
            }

            vtkMetaImageReader metaImageReader = vtkMetaImageReader.New();
            metaImageReader.SetFileName(mhdFileName);
            metaImageReader.Update();

            vtkImageData readMetaImage = metaImageReader.GetOutput();

            metaImageReader.Dispose();
            metaImageReader = null;

            sw.Stop();
            //Console.WriteLine("Read {0} took {1} ms.", mhdFileName, sw.ElapsedMilliseconds);

            return readMetaImage;
        }

        internal static vtkPolyData ReadPolyData(this string vtpFileName)
        {
            if (string.IsNullOrEmpty(vtpFileName))
            {
                throw new Exception("Empty filename is given.");
            }
            if (!File.Exists(vtpFileName))
            {
                throw new FileNotFoundException(string.Format("{0} not found", vtpFileName), vtpFileName);
            }

            //File.ReadAllBytes(vtpFileName);

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
                return VTKUtil.ReadSTLPolyData(vtpFileName);
            }
            else
            {
                vtkXMLPolyDataReader reader = vtkXMLPolyDataReader.New();
                reader.SetFileName(vtpFileName);
                reader.Update();
                vtkPolyData polyData = reader.GetOutput();
                reader.Dispose();
                return polyData;
                //throw new Exception(string.Format("Unknown file type = " + vtpFileName));
            }
        }

        public static List<double[]> ReadPolyDataPoints(this string vtpFileName)
        {
            vtkPolyData polyData = ReadPolyData(vtpFileName);

            return ReadPolyDataPoints(polyData);
        }


        public static List<double[]> ReadPolyDataPoints(this vtkPolyData polyData)
        {
            List<double[]> ret = new List<double[]>();

            for (int i = 0; i < polyData.GetNumberOfPoints(); ++i)
            {
                ret.Add(polyData.GetPoint(i));
            }

            return ret;
        }


        public static List<List<double[]>> ReadPolyDataList(this string vtpFileName)
        {
            vtkPolyData polyData = ReadPolyData(vtpFileName);

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

            polyData.Dispose();

            return ret;
        }
    }
}

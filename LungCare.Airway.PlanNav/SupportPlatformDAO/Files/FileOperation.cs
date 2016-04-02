using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Kitware.VTK;
using System.Drawing.Imaging;
using System.Diagnostics;
using LungCare.Airway.WinformUIControls;
using System.Xml.Serialization;
using LungCare.SupportPlatform.SupportPlatformDAO.VMTKs;
using LungCare.SupportPlatform.SupportPlatformDAO.Logs;
namespace LungCare.SupportPlatform.SupportPlatformDAO.Files
{
    class FileOperation
    {
        public static vtkImageData ReadMetaImageData(string filePath)
        {
            vtkMetaImageReader reader = new vtkMetaImageReader();
            reader.SetFileName(filePath);
            
            reader.Update();

            vtkImageData imagedata = reader.GetOutput();
            reader.Dispose();

            return imagedata;
        }

        public static void WriteImageData(string imageDataPath , string connectiveAirwayVtpPath, string unConnectiveAirwayVTPFile , vtkImageData imagedata)
        {
            vtkMetaImageWriter write = new vtkMetaImageWriter();
            write.SetFileName(imageDataPath);
            write.SetInput(imagedata);
            write.Write();

            try
            {
                VMTKUtil.DoMarchingCubeConnectiveWithoutSmooth(imageDataPath, connectiveAirwayVtpPath, 1);
                VMTKUtil.DoMarchingCubeNotConnectiveWithoutSmooth(imageDataPath, unConnectiveAirwayVTPFile, 1);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                throw;
            }
           
            //vtkPolyData polyData = MarchingCubeImageData(imagedata, 1);

            //vtkXMLPolyDataWriter writepolyData = new vtkXMLPolyDataWriter();
            //writepolyData.SetFileName(airwayVtpPath);
            //writepolyData.SetInput(polyData);
            //writepolyData.Write();
        }

        public static void WriteImageData(string imageDataPath, vtkImageData imagedata)
        {
            vtkMetaImageWriter write = new vtkMetaImageWriter();
            write.SetFileName(imageDataPath);
            write.SetInput(imagedata);
            write.Write();
            write.Dispose();
        }

        public static void WriteImageData(string imageDataPath, string connectiveAirwayVtpPath, vtkImageData imagedata)
        {
            vtkMetaImageWriter write = new vtkMetaImageWriter();
            write.SetFileName(imageDataPath);
            write.SetInput(imagedata);
            write.Write();
            write.Dispose();
            try
            {
                VMTKUtil.DoMarchingCubeNotConnective(imageDataPath, connectiveAirwayVtpPath, 1);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                throw;
            }

            //vtkPolyData polyData = MarchingCubeImageData(imagedata, 1);

            //vtkXMLPolyDataWriter writepolyData = new vtkXMLPolyDataWriter();
            //writepolyData.SetFileName(airwayVtpPath);
            //writepolyData.SetInput(polyData);
            //writepolyData.Write();
        }
        // static string saveBitmapPath = Application.StartupPath + "\\save1.2.840.113619.2.55.3.336254977.198.1376723463.663";
        //static string saveBitmapPath = @"C:\Users\Administrator\Desktop\lung\segment1\orderBitmap8";
        //static string oldBitmapPath = Application.StartupPath + "\\temp";
        /// <summary>
        /// 通过FileStream 来打开文件，这样就可以实现不锁定Image文件，到时可以让多用户同时访问Image文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap ReadImageFile(string path)
        {
            FileStream fs = File.OpenRead(path); //OpenRead
            int filelength = 0;
            filelength = (int)fs.Length; //获得文件长度 
            Byte[] image = new Byte[filelength]; //建立一个字节数组 
            fs.Read(image, 0, filelength); //按字节流读取 
            System.Drawing.Image result = System.Drawing.Image.FromStream(fs);
            fs.Close();
            Bitmap bit = new Bitmap(result);
            return bit;
        }

        public static Bitmap GetChangedBitmap(string saveBitmapPath, int frame)
        {
            string name = frame.ToString().PadLeft(3, '0').ToString();
            string coverBitmapPath = saveBitmapPath + "\\" + name + ".bmp";

            if (File.Exists(coverBitmapPath))
            {
                Bitmap CoverBitmap = ReadImageFile(coverBitmapPath);
                Color color = CoverBitmap.GetPixel(1, 1);

                for (int i = 0; i < CoverBitmap.Width; i++)
                {
                    for (int j = 0; j < CoverBitmap.Height; j++)
                    {
                        if (CoverBitmap.GetPixel(i, j) != color)
                        {
                            CoverBitmap.SetPixel(i, j, Color.Red);
                        }
                    }
                }

                CoverBitmap.MakeTransparent(color);
                Console.WriteLine("use changed bitmap");
                return CoverBitmap;

            }
            return null;
            //else
            //{
            //    //coverBitmapPath = oldBitmapPath+"\\" + frame + ".bmp";
            //    Bitmap CoverBitmap = new Bitmap(coverBitmapPath);
            //    Color color = CoverBitmap.GetPixel(1, 1);

            //    for (int i = 0; i < CoverBitmap.Width; i++)
            //    {
            //        for (int j = 0; j < CoverBitmap.Height; j++)
            //        {
            //            if (CoverBitmap.GetPixel(i, j) != color)
            //            {
            //                CoverBitmap.SetPixel(i, j, Color.Red);
            //            }
            //        }
            //    }

            //    CoverBitmap.MakeTransparent(color);
            //    Console.WriteLine("use old bitmap");
            //    return CoverBitmap;

            //}
        }


        public static void ReplaceMHDValue(string fileName, string newValue)
        {
            string path = fileName;
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamReader reader = new StreamReader(fs);
            string content = reader.ReadToEnd();
            int startIndex = content.IndexOf("ElementSpacing");
            int endIndex = content.IndexOf("DimSize ");

            string selectedContent = content.Substring(startIndex + 17, endIndex - startIndex - 17);
            content = content.Replace(selectedContent, newValue + Environment.NewLine);
            Console.WriteLine(selectedContent);
            reader.Close();
            fs.Close();


            FileStream fsNew = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter write = new StreamWriter(fsNew);
            write.Write(content);
            write.Close();
            fsNew.Close();

        }



        public static vtkPolyData MarchingCubeImageData(vtkImageData imageData, double ISO)
        {
            // extract the skin
            vtkMarchingCubes _skinExtractor = new vtkMarchingCubes();
            _skinExtractor.SetInput(imageData);
            _skinExtractor.Update();
            Console.WriteLine("Extractor.Update(); Done");

            //_skinExtractor.SetValue(0, 155);
            _skinExtractor.SetValue(0, ISO);
            _skinExtractor.Update();

            vtkStripper _skinStripper = new vtkStripper();
            _skinStripper.SetInput(_skinExtractor.GetOutput());
            _skinStripper.Update();
            Console.WriteLine("Stripper.Update(); Done");

            vtkPolyData ret = _skinStripper.GetOutput();

            return ret;
        }
        public static Bitmap GetOldBitmap(int frame)
        {
            string coverBitmapPath = "oldBitmapPath" + "\\" + frame + ".bmp";
            Bitmap CoverBitmap = new Bitmap(coverBitmapPath);
            Color color = CoverBitmap.GetPixel(1, 1);

            for (int i = 0; i < CoverBitmap.Width; i++)
            {
                for (int j = 0; j < CoverBitmap.Height; j++)
                {
                    if (CoverBitmap.GetPixel(i, j) != color)
                    {
                        CoverBitmap.SetPixel(i, j, Color.Red);
                    }
                }
            }

            CoverBitmap.MakeTransparent(color);
            return CoverBitmap;
        }

        public static void BitmpToFormat8bppIndexed(string oldPath, string newPath)
        {
            DirectoryInfo dir = new DirectoryInfo(oldPath);
            FileInfo[] files = dir.GetFiles();
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            for (int i = 0; i < files.Count(); i++)
            {
                Bitmap bitmap = new Bitmap(files[i].FullName);
                Bitmap newBitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format8bppIndexed);
                string name = files[i].Name.Substring(0, files[i].Name.IndexOf(".")).ToString().PadLeft(3, '0');
                newBitmap.Save(newPath + "\\" + name + ".bmp", ImageFormat.Bmp);
                newBitmap.Dispose();
                bitmap.Dispose();
                File.Delete(files[i].FullName);
            }

            Directory.Delete(oldPath);
            //Process.Start(newPath);
        }

        public static void vtkMetaImageReaderMhdData(string fileName, string oldBitmapPath)
        {
            vtkMetaImageReader reader = new vtkMetaImageReader();
            reader.SetFileName(fileName);
            reader.Update();

            //vtkImageCast imageCast = new vtkImageCast();
            //imageCast.SetInputConnection(reader.GetOutputPort());
            //imageCast.SetOutputScalarTypeToUnsignedChar();
            //imageCast.Update();

            Console.WriteLine(reader.GetFilePrefix());
            if (!Directory.Exists(oldBitmapPath))
            {
                Directory.CreateDirectory(oldBitmapPath);
            }
            vtkBMPWriter write = new vtkBMPWriter();
            write.SetFilePrefix(oldBitmapPath + "\\");
            write.SetFilePattern("%s%d.bmp");
            write.SetInputConnection(reader.GetOutputPort());
            write.Write();

            //Process.Start(oldBitmapPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AlphabetNum">开头字母数</param>
        public static void ReNameFiles(int AlphabetNum, string path)
        {
            string fatherPath = path.Substring(0, path.LastIndexOf("\\"));
            string newFolderName = fatherPath + "\\orderFiles";
            if (!Directory.Exists(newFolderName))
            {
                Directory.CreateDirectory(newFolderName);
            }
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles();
            foreach (var item in files)
            {
                string newName = item.Name.Substring(AlphabetNum, item.Name.Length - AlphabetNum);
                newName = newName.PadLeft(3, '0');
                File.Copy(item.FullName, newFolderName + "\\" + newName);
            }

        }

        public static void Serialize<T>(T instance, string fileName)
        {
            TextWriter write = new StringWriter();
            XmlSerializer serialize = new XmlSerializer(typeof(T));
            serialize.Serialize(write, instance);
            File.WriteAllText(fileName, write.ToString());
        }

        public static T Deserialize<T>(string fileName)
        {
            TextReader reader = new StringReader(File.ReadAllText(fileName));
            XmlSerializer deSerialize = new XmlSerializer(typeof(T));
            return (T)deSerialize.Deserialize(reader);
        }

    }
}

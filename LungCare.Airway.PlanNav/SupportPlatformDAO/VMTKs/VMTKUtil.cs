using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Kitware.VTK;
using System.Windows;
using System.Threading;
namespace LungCare.SupportPlatform.SupportPlatformDAO.VMTKs
{
    class VMTKUtil
    {
        private static string VMTKFileName = System.Windows.Forms.Application.StartupPath + @"\vmtk 0.9.0\vmtk-startup-command-file.bat";
        static string commandFile = System.Windows.Forms.Application.StartupPath + @"\vmtk 0.9.0\command.txt";
        internal static void Validate()
        {
            if(File.Exists(VMTKFileName))
            {
                File.Delete(VMTKFileName);
            }
            if (!File.Exists(VMTKFileName))
            {
                if (!Directory.Exists(new FileInfo(VMTKFileName).Directory.FullName))
                {
                    Directory.CreateDirectory(new FileInfo(VMTKFileName).Directory.FullName);
                }
                string d = Path.GetPathRoot(VMTKFileName);
                d = d.Substring(0, d.IndexOf(@":\") + 1);
                string content = d+@"
                cd """ + System.Windows.Forms.Application.StartupPath + @"\vmtk 0.9.0""
                rem @echo off
                set VMTK_DIR=%~dp0
                set PATH=%VMTK_DIR%bin;%VMTK_DIR%lib\InsightToolkit;%VMTK_DIR%lib\Python;%PATH%
                set PYTHONPATH=%VMTK_DIR%lib\site-packages;%VMTK_DIR%lib\vtk-5.6;%VMTK_DIR%lib\vmtk
                cd %VMTK_DIR%
                python bin\vmtk-exe.py --file """ + System.Windows.Forms.Application.StartupPath + @"\vmtk 0.9.0\command.txt""";
                File.WriteAllText(VMTKFileName,
                    content);
            }
        }

        private static void Do(StringBuilder sb)
        {
            Validate();
            Console.WriteLine(sb.ToString());
            //File.WriteAllText(commandFile, sb.ToString().Replace(@"\", "/"));
            //创建文件流
            FileStream fs = new FileStream(commandFile, FileMode.OpenOrCreate, FileAccess.Write);
            //创建写入流
            StreamWriter sw = new StreamWriter(fs);
            //写入内容
            sw.WriteLine(sb.ToString().Replace(@"\", "/"));
            string content = sb.ToString().Replace(@"\", "/");
            //关闭流
            sw.Close();
            fs.Close();

            Console.WriteLine("after : " + sb.ToString().Replace(@"\", "/"));
            ProcessStartInfo psi = new ProcessStartInfo(VMTKFileName);
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(psi).WaitForExit();
        }

        internal static void DoMarchingCubeConnective(string segmentMhd, string _3dFile, double iso)
        {
            //segmentMhd ="\""+ segmentMhd+  "\"";
            //_3dFile = "\"" + _3dFile + "\"";
            Console.WriteLine(segmentMhd + "  " + _3dFile);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("vmtkmarchingcubes -ifile {0} -l {2} --pipe vmtksurfacesmoothing -passband 0.1 -iterations 30 --pipe vmtksurfaceconnectivity -ofile {1}", segmentMhd, _3dFile, Convert.ToString(iso)));
            Do(sb);
        }

        internal static void DoMarchingCubeNotConnective(string segmentMhd, string _3dFile, double iso)
        {
            //segmentMhd ="\""+ segmentMhd+  "\"";
            //_3dFile = "\"" + _3dFile + "\"";
            Console.WriteLine(segmentMhd + "  " + _3dFile);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("vmtkmarchingcubes -ifile {0} -l {2} --pipe vmtksurfacesmoothing -passband 0.1 -iterations 30 -ofile {1}", segmentMhd, _3dFile, Convert.ToString(iso)));
            Do(sb);
        }

        internal static void DoMarchingCubeConnectiveWithoutSmooth(string segmentMhd, string _3dFile, double iso)
        {
            //segmentMhd ="\""+ segmentMhd+  "\"";
            //_3dFile = "\"" + _3dFile + "\"";
            Console.WriteLine(segmentMhd + "  " + _3dFile);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("vmtkmarchingcubes -ifile {0} -l {2} --pipe vmtksurfaceconnectivity -ofile {1}", segmentMhd, _3dFile, Convert.ToString(iso)));
            Do(sb);
        }

        internal static void DoMarchingCubeNotConnectiveWithoutSmooth(string segmentMhd, string _3dFile, double iso)
        {
            //segmentMhd ="\""+ segmentMhd+  "\"";
            //_3dFile = "\"" + _3dFile + "\"";
            Console.WriteLine(segmentMhd + "  " + _3dFile);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("vmtkmarchingcubes -ifile {0} -l {2} -ofile {1}", segmentMhd, _3dFile, Convert.ToString(iso)));
            Do(sb);
        }


        public static void ShowPolydata(string airwayPath)
        {
            if (!File.Exists(airwayPath))
            {
                MessageBox.Show("文件不存在！");
                return;
            }
            vtkRenderWindow renWin = vtkRenderWindow.New();
            renWin.SetSize(600 , 600);

            vtkRenderWindowInteractor iren = new vtkRenderWindowInteractor();
            renWin.SetInteractor(iren);

            vtkInteractorStyleTrackballCamera interactorStyle = new vtkInteractorStyleTrackballCamera();
            iren.SetInteractorStyle(interactorStyle);

            vtkRenderer renderer = vtkRenderer.New();
            renderer.GradientBackgroundOn();
            renderer.SetBackground(0, 0, 0);
            renderer.SetBackground2(0, 0, 1);
            renWin.AddRenderer(renderer);
            vtkXMLPolyDataReader reader = new vtkXMLPolyDataReader();
            reader.SetFileName(airwayPath);
            reader.Update();

            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            mapper.ScalarVisibilityOff();
            mapper.SetInput(reader.GetOutput());
            reader.Dispose();
            vtkActor actor = new vtkActor();
            actor.SetMapper(mapper);

            renderer.AddActor(actor);

            renWin.Render();

        }

        public static void ShowPolydata(string airwayPath , string lesionPath)
        {
            vtkAppendPolyData appendPolydata = new vtkAppendPolyData();
            if (File.Exists(airwayPath))
            {
                vtkXMLPolyDataReader reader = new vtkXMLPolyDataReader();
                reader.SetFileName(airwayPath);
                reader.Update();
                appendPolydata.AddInput(reader.GetOutput());
                reader.Dispose();
                
            }
            else
            {
                MessageBox.Show("airwayPath : " + airwayPath + "文件不存在！");
            }

            if (File.Exists(lesionPath))
            {
                vtkXMLPolyDataReader reader = new vtkXMLPolyDataReader();
                reader.SetFileName(lesionPath);
                reader.Update();
                appendPolydata.AddInput(reader.GetOutput());
                reader.Dispose();

            }
            else
            {
                MessageBox.Show("lesionPath : " + lesionPath + "文件不存在！");
            }

            vtkRenderWindow renWin = vtkRenderWindow.New();
            renWin.SetSize(600, 600);

            vtkRenderWindowInteractor iren = new vtkRenderWindowInteractor();
            renWin.SetInteractor(iren);

            vtkInteractorStyleTrackballCamera interactorStyle = new vtkInteractorStyleTrackballCamera();
            iren.SetInteractorStyle(interactorStyle);

            vtkRenderer renderer = vtkRenderer.New();
            renderer.GradientBackgroundOn();
            renderer.SetBackground(0, 0, 0);
            renderer.SetBackground2(0, 0, 1);
            renWin.AddRenderer(renderer);
           

            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            mapper.ScalarVisibilityOff();
            mapper.SetInput(appendPolydata.GetOutput());
            vtkActor actor = new vtkActor();
            actor.SetMapper(mapper);

            renderer.AddActor(actor);

            renderer.Render();
            renderer.ResetCamera();
            renWin.Render();

            //Thread thread = new Thread(new ThreadStart(
            //                                       delegate
            //                                       {
            //                                           iren.Start();
            //                                       }));
            //thread.Start();

        }
    }
}

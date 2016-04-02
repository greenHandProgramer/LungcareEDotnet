/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using System;
using Kitware.VTK;
using System.IO;
using System.Windows.Forms;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    public class XmlPolyDataPackage
    {
        public vtkPolyData PolyData;
        vtkActor actor;
        vtkXMLPolyDataReader reader;

        vtkPolyDataMapper _polyDataMapper;

        public vtkActor Actor
        {
            get
            {
                return actor;
            }
        }

        public void SetColor(double r, double g, double b)
        {
            double[] color = actor.GetProperty().GetColor();
            if (color[0] == r && color[1] == g && color[2] == b) { return; }
            actor.GetProperty().SetColor(r, g, b);
        }

        public void SetOpacity(double opacity)
        {
            actor.GetProperty().SetOpacity(opacity);
        }

        private vtkRenderer renderer;
        
        public void 粉色()
        {
            //SetColor(251.0 / 255.0, 185.0 / 255.0, 223.0 / 255.0);
            SetColor(246.0 / 255.0, 153.0 / 255.0, 180.0 / 255.0);
        }

        public void 红色()
        {
            //SetColor(251.0 / 255.0, 185.0 / 255.0, 223.0 / 255.0);
            SetColor(255.0 / 255.0, 0 / 255.0, 0 / 255.0);
        }

        public void 黄色()
        {
            //SetColor(251.0 / 255.0, 185.0 / 255.0, 223.0 / 255.0);
            SetColor(255.0 / 255.0, 255.0 / 255.0 , 0/255.0);
        }

        public void 正面观()
        {
            vtkCamera camera = renderer.GetActiveCamera();
            double[] bound = actor.GetBounds();

            camera.SetFocalPoint(
                (bound[0] + bound[1]) / 2,
                (bound[2] + bound[3]) / 2,
                (bound[4] + bound[5]) / 2);

            camera.SetPosition(
                (bound[0] + bound[1]) / 2,
                1000,
                (bound[4] + bound[5]) / 2);

            camera.SetViewUp(0, 0, -1);
        }

        public XmlPolyDataPackage(string vtpFile, vtkRenderer renderer)
        {
            //Console.WriteLine(string.Format("VTP文件 = {0}", vtpFile));

            if (File.Exists(vtpFile))
            {
                if (vtpFile.EndsWith(".vtp"))
                {
                    //Console.WriteLine(string.Format("VTP文件 = {0}已存在", vtpFile));
                    reader = vtkXMLPolyDataReader.New();
                    reader.SetFileName(vtpFile);
                    reader.Update();

                    PolyData = reader.GetOutput();
                }
                else if (vtpFile.EndsWith(".stl"))
                {
                    PolyData = VTKUtil.ReadSTLPolyData(vtpFile);
                }

                vtkCamera camera = vtkCamera.New();

                var sortPolydata = new vtkDepthSortPolyData();
                sortPolydata.SetInput(PolyData);
                sortPolydata.SetCamera(camera);
                sortPolydata.SetDirectionToFrontToBack();
                sortPolydata.SetVector(1, 1, 1);
                sortPolydata.SetSortScalars(1);
                sortPolydata.Update();

                _polyDataMapper = vtkPolyDataMapper.New();
                _polyDataMapper.ScalarVisibilityOff();
                _polyDataMapper.SetInput(sortPolydata.GetOutput());
                _polyDataMapper.Update();

                actor = vtkActor.New();
                actor.SetMapper(_polyDataMapper);
                actor.GetProperty().SetColor(1, 1, 1);

                //marchingCubeActor.GetProperty().BackfaceCullingOn();
                //marchingCubeActor.GetProperty().FrontfaceCullingOn();
                //marchingCubeActor.GetProperty().BackfaceCullingOff();

                // now, tell the renderer our actors
                renderer.AddActor(actor);

                this.renderer = renderer;
            }
            else
            {
                throw new Exception(string.Format("VTP文件 = {0}不存在。", vtpFile));
            }

            //正面观();
        }


        public void Dispose()
        {
            if (actor != null)
            {
                actor.Dispose();
                _polyDataMapper.Dispose();
                reader.Dispose();
                
            }
        }
        public void ScalarVisibilityOff()
        {
            _polyDataMapper.ScalarVisibilityOff();
            _polyDataMapper.Modified();
            actor.Modified();
        }

        public void ScalarVisibilityOn()
        {
            _polyDataMapper.ScalarVisibilityOn();
            _polyDataMapper.Modified();
            actor.Modified();
        }

        public XmlPolyDataPackage(vtkPolyData polyData, vtkRenderer renderer)
        {
            this.PolyData = polyData;
            _polyDataMapper = vtkPolyDataMapper.New();
            _polyDataMapper.ScalarVisibilityOff();
            _polyDataMapper.SetInput(PolyData);
            _polyDataMapper.Update();

            actor = vtkActor.New();
            actor.SetMapper(_polyDataMapper);

            //marchingCubeActor.GetProperty().BackfaceCullingOn();
            //marchingCubeActor.GetProperty().FrontfaceCullingOn();
            //marchingCubeActor.GetProperty().BackfaceCullingOff();

            // now, tell the renderer our actors
            renderer.AddActor(actor);

            this.renderer = renderer;
        }

        public void ReplacePolyData(vtkPolyData polyData)
        {
            this.PolyData = polyData;

            _polyDataMapper.SetInput(polyData);
            _polyDataMapper.Modified();
        }
        
        public void VisibilityOn()
        {
            if (actor != null)
            {
                if (actor.GetVisibility() == 0)
                {
                    actor.VisibilityOn();
                }
            }
        }

        public void VisibilityOff()
        {
            if (actor != null)
            {
                if (actor.GetVisibility() != 0)
                {
                    actor.VisibilityOff();
                }
            }
        }

        public event EventHandler<UserMatrixUpdatedEventArgs> UserMatrixUpdated;
        
        public void SetVisibility(object sender)
        {
            bool visible = ((System.Windows.Controls.Primitives.ToggleButton)sender).IsChecked.Value;
            if (visible)
            {
                VisibilityOn();
            }
            else
            {
                VisibilityOff();
            }
        }
    }

    public class UserMatrixUpdatedEventArgs : EventArgs
    {
        public vtkMatrix4x4 Matrix;
    }
}

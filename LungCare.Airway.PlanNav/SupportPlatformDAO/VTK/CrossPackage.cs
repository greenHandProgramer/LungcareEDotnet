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
    public class CrossPackage : IVisibilityPackage
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

        public void SetClippingPlane(vtkPlane clippingPlane)
        {
            RemoveAllClippingPlanes();
            _polyDataMapper.AddClippingPlane(clippingPlane);
        }

        public void RemoveMe()
        {
            if (actor != null)
            {
                _renderer.RemoveActor(actor);

                if (reader != null) reader.Dispose();
                PolyData.Dispose();
                _polyDataMapper.Dispose();
                actor.Dispose();
            }
            actor = null;
        }

        public void RemoveAllClippingPlanes()
        {
            _polyDataMapper.RemoveAllClippingPlanes();
        }

        private readonly vtkRenderer _renderer;

        public double[] Position { get; set; }
        public void SetPosition(double[] position)
        {
            if (position == null)
            {
                return;
            }
            this.Position = position;

            vtkTransform transform = vtkTransform.New();
            transform.Translate(position[0], position[1], position[2]);

            SetUserTransform(transform: transform);

            transform.Dispose();
        }

        public CrossPackage(double size, vtkRenderer renderer)
        {
            //Console.WriteLine(string.Format("VTP文件 = {0}", vtpFile));
            vtkPolyData cursorPolyData = vtkPolyData.New();

            vtkPoints cursorPts = vtkPoints.New();
            cursorPts.SetNumberOfPoints(12);

            cursorPts.SetPoint(0, -size, -0, 0);
            cursorPts.SetPoint(1, size, -0, 0);
            cursorPts.SetPoint(2, size, 0, 0);
            cursorPts.SetPoint(3, -size, 0, 0);

            cursorPts.SetPoint(4, -0, size, 0);
            cursorPts.SetPoint(5, 0, size, 0);
            cursorPts.SetPoint(6, 0, -size, 0);
            cursorPts.SetPoint(7, -0, -size, 0);

            cursorPts.SetPoint(8, -0, 0, size);
            cursorPts.SetPoint(9, 0, 0, size);
            cursorPts.SetPoint(10, 0, 0, -size);
            cursorPts.SetPoint(11, -0, 0, -size);

            vtkCellArray cells = vtkCellArray.New();

            cells.InsertNextCell(3, VTKUtil.ConvertIntPtr(new long[] { 0, 1, 2 }));
            cells.InsertNextCell(3, VTKUtil.ConvertIntPtr(new long[] { 1, 2, 3 }));
            cells.InsertNextCell(3, VTKUtil.ConvertIntPtr(new long[] { 4, 5, 6 }));
            cells.InsertNextCell(3, VTKUtil.ConvertIntPtr(new long[] { 5, 6, 7 }));
            cells.InsertNextCell(3, VTKUtil.ConvertIntPtr(new long[] { 8, 9, 10 }));
            cells.InsertNextCell(3, VTKUtil.ConvertIntPtr(new long[] { 9, 10, 11 }));

            vtkCellArray lines = vtkCellArray.New();
            lines.InsertNextCell(2, VTKUtil.ConvertIntPtr(new long[] { 0, 1 }));
            lines.InsertNextCell(2, VTKUtil.ConvertIntPtr(new long[] { 6, 5 }));
            lines.InsertNextCell(2, VTKUtil.ConvertIntPtr(new long[] { 10, 9 }));

            cursorPolyData.SetPoints(cursorPts);
            cursorPolyData.SetVerts(cells);
            //CursorPolyData.SetPolys(cells);
            cursorPolyData.SetLines(lines);
            cursorPolyData.Update();

            PolyData = cursorPolyData;

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

            this._renderer = renderer;
        }

        public void ShadingOff()
        {
            Actor.GetProperty().ShadingOff();
            Actor.GetProperty().SetAmbient(1);
        }

        public void ScalarVisibilityOn()
        {
            _polyDataMapper.ScalarVisibilityOn();
            _polyDataMapper.Modified();
            actor.Modified();
        }

        public void ReplacePolyData(vtkPolyData polyData)
        {
            this.PolyData = polyData;

            _polyDataMapper.SetInput(polyData);
            _polyDataMapper.Modified();
        }

        public void RotateX(double p)
        {
            actor.RotateX(p);
        }

        public void RotateY(double p)
        {
            actor.RotateY(p);
        }

        public void RotateZ(double p)
        {
            actor.RotateZ(p);
        }

        public void ResetMatrix()
        {
            vtkMatrix4x4 matrix = actor.GetMatrix();
            matrix.Zero();
            matrix.SetElement(0, 0, 1);
            matrix.SetElement(1, 1, 1);
            matrix.SetElement(2, 2, 1);
            matrix.SetElement(3, 3, 1);
        }

        public void SwitchVisibility()
        {
            if (actor != null)
            {
                if (actor.GetVisibility() == 0)
                {
                    actor.VisibilityOn();
                }
                else
                {
                    actor.VisibilityOff();
                }
            }
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

        public void SetUserTransform(vtkTransform transform)
        {
            actor.SetUserTransform(transform);
        }

        public vtkMatrix4x4 GetMatrix()
        {
            return actor.GetMatrix();
        }

        public vtkLinearTransform GetUserTransform()
        {
            return actor.GetUserTransform();
        }

        public void ApplyTransform(vtkMatrix4x4 matrix)
        {
            vtkMatrix4x4 t = vtkMatrix4x4.New();

            vtkMatrix4x4.Multiply4x4(actor.GetMatrix(), matrix, t);

            vtkTransform transform = vtkTransform.New();
            transform.SetMatrix(t);

            this.actor.SetUserTransform(transform);
        }

        public void ApplyTransformBefore(vtkMatrix4x4 matrix)
        {
            vtkMatrix4x4 t = vtkMatrix4x4.New();

            vtkMatrix4x4.Multiply4x4(matrix, actor.GetMatrix(), t);

            vtkTransform transform = vtkTransform.New();
            transform.SetMatrix(t);

            this.actor.SetUserTransform(transform);
        }


        #region IVisibilityPackage Members

        void IVisibilityPackage.SwitchVisibility()
        {
            if (actor != null)
            {
                if (actor.GetVisibility() == 0)
                {
                    actor.VisibilityOn();
                }
                else
                {
                    actor.VisibilityOff();
                }
            }
        }

        #endregion

        public event EventHandler<UserMatrixUpdatedEventArgs> UserMatrixUpdated;

        public void SetTransform(vtkMatrix4x4 matrix)
        {
            if (actor == null) { return; }

            vtkTransform transform = vtkTransform.New();
            transform.SetMatrix(matrix);

            this.actor.SetUserTransform(transform);
        }

        public void AttachOpacityTrackbar(TrackBar trackBar)
        {
            trackBar.Minimum = 0;
            trackBar.Maximum = 100;
            trackBar.Scroll += new EventHandler(trackBar_Scroll);
            trackBar.ValueChanged += new EventHandler(trackBar_Scroll);
        }

        void trackBar_Scroll(object sender, EventArgs e)
        {
            TrackBar trackBar = (TrackBar)sender;
            SetOpacity(trackBar.Value / 100f);
        }

        public void AttachAutoRefreshTrackbar(TrackBar trackBar, RendererPackage rendererPackage)
        {
            trackBar.Scroll += new EventHandler(delegate(object sender, EventArgs e) { rendererPackage.RefreshAll(); });
            trackBar.ValueChanged += new EventHandler(delegate(object sender, EventArgs e) { rendererPackage.RefreshAll(); });
            //trackBar.ValueChanged += new EventHandler(trackBar_refreseh_Scroll);
        }
    }
}

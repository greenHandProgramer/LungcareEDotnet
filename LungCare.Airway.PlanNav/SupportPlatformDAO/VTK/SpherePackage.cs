/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using System;
using Kitware.VTK;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    public class SpherePackage
    {
        public event EventHandler PositionChanged;

        public vtkActor Actor
        {
            get
            {
                return _sphereActor;
            }
        }

        readonly vtkSphereSource _sphereSource;
        readonly vtkActor _sphereActor;
        readonly vtkFollower _textActor;
        readonly vtkVectorText _aText;
        private readonly vtkRenderer _aRender;
        public vtkPolyDataMapper SphereMapper;

        public void RandColor()
        {
            SetColor(vtkMath.Random(0.5, 1), vtkMath.Random(0.5, 1), vtkMath.Random(0.5, 1));
        }
        
        public SpherePackage(vtkRenderer aRender)
        {
            _aRender = aRender;

            _sphereSource = vtkSphereSource.New();
            _sphereSource.SetRadius(3);
            _sphereSource.SetRadius(0.5);
            _sphereSource.SetThetaResolution(26);
            _sphereSource.SetPhiResolution(26);
            _sphereSource.ModifiedEvt += _sphereSource_ModifiedEvt;

            SphereMapper = vtkPolyDataMapper.New();
            SphereMapper.SetInputConnection(_sphereSource.GetOutputPort());

            _sphereActor = vtkActor.New();
            _sphereActor.SetMapper(SphereMapper);
            aRender.AddActor(_sphereActor);

            _aText = vtkVectorText.New();
            _aText.SetText("");

            vtkPolyDataMapper textMapper = vtkPolyDataMapper.New();
            textMapper.SetInputConnection(_aText.GetOutputPort());
            _textActor = vtkFollower.New();
            //textActor.GetProperty().SetColor(point.Color.X, point.Color.Y, point.Color.Z);
            _textActor.SetMapper(textMapper);
            //textActor.SetScale(0.2, 0.2, 0.2);
            //textActor.SetScale(4);
            _textActor.SetCamera(aRender.GetActiveCamera());
            aRender.AddActor(_textActor);

            //SetOpacity(0.5f);

            //VisOff();

            RandColor();
        }

        void _sphereSource_ModifiedEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            _sphereSource.Update();
            SpherePolyData = _sphereSource.GetOutput();
        }

        public void SetResolution(int a, int b)
        {
            _sphereSource.SetPhiResolution(a);
            _sphereSource.SetThetaResolution(b);
        }

        public void SetRadius(double radius)
        {
            _sphereSource.SetRadius(radius);
        }

        public vtkPolyData SpherePolyData;

        public void SetPosition(double[] xyz)
        {
            if (_sphereSource.GetCenter()[0] == 0 && _sphereSource.GetCenter()[1] == 0 && _sphereSource.GetCenter()[2] == 0)
            {
                VisibilityOn();
            }

            _sphereSource.SetCenter(xyz[0], xyz[1], xyz[2]);
            _sphereSource.Modified();

            if (_textActor != null) _textActor.SetPosition(xyz[0] + 5, xyz[1] + 5, xyz[2] + 5);
            _textActor.SetPosition(xyz[0] + 1, xyz[1] + 1, xyz[2] + 1);

            if (PositionChanged != null)
            {
                PositionChanged(this, new EventArgs());
            }
        }

        public void SetColor(double r, double g, double b)
        {
            _sphereActor.GetProperty().SetColor(r, g, b);
            if (_textActor != null)
            {
                _textActor.GetProperty().SetColor(r, g, b);
            }
        }

        public void SetName(string name)
        {
            _aText.SetText(name);
        }

        public void SetOpacity(float opacity)
        {
            _sphereActor.GetProperty().SetOpacity(opacity);
        }

        #region IDisposable Members

        public void Dispose()
        {
            VTKUtil.DisposeVtkObject(_sphereActor);
            VTKUtil.DisposeVtkObject(_sphereSource);
            VTKUtil.DisposeVtkObject(_aText);
        }

        #endregion

        public void VisibilityOff()
        {
            _sphereActor.VisibilityOff();
            if (_textActor != null) _textActor.VisibilityOff();
        }

        public void VisibilityOn()
        {
            _sphereActor.VisibilityOn();
            if (_textActor != null) _textActor.VisibilityOn();
        }
    }
}

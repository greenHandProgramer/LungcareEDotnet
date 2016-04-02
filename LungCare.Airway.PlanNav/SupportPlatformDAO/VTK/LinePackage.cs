/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using Kitware.VTK;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    public class LinePackage: IVisibilityPackage
    {
        public vtkLineSource line;

        private readonly vtkActor _lineActor;
        private readonly RendererPackage _rendererPackage;
        private readonly vtkRenderer _renderer;

        public LinePackage(RendererPackage rendererPackage, double[] p1, double[] direction, double length)
            : this(rendererPackage)
        {
            if (p1 != null && direction != null) 
            {
                double[] p2 = VTKUtil.Extend2(p1, VTKUtil.Add(p1, direction), length);
                SetPosition(p1, p2); 
            }
        }

        public LinePackage(RendererPackage rendererPackage, double[] p1, double[] p2)
            : this(rendererPackage)
        {
            if (p1 != null && p2 != null) { SetPosition(p1, p2); }
        }

        public vtkActor Actor
        {
            get { return _lineActor; }
        }

        public LinePackage(vtkRenderer renderer)
        {
            line = vtkLineSource.New();
            line.SetResolution(100);

            vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
            sphereMapper.SetInput(line.GetOutput());
            _lineActor = vtkActor.New();

            _lineActor.SetMapper(sphereMapper);
            renderer.AddActor(_lineActor);

            _renderer = renderer;
        }

        public LinePackage(RendererPackage rendererPackage)
        {
            line = vtkLineSource.New();
            line.SetResolution(100);
            
            vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
            sphereMapper.SetInput(line.GetOutput());
            _lineActor = vtkActor.New();
            
            _lineActor.SetMapper(sphereMapper);
            rendererPackage.Renderer.AddActor(_lineActor);

            this._rendererPackage = rendererPackage;
        }

        public void SetPosition(double[] xyz1, double[] xyz2)
        {
            line.SetPoint1(xyz1[0], xyz1[1], xyz1[2]);
            line.SetPoint2(xyz2[0], xyz2[1], xyz2[2]);
            //lineActor.SetPosition(xyz[0] + 5, xyz[1] + 5, xyz[2] + 5);
            this.xyz1 = xyz1;
            this.xyz2 = xyz2;
        }

        public void SetPosition(double[] xyz1, double[] xyz2,double length)
        {
            SetPosition(xyz1, VTKUtil.Extend2(xyz1, xyz2, length));
        }

        private double[] xyz1, xyz2;
        public double[] GetLinePoint1()
        {
            return xyz1;
        }

        public double[] GetLinePoint2()
        {
            return xyz2;
        }

        public void SetColor(double r, double g, double b)
        {
            _lineActor.GetProperty().SetColor(r, g, b);
        }

        public void SetSize(double size)
        {
            _lineActor.SetScale(size);
        }

        public void RemoveMe()
        {
            if (_rendererPackage != null)
            {
                _rendererPackage.RemoveActor(_lineActor);
            }
            if (_renderer != null)
            {
                _renderer.RemoveActor(_lineActor);
            }

            _lineActor.Dispose();
            line.Dispose();
        }

        public void SetTransform(vtkMatrix4x4 matrix)
        {
            vtkTransform transform = vtkTransform.New();
            transform.SetMatrix(matrix);

            //this.lineActor.SetUserTransform(transform);
            //return;

            double[] linePoint1 = GetLinePoint1();
            double[] linePoint2 = GetLinePoint2();

            linePoint1 = xyz1;
            linePoint2 = xyz2;

            double[] p1 = matrix.MultiplyDoublePoint(VTKUtil.ConvertTo4P(linePoint1));
            double[] p2 = matrix.MultiplyDoublePoint(VTKUtil.ConvertTo4P(linePoint2));

            line.SetPoint1(p1[0], p1[1], p1[2]);
            line.SetPoint2(p2[0], p2[1], p2[2]);
        }

        public vtkMatrix4x4 GetTransform()
        {
            return VTKUtil.OneMatrix;

            return this._lineActor.GetUserTransform().GetMatrix();
        }

        #region IVisibilityPackage Members

        public void VisibilityOff()
        {
            if (_lineActor != null)
            {
                if (_lineActor.GetVisibility() == 0)
                {
                }
                else
                {
                    _lineActor.VisibilityOff();
                }
            }
        }

        public void VisibilityOn()
        {
            if (_lineActor != null)
            {
                if (_lineActor.GetVisibility() == 0)
                {
                    _lineActor.VisibilityOn();
                }
            }
        }

        public void SwitchVisibility()
        {
            if (_lineActor != null)
            {
                if (_lineActor.GetVisibility() == 0)
                {
                    _lineActor.VisibilityOn();
                }
                else
                {
                    _lineActor.VisibilityOff();
                }
            }
        }

        #endregion

        public void WorldToView(out double x1, out double y1, out double x2, out double y2)
        {
            double[] linePoint1 = GetLinePoint1();
            double[] linePoint2 = GetLinePoint2();

            double[] p1 = GetTransform().MultiplyDoublePoint(VTKUtil.ConvertTo4P(linePoint1));
            double[] p2 = GetTransform().MultiplyDoublePoint(VTKUtil.ConvertTo4P(linePoint2));

            p1 = line.GetPoint1();
            p2 = line.GetPoint2();

            VTKUtil.WorldToView(p1, out x1, out y1, _rendererPackage);
            VTKUtil.WorldToView(p2, out x2, out y2, _rendererPackage);
        }

        public void SetTopMost()
        {
            vtkRenderer _aRender = _rendererPackage.Renderer;

            if (_aRender.GetLayer() > 0)
            {
                return;
            }

            if (_aRender.GetRenderWindow().GetRenderers().GetNumberOfItems() == 1)
            {
                vtkRenderer ren1 = vtkRenderer.New(); // 2d actor        
                ren1.SetLayer(1); // top layer    
                ren1.InteractiveOff();

                _aRender.SetLayer(0);
                _aRender.GetRenderWindow().SetNumberOfLayers(2);
                _aRender.GetRenderWindow().AddRenderer(ren1);

                //ren1.SetActiveCamera(_aRender.GetActiveCamera());

                //new SpherePackage(ren1, new double[] { 0, 0, 0 }, 600).SetOpacity(0.01f);

                _aRender.SetActiveCamera(ren1.GetActiveCamera());

                _aRender.RemoveActor(Actor);
                ren1.AddActor(Actor);

                _aRender.InteractiveOn();
            }
            else
            {
                for (int i = 0; i < _aRender.GetRenderWindow().GetRenderers().GetNumberOfItems(); ++i)
                {
                    int layer = vtkRenderer.SafeDownCast(_aRender.GetRenderWindow().GetRenderers().GetItemAsObject(i)).GetLayer();

                    if (layer == 1)
                    {
                        vtkRenderer.SafeDownCast(_aRender.GetRenderWindow().GetRenderers().GetItemAsObject(i)).AddActor(Actor);
                    }
                    else
                    {
                        vtkRenderer.SafeDownCast(_aRender.GetRenderWindow().GetRenderers().GetItemAsObject(i)).RemoveActor(Actor);
                    }
                }
            }
        }
    }
}

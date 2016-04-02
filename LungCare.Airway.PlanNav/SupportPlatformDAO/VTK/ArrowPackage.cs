/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using Kitware.VTK;
using System;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    public class ArrowPackage2
    {
        private vtkPolyDataMapper _mapper;
        internal vtkActor _arrowActor;
        private vtkArrowSource _arrowSource;
        private vtkRenderer _topRenderer;

        internal void SetOpacity(double opacity)
        {
            _arrowActor.GetProperty().SetOpacity(opacity);    
        }

        static void Subtract(double[] a, double[] b, double[] c)
        {
            for (int i = 0; i < 3; ++i)
                c[i] = a[i] - b[i];
        }

        public void VisibilityOff()
        {
            _arrowActor.VisibilityOff();
        }

        public void VisibilityOn()
        {
            _arrowActor.VisibilityOn();
        }

        public void SetColor(double r, double g, double b)
        {
            _arrowActor.GetProperty().SetColor(r, g, b);
        }

        public ArrowPackage2(double[] startPoint, double[] endPoint, vtkRenderer renderer)
        {
            this._topRenderer = renderer;

            //Create an arrow.
            _arrowSource = vtkArrowSource.New();
            _arrowSource.SetShaftRadius(0.02);
            _arrowSource.SetTipRadius(0.04);

            _arrowSource.SetShaftRadius(0.05);
            _arrowSource.SetTipRadius(0.1);

            //_arrowSource.SetShaftRadius(2);
            //_arrowSource.SetTipRadius(4);

            //_arrowSource.SetTipLength(10);
            _arrowSource.SetShaftResolution(100);
            _arrowSource.SetTipResolution(100);

            //Create a mapper and actor for the arrow
            _mapper = vtkPolyDataMapper.New();
            _arrowActor = vtkActor.New();

            // Generate a random start and end point
            // Compute a basis
            UpdatePosition(endPoint, startPoint);

            // Create spheres for start and end point
            //vtkSphereSource sphereStartSource = new vtkSphereSource();
            //sphereStartSource.SetCenter(VTKUtil.ConvertIntPtr(startPoint));
            //vtkPolyDataMapper sphereStartMapper = vtkPolyDataMapper.New();
            //sphereStartMapper.SetInputConnection(sphereStartSource.GetOutputPort());
            //vtkActor sphereStart = new vtkActor();
            //sphereStart.SetMapper(sphereStartMapper);
            //sphereStart.GetProperty().SetColor(1.0, 1.0, .3);

            //vtkSphereSource sphereEndSource = new vtkSphereSource();
            //sphereEndSource.SetCenter(VTKUtil.ConvertIntPtr(endPoint));
            //vtkPolyDataMapper sphereEndMapper = vtkPolyDataMapper.New();
            //sphereEndMapper.SetInputConnection(sphereEndSource.GetOutputPort());
            //vtkActor sphereEnd = new vtkActor();
            //sphereEnd.SetMapper(sphereEndMapper);
            //sphereEnd.GetProperty().SetColor(1.0, .3, .3);

            //_arrowSource.Update();
            //_mapper.SetInput(_arrowSource.GetOutput());
            //_arrowActor.SetMapper(_mapper);

            _arrowActor.GetProperty().SetOpacity(0.2);
            _arrowActor.GetProperty().SetColor(0, 1, 0);

            _topRenderer.AddActor(_arrowActor);
            //rendererPackage.AddActor(sphereStart);
            //rendererPackage.AddActor(sphereEnd);
        }

        public ArrowPackage2(double[] startPoint, double[] endPoint, RendererPackage rendererPackage)
        {
            int oldNumberOfRenderer = rendererPackage.RenderWindow.GetNumberOfLayers();
            Console.WriteLine(string.Format("oldNumberOfRenderer = {0}", oldNumberOfRenderer));
            int newNumberOfRenderer = oldNumberOfRenderer + 1;

            rendererPackage.Renderer.SetLayer(0);

            _topRenderer = vtkRenderer.New();
            _topRenderer.SetViewport(0, 0, 1, 1);
            _topRenderer.SetLayer(newNumberOfRenderer - 1);
            _topRenderer.InteractiveOff();

            //new XmlPolyDataPackage(polyData, _topRenderer).SetColor(1, 0, 0);

            rendererPackage.RenderWindow.SetNumberOfLayers(newNumberOfRenderer);
            rendererPackage.RenderWindow.AddRenderer(_topRenderer);

            //Create an arrow.
            _arrowSource = vtkArrowSource.New();
            _arrowSource.SetShaftRadius(0.02);
            _arrowSource.SetTipRadius(0.04);

            _arrowSource.SetShaftRadius(0.05);
            _arrowSource.SetTipRadius(0.1);

            //_arrowSource.SetShaftRadius(2);
            //_arrowSource.SetTipRadius(4);
            
            //_arrowSource.SetTipLength(10);
            _arrowSource.SetShaftResolution(100);
            _arrowSource.SetTipResolution(100);

            //Create a mapper and actor for the arrow
            _mapper = vtkPolyDataMapper.New();
            _arrowActor = vtkActor.New();

            // Generate a random start and end point
            // Compute a basis
            UpdatePosition(endPoint, startPoint);

            // Create spheres for start and end point
            //vtkSphereSource sphereStartSource = new vtkSphereSource();
            //sphereStartSource.SetCenter(VTKUtil.ConvertIntPtr(startPoint));
            //vtkPolyDataMapper sphereStartMapper = vtkPolyDataMapper.New();
            //sphereStartMapper.SetInputConnection(sphereStartSource.GetOutputPort());
            //vtkActor sphereStart = new vtkActor();
            //sphereStart.SetMapper(sphereStartMapper);
            //sphereStart.GetProperty().SetColor(1.0, 1.0, .3);

            //vtkSphereSource sphereEndSource = new vtkSphereSource();
            //sphereEndSource.SetCenter(VTKUtil.ConvertIntPtr(endPoint));
            //vtkPolyDataMapper sphereEndMapper = vtkPolyDataMapper.New();
            //sphereEndMapper.SetInputConnection(sphereEndSource.GetOutputPort());
            //vtkActor sphereEnd = new vtkActor();
            //sphereEnd.SetMapper(sphereEndMapper);
            //sphereEnd.GetProperty().SetColor(1.0, .3, .3);

            //_arrowSource.Update();
            //_mapper.SetInput(_arrowSource.GetOutput());
            //_arrowActor.SetMapper(_mapper);

            _arrowActor.GetProperty().SetOpacity(0.2);
            _arrowActor.GetProperty().SetColor(0, 1, 0);

            _topRenderer.AddActor(_arrowActor);
            //rendererPackage.AddActor(sphereStart);
            //rendererPackage.AddActor(sphereEnd);
        }

        public void UpdatePosition(double[] endPoint, double[] startPoint, double length)
        {
            double[] newEndPoint = VTKUtil.Extend(endPoint, startPoint, -length);
            UpdatePosition(newEndPoint, startPoint);
        }

        public void UpdatePosition(double[] endPoint, double[] startPoint)
        {
            double[] normalizedX = new double[3];
            double[] normalizedY = new double[3];
            double[] normalizedZ = new double[3];

            // The X axis is a vector from start to end
            Subtract(endPoint, startPoint, normalizedX);

            double length =
                Math.Sqrt(normalizedX[0] * normalizedX[0] + normalizedX[1] * normalizedX[1] + normalizedX[2] * normalizedX[2]);
            
            //Console.WriteLine(@"length = " + length);

            normalizedX = VTKUtil.Normalize(normalizedX);

            // The Z axis is an arbitrary vecotr cross X
            double[] arbitrary = new double[3];
            arbitrary[0] = vtkMath.Random(-10, 10);
            arbitrary[1] = vtkMath.Random(-10, 10);
            arbitrary[2] = vtkMath.Random(-10, 10);
            arbitrary = VTKUtil.Normalize(arbitrary);
            // TODO FIX ME

            normalizedZ = VTKUtil.Cross(normalizedX, arbitrary);
            //vtkMath.Cross(VTKUtil.ConvertIntPtr(normalizedX), VTKUtil.ConvertIntPtr(arbitrary), VTKUtil.ConvertIntPtr(normalizedZ));
            //vtkMath.Normalize(VTKUtil.ConvertIntPtr(normalizedZ));

            // The Y axis is Z cross X

            // TODO FIX ME
            //vtkMath.Cross(VTKUtil.ConvertIntPtr(normalizedZ), VTKUtil.ConvertIntPtr(normalizedX), VTKUtil.ConvertIntPtr(normalizedY));
            normalizedY = VTKUtil.Cross(normalizedX, normalizedZ);

            vtkMatrix4x4 matrix = vtkMatrix4x4.New();

            // Create the direction cosine matrix
            matrix.Identity();
            for (int i = 0; i < 3; i++)
            {
                matrix.SetElement(i, 0, normalizedX[i]);
                matrix.SetElement(i, 1, normalizedY[i]);
                matrix.SetElement(i, 2, normalizedZ[i]);
            }

            //Console.WriteLine(matrix);

            // Apply the transforms
            vtkTransform transform = vtkTransform.New();
            transform.Translate(VTKUtil.ConvertIntPtr(startPoint));
            transform.Concatenate(matrix);

            //length = 500;
            transform.Scale(length, length, length);

            // Transform the polydata
            //vtkTransformPolyDataFilter transformPD = new vtkTransformPolyDataFilter();
            //transformPD.SetTransform(transform);
            //transformPD.SetInputConnection(_arrowSource.GetOutputPort());

            _arrowSource.SetTipRadius(0.25);
            _arrowSource.SetTipLength(0.7);
            _arrowSource.Update();
            _mapper.SetInput(_arrowSource.GetOutput());
            _arrowActor.SetUserMatrix(transform.GetMatrix());
            _arrowActor.GetProperty().ShadingOn();
            _arrowActor.GetProperty().SetAmbient(0.7);
            _arrowActor.GetProperty().SetOpacity(0.4);
            _arrowActor.SetMapper(_mapper);
        }

        public void UpdatePosition_OLD(double[] endPoint, double[] startPoint)
        {
            double[] normalizedX = new double[3];
            double[] normalizedY = new double[3];
            double[] normalizedZ = new double[3];

            // The X axis is a vector from start to end
            Subtract(endPoint, startPoint, normalizedX);

            double length = vtkMath.Norm(VTKUtil.ConvertIntPtr(normalizedX));
            vtkMath.Normalize(VTKUtil.ConvertIntPtr(normalizedX));

            // The Z axis is an arbitrary vecotr cross X
            double[] arbitrary = new double[3];
            arbitrary[0] = vtkMath.Random(-10, 10);
            arbitrary[1] = vtkMath.Random(-10, 10);
            arbitrary[2] = vtkMath.Random(-10, 10);

            // TODO FIX ME

            vtkMath.Cross(VTKUtil.ConvertIntPtr(normalizedX), VTKUtil.ConvertIntPtr(arbitrary), VTKUtil.ConvertIntPtr(normalizedZ));
            vtkMath.Normalize(VTKUtil.ConvertIntPtr(normalizedZ));

            // The Y axis is Z cross X

            // TODO FIX ME
            vtkMath.Cross(VTKUtil.ConvertIntPtr(normalizedZ), VTKUtil.ConvertIntPtr(normalizedX), VTKUtil.ConvertIntPtr(normalizedY));
            vtkMatrix4x4 matrix = vtkMatrix4x4.New();

            // Create the direction cosine matrix
            matrix.Identity();
            for (int i = 0; i < 3; i++)
            {
                matrix.SetElement(i, 0, normalizedX[i]);
                matrix.SetElement(i, 1, normalizedY[i]);
                matrix.SetElement(i, 2, normalizedZ[i]);
            }

            // Apply the transforms
            vtkTransform transform = vtkTransform.New();
            transform.Translate(VTKUtil.ConvertIntPtr(startPoint));
            transform.Concatenate(matrix);

            //length = 500;
            transform.Scale(length, length, length);

            // Transform the polydata
            //vtkTransformPolyDataFilter transformPD = new vtkTransformPolyDataFilter();
            //transformPD.SetTransform(transform);
            //transformPD.SetInputConnection(_arrowSource.GetOutputPort());

            _arrowSource.Update();
            _mapper.SetInput(_arrowSource.GetOutput());
            _arrowActor.SetUserMatrix(transform.GetMatrix());
            _arrowActor.SetMapper(_mapper);
        }

        internal void UpdateCamera(vtkCamera mainViewCamera)
        {
            //return;

            vtkCamera OrientationEnumViewCamera = _topRenderer.GetActiveCamera();

            double[] mainViewCameraPosition = mainViewCamera.GetPosition();
            double[] mainViewFocalPosition = mainViewCamera.GetFocalPoint();
            double[] mainViewViewUp = mainViewCamera.GetViewUp();

            OrientationEnumViewCamera.SetPosition(mainViewCameraPosition[0], mainViewCameraPosition[1], mainViewCameraPosition[2]);
            OrientationEnumViewCamera.SetFocalPoint(mainViewFocalPosition[0], mainViewFocalPosition[1], mainViewFocalPosition[2]);
            OrientationEnumViewCamera.SetViewUp(mainViewViewUp[0], mainViewViewUp[1], mainViewViewUp[2]);

            _topRenderer.ResetCameraClippingRange();
            //_topRenderer.ResetCamera();
        }
    }
}

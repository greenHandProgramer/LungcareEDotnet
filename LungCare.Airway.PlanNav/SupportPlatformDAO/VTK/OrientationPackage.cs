/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using Kitware.VTK;
using System;
namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    internal class OrientationEnumPackage
    {
        internal vtkRenderer _topRenderer;
        internal vtkRenderer _mainRenderer;
        internal vtkRenderWindow _renwin;
        internal XmlPolyDataPackage _xmlPolyDataPackage;

        internal OrientationEnumPackage(vtkPolyData polyData, vtkRenderWindow renwin, vtkRenderer mainRenderer)
        {
            this._renwin = renwin;
            this._mainRenderer = mainRenderer;

            int oldNumberOfRenderer = renwin.GetNumberOfLayers();
            Console.WriteLine(string.Format("oldNumberOfRenderer = {0}", oldNumberOfRenderer));
            int newNumberOfRenderer = oldNumberOfRenderer + 1;

            mainRenderer.SetLayer(0);

            _topRenderer = vtkRenderer.New();
            _topRenderer.SetViewport(0, 0, 0.2, 0.2);
            _topRenderer.SetLayer(newNumberOfRenderer - 1);
            _topRenderer.InteractiveOff();

            _xmlPolyDataPackage = new XmlPolyDataPackage(polyData, _topRenderer);
            //xmlPolyDataPackage.SetOpacity(0.8);
            //xmlPolyDataPackage.Actor.GetProperty().ShadingOn();
            //xmlPolyDataPackage.Actor.GetProperty().SetAmbient(0.8);
            //xmlPolyDataPackage.SetColor(1, 1, 0.5);
            _xmlPolyDataPackage.SetColor(255 / 255.0, 204 / 255.0, 102 / 255.0);

            renwin.SetNumberOfLayers(newNumberOfRenderer);
            renwin.AddRenderer(_topRenderer);

            //_topRenderer.SetActiveCamera(mainRenderer.GetActiveCamera());
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
            OrientationEnumViewCamera.ComputeViewPlaneNormal();
            OrientationEnumViewCamera.ComputeProjAndViewParams();
            OrientationEnumViewCamera.OrthogonalizeViewUp();

            _topRenderer.ResetCameraClippingRange();
            _topRenderer.ResetCamera();
        }
    }
}

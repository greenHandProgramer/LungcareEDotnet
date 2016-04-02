/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using Kitware.VTK;
using System;
namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    internal class WatermarkPackage
    {
        public vtkImageData _imageData;

        public void On()
        {
            _actor.VisibilityOn();
        }

        public void Off()
        {
            _actor.VisibilityOff();
        }

        public void SetOpacity(double opacity)
        {
            _actor.SetOpacity(opacity);
        }

        public WatermarkPackage(vtkRenderWindow renwin, vtkRenderer mainRenderer, vtkImageData maskImage)
        {
            int oldNumberOfRenderer = renwin.GetNumberOfLayers();
            Console.WriteLine(string.Format("oldNumberOfRenderer = {0}", oldNumberOfRenderer));
            int newNumberOfRenderer = oldNumberOfRenderer + 1;

            mainRenderer.SetLayer(0);

            vtkRenderer _topRenderer = vtkRenderer.New();
            _topRenderer.SetViewport(0, 0, 1, 1);
            _topRenderer.SetLayer(newNumberOfRenderer - 1);
            _topRenderer.InteractiveOff();
            _topRenderer.GetActiveCamera().ParallelProjectionOn();

            mainRenderer.SetLayer(1);
            _topRenderer.SetLayer(0);

            renwin.SetNumberOfLayers(newNumberOfRenderer);
            renwin.AddRenderer(_topRenderer);

            // Display the image
            _actor = vtkImageActor.New();

            _actor.SetInput(maskImage);
            _actor.SetOpacity(0.8);

            _topRenderer.AddActor(_actor);

            _topRenderer.ResetCameraClippingRange();
            _topRenderer.ResetCamera();
            _topRenderer.GetActiveCamera().Zoom(1.6);
        }

        private vtkImageActor _actor;
    }
}

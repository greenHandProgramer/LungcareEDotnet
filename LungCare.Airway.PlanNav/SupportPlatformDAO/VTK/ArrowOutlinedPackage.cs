/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using Kitware.VTK;
using System;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    public class ArrowOutlinedPackage
    {
        private readonly ArrowPackage2 _arrowPackage;
        private readonly ArrowPackage2 _arrowPackageTop;

        public void UpdateCamera(double[] cameraPosition, double[] focalPosition)
        {
            _arrowPackage.UpdatePosition(VTKUtil.Extend2(cameraPosition, focalPosition, 10), cameraPosition);
            _arrowPackageTop.UpdatePosition(VTKUtil.Extend2(cameraPosition, focalPosition, 8), VTKUtil.Extend2(cameraPosition, focalPosition, 1.5));
        }

        public ArrowOutlinedPackage(vtkRenderer rendererTopTopMostRenderer, vtkRenderer rendererTopTopTopMostRenderer)
        {
            _arrowPackage = new ArrowPackage2(new double[] { 0, 0, 0 }, new double[] { 0, 0, 1 }, rendererTopTopMostRenderer);
            _arrowPackage.SetColor(1, 1, 1);
            _arrowPackage.SetOpacity(0.8);

            _arrowPackageTop = new ArrowPackage2(new double[] { 0, 0, 0 }, new double[] { 0, 0, 1 }, rendererTopTopTopMostRenderer);
            _arrowPackageTop.SetColor(1, 1, 0);
            _arrowPackageTop.SetOpacity(0.8);
        }

        internal void SwitchNoLesion()
        {
            if (_arrowPackage != null)
            {
                _arrowPackage.VisibilityOff();
            }
            if (_arrowPackageTop != null)
            {
                _arrowPackageTop.VisibilityOff();
            }
            //if (_lesionPackage != null)
            //{
            //    _lesionPackage.VisibilityOff();
            //}
        }

        public void SetNavArrowVisible(bool visible)
        {
            if (visible)
            {
                _arrowPackage.VisibilityOn();
                _arrowPackageTop.VisibilityOn();
            }
            else
            {
                _arrowPackage.VisibilityOff();
                _arrowPackageTop.VisibilityOff();
            }
        }

        internal void SetColor(int p1, int p2, int p3)
        {
            _arrowPackageTop.SetColor(1, 0, 0);            
        }
    }
}

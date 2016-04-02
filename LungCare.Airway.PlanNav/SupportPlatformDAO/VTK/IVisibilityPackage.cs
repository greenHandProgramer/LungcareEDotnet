/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    internal interface IVisibilityPackage
    {
        void SwitchVisibility();
        void VisibilityOff();
        void VisibilityOn();
    }
}

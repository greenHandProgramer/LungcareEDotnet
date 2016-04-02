/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using Kitware.VTK;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    public class CornerAnnotationActorPackage
    {
        private vtkCornerAnnotation cornerAnnotation;

        public CornerAnnotationActorPackage(vtkRenderer aRender)
        {
            cornerAnnotation = vtkCornerAnnotation.New();
            cornerAnnotation.SetLinearFontScaleFactor(2);
            cornerAnnotation.GetTextProperty().ShadowOn();
            cornerAnnotation.SetNonlinearFontScaleFactor(1);
            cornerAnnotation.SetMaximumFontSize(20);
            cornerAnnotation.SetText(0, "lower left");
            cornerAnnotation.SetText(1, "lower right");
            cornerAnnotation.SetText(2, "upper left");
            cornerAnnotation.SetText(3, "upper right");
            cornerAnnotation.GetTextProperty().SetColor(0.8, 0.8, 0.8);
            cornerAnnotation.GetTextProperty().SetColor(1, 1, 1);
            //cornerAnnotation.GetTextProperty().SetFontFamily(4);
            cornerAnnotation.GetTextProperty().BoldOn();
            //.SetFontFamilyToArial();

            cornerAnnotation.SetText(0, " ");
            cornerAnnotation.SetText(1, " ");
            cornerAnnotation.SetText(2, " ");
            cornerAnnotation.SetText(3, " ");

            aRender.AddViewProp(cornerAnnotation);
        }

        public string GetText(int i)
        {
            return cornerAnnotation.GetText(i);
        }

        public void SetText(int i, string text)
        {
            cornerAnnotation.SetText(i, text);
        }

        public void SetLeftDownText(string text)
        {
            cornerAnnotation.SetText(0, text);
        }

        public void SetRightDownText(string text)
        {
            cornerAnnotation.SetText(1, text);
        }

        public void SetLeftTop(string text)
        {
            cornerAnnotation.SetText(2, text);
        }

        public void SetRightTop(string text)
        {
            cornerAnnotation.SetText(3, text);
        }

        public string GetLeftDownText()
        {
            return cornerAnnotation.GetText(0);
        }

        public void SetColor(double r, double g, double b)
        {
            cornerAnnotation.GetTextProperty().SetColor(r, g, b);
        }
        
        public void TextOn()
        {
            if (cornerAnnotation.GetVisibility() == 0)
            {
                cornerAnnotation.VisibilityOn();
            }
        }

        public void TextOff()
        {
            if (cornerAnnotation.GetVisibility() == 1)
            {
                cornerAnnotation.VisibilityOff();
            }
        }

        public void SetFontSize(int size)
        {
            cornerAnnotation.SetMaximumFontSize(size);
        }
    }
}

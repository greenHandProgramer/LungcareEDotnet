using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Graph = Microsoft.Office.Interop.Graph;
using System.Drawing;
using System.Windows;
using Microsoft.Office.Interop.PowerPoint;
using System.IO;
using System.Threading;

namespace LungCare.SupportPlatform.SupportPlatformDAO.Utils
{
    /// <summary>
    /// PPT文档操作实现类.
    /// </summary>
    class PowerPointDAO
    {

        public PowerPointDAO()
        {
            CreateNewPPT();
        }

        private string _fileName = System.Windows.Forms.Application.StartupPath+ "\\test.pot";
        
        #region=========基本的参数信息=======
        PowerPoint.Application application = null;
        PowerPoint.Presentations pptPresentationSet = null;
        PowerPoint._Presentation pptPresentation;
        PowerPoint.Slides pptSlides;
        PowerPoint._Slide pptSlide;
        CustomLayout customLayout;

        PowerPoint.SlideShowWindows slideWindows;
        PowerPoint.SlideShowSettings slideSettings;

        PowerPoint.SlideShowTransition objSST;
        PowerPoint.SlideRange objSldRng;
      
        PowerPoint.TextRange textRng;
        bool bAssistantOn;
        double pixperPoint = 0;
        double offsetx = 0;
        double offsety = 0;

        private string textDefaultFontName = "微软雅黑";
        private int textDefaultFontSize = 24;
        #endregion

        public PowerPointDAO(string filename)
        {
            File.Copy(_fileName, filename , true);

            Thread.Sleep(200);
            _fileName = filename;
            CreateNewPPT();
        }


        public void CreateNewPPT()
        {
            application = new PowerPoint.Application();
            pptPresentationSet = application.Presentations;
            pptPresentation = pptPresentationSet.Open(_fileName, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoTrue);
            //customLayout = pptPresentationSet.SlideMaster.CustomLayouts[Microsoft.Office.Interop.PowerPoint.PpSlideLayout.ppLayoutText];
            pptSlides = pptPresentation.Slides;
            pptSlide = pptSlides.AddSlide(1 , customLayout);

            textRng = pptSlide.Shapes[1].TextFrame.TextRange;
            textRng.Font.Name = textDefaultFontName;
            textRng.Font.Size = textDefaultFontSize;
        }


        public _Slide AddNewSlide()
        {
            return pptSlides.Add(pptSlides.Count , PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
        }
        public void AddText(PowerPoint._Slide slide, String text, float left, float top, float width, float height)
        {
            PowerPoint.Shape shape = slide.Shapes.AddTextbox(MsoTextOrientation.msoTextOrientationHorizontal, left, top, width, height);
            PowerPoint.TextRange textRng = shape.TextFrame.TextRange;
            textRng.Text = text;
        }



        public void AddText(int slideIndex , int shapeIndex , string content)
        {
            textRng = pptSlides[slideIndex].Shapes[shapeIndex].TextFrame.TextRange;
            textRng.Text = content;

        }


        public void AddText(String text , PowerPoint._Slide slide,  float left, float top, float width, float height)
        {
            PowerPoint.Shape shape = slide.Shapes.AddTextbox(MsoTextOrientation.msoTextOrientationHorizontal, left, top, width, height);
            PowerPoint.TextRange textRng = shape.TextFrame.TextRange;
            textRng.Font.Name = "微软雅黑";
            textRng.Font.Size=24;
            textRng.Font.Bold = MsoTriState.msoCTrue;
            textRng.Text = text;

        }

        public void AddImage(string fileName, float left, float top, float width, float height)
        {
            if (File.Exists(fileName))
            {
                pptSlide.Shapes.AddPicture(fileName, MsoTriState.msoFalse, MsoTriState.msoTrue, 150, 150, 128, 128);
            }
        }


        public void AddImage(string fileName,_Slide slide , float left, float top, float width, float height)
        {
            if (File.Exists(fileName))
            {
                slide.Shapes.AddPicture(fileName, MsoTriState.msoFalse, MsoTriState.msoTrue, left, top, width, height);
            }
        }

        public void Save()
        {
            //Prevent Office Assistant from displaying alert messages:
            bool bAssistantOn = application.Assistant.On;
            application.Assistant.On = false;

            //Run the Slide show from slides 1 thru 3.
            slideSettings = pptPresentation.SlideShowSettings;
            slideSettings.StartingSlide = 1;
            slideSettings.EndingSlide = 3;
            slideSettings.Run();

            //Wait for the slide show to end.
            slideWindows = application.SlideShowWindows;
            while (slideWindows.Count >= 1) System.Threading.Thread.Sleep(100);

            //Reenable Office Assisant, if it was on:
            if (bAssistantOn)
            {
                application.Assistant.On = true;
                application.Assistant.Visible = false;
            }

            //Close the presentation without saving changes and quit PowerPoint.
            pptPresentation.Close();
            application.Quit();

            Thread.Sleep(200);
            string fileName = string.Format("朗开医疗体检报告[{0}][{1}].ppt", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            File.Copy(_fileName , Path.GetDirectoryName(_fileName)+"\\"+fileName);
        }

        public void Save(string destFileName)
        {
            Save();
            File.Copy(_fileName , destFileName , true);
        }
        

    }
}

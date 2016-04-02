using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Graph = Microsoft.Office.Interop.Graph;
using System.Drawing;
using System.Windows;

namespace LungCare.SupportPlatform.SupportPlatformDAO.Utils
{
    class ProducePPT
    {

        public void CreatePPT(Bitmap axialAllBitmap, Bitmap axialPartBitmap, Bitmap axial3DBitmap,
         Bitmap sagitalAllBitmap, Bitmap sagitalPartBitmap, Bitmap sagital3DBitmap,
         Bitmap coronalAllBitmap, Bitmap coronalPartBitmap, Bitmap coronal3DBitmap)
        {
            add9CTPic(axialAllBitmap, axialPartBitmap, axial3DBitmap,
             sagitalAllBitmap, sagitalPartBitmap, sagital3DBitmap,
             coronalAllBitmap, coronalPartBitmap, coronal3DBitmap);
            //ShowPresentation();
            //addText();
            GC.Collect();
        }

        private void add9CTPic(Bitmap axialAllBitmap, Bitmap axialPartBitmap, Bitmap axial3DBitmap,
         Bitmap sagitalAllBitmap, Bitmap sagitalPartBitmap, Bitmap sagital3DBitmap,
         Bitmap coronalAllBitmap, Bitmap coronalPartBitmap, Bitmap coronal3DBitmap)
        {
            String strTemplate, strPic;
            strTemplate = @"C:\AirwayVE\PPTModel\test.pot";

            bool bAssistantOn;

            PowerPoint.Application objApp;
            PowerPoint.Presentations objPresSet;
            PowerPoint._Presentation objPres;
            PowerPoint.Slides objSlides;
            PowerPoint._Slide objSlide;
            PowerPoint.TextRange objTextRng;
            PowerPoint.SlideShowWindows objSSWs;
            PowerPoint.SlideShowSettings objSSS;
            

            //Create a new presentation based on a template.
            objApp = new PowerPoint.Application();
            objApp.Visible = MsoTriState.msoTrue;
            objPresSet = objApp.Presentations;
            objPres = objPresSet.Open(strTemplate, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoTrue);
            objSlides = objPres.Slides;

            //Build Slide #1:
            //Add text to the slide, change the font and insert/position a 
            //picture on the first slide.
            objSlide = objSlides.Add(2, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            objTextRng.Text = "My Sample Presentation";
            //objTextRng.Font.Name = "Arial Black";
            //objTextRng.Font.Size = 48;
            objSlide.Shapes.AddPicture("C:\\AirwayVE\\Images\\1.bmp", MsoTriState.msoFalse, MsoTriState.msoTrue, 100, 200, 128, 128);
            InsertText(objSlide, "11111111", 100, 100, 200, 50);


            objSlide = objSlides.Add(3, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            objTextRng.Text = "My Sample Presentation";
            objTextRng.Font.Name = "Comic Sans MS";
            objTextRng.Font.Size = 48;
            objSlide.Shapes.AddPicture("C:\\AirwayVE\\Images\\2.bmp", MsoTriState.msoFalse, MsoTriState.msoTrue, 150, 150, 128, 128);
            InsertText(objSlide, "22222222", 100, 100, 200, 50);

            objSlide = objSlides.Add(4, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            objTextRng.Text = "My Sample Presentation";
            objTextRng.Font.Name = "Comic Sans MS";
            objTextRng.Font.Size = 48;
            objSlide.Shapes.AddPicture("C:\\AirwayVE\\Images\\3.bmp", MsoTriState.msoFalse, MsoTriState.msoTrue, 150, 150, 128, 128);
            InsertText(objSlide, "33333333", 100, 100, 200, 50);

            objSlide = objSlides.Add(5, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            objTextRng.Text = "My Sample Presentation";
            objTextRng.Font.Name = "Comic Sans MS";
            objTextRng.Font.Size = 48;
            objSlide.Shapes.AddPicture("C:\\AirwayVE\\Images\\4.bmp", MsoTriState.msoFalse, MsoTriState.msoTrue, 150, 150, 128, 128);
            InsertText(objSlide, "44444444", 100, 100, 200, 50);

            objSlide = objSlides.Add(6, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            objTextRng.Text = "My Sample Presentation";
            objTextRng.Font.Name = "Comic Sans MS";
            objTextRng.Font.Size = 48;
            objSlide.Shapes.AddPicture("C:\\AirwayVE\\Images\\5.bmp", MsoTriState.msoFalse, MsoTriState.msoTrue, 150, 150, 128, 128);
            InsertText(objSlide, "55555555", 100, 100, 200, 50);

            objSlide = objSlides.Add(7, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            objTextRng.Text = "My Sample Presentation";
            objTextRng.Font.Name = "Comic Sans MS";
            objTextRng.Font.Size = 48;
            objSlide.Shapes.AddPicture("C:\\AirwayVE\\Images\\6.bmp", MsoTriState.msoFalse, MsoTriState.msoTrue, 150, 150, 128, 128);
            InsertText(objSlide, "66666666", 100, 100, 200, 50);

            objSlide = objSlides.Add(8, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            objTextRng.Text = "My Sample Presentation";
            objTextRng.Font.Name = "Comic Sans MS";
            objTextRng.Font.Size = 48;
            objSlide.Shapes.AddPicture("C:\\AirwayVE\\Images\\7.bmp", MsoTriState.msoFalse, MsoTriState.msoTrue, 150, 150, 128, 128);
            InsertText(objSlide, "77777777", 100, 100, 200, 50);

            objSlide = objSlides.Add(9, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            objTextRng.Text = "My Sample Presentation";
            objTextRng.Font.Name = "Comic Sans MS";
            objTextRng.Font.Size = 48;
            objSlide.Shapes.AddPicture("C:\\AirwayVE\\Images\\8.bmp", MsoTriState.msoFalse, MsoTriState.msoTrue, 150, 150, 128, 128);
            InsertText(objSlide, "88888888", 100, 100, 200, 50);

            objSlide = objSlides.Add(10, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            objTextRng.Text = "My Sample Presentation";
            objTextRng.Font.Name = "Comic Sans MS";
            objTextRng.Font.Size = 48;
            objSlide.Shapes.AddPicture("C:\\AirwayVE\\Images\\9.bmp", MsoTriState.msoFalse, MsoTriState.msoTrue, 150, 150, 128, 128);
            InsertText(objSlide, "99999999", 100, 100, 200, 50);


            

            //Prevent Office Assistant from displaying alert messages:
            bAssistantOn = objApp.Assistant.On;
            objApp.Assistant.On = false;

            //Run the Slide show from slides 1 thru 3.
            objSSS = objPres.SlideShowSettings;
            objSSS.StartingSlide = 1;
            objSSS.EndingSlide = 3;
            objSSS.Run();

            //Wait for the slide show to end.
            objSSWs = objApp.SlideShowWindows;
            while (objSSWs.Count >= 1) System.Threading.Thread.Sleep(100);

            //Reenable Office Assisant, if it was on:
            if (bAssistantOn)
            {
                objApp.Assistant.On = true;
                objApp.Assistant.Visible = false;
            }

            //Close the presentation without saving changes and quit PowerPoint.
            objPres.Close();
            objApp.Quit();
        }

        private void ShowPresentation()
        {
            String strTemplate, strPic;
            //strTemplate = "C:\\Program Files\\Microsoft Office\\Templates\\Presentation Designs\\Blends.pot";
            //strPic = "C:\\Windows\\Blue Lace 16.bmp";
            strTemplate = @"C:\Users\lk_wjl\Desktop\test.pot";
            strPic = "F:\\AllFiles\\jyc\\气管+病灶.png";

            bool bAssistantOn;

            PowerPoint.Application objApp;
            PowerPoint.Presentations objPresSet;
            PowerPoint._Presentation objPres;
            PowerPoint.Slides objSlides;
            PowerPoint._Slide objSlide;
            PowerPoint.TextRange objTextRng;
            PowerPoint.SlideShowWindows objSSWs;
            PowerPoint.SlideShowTransition objSST;
            PowerPoint.SlideShowSettings objSSS;
            PowerPoint.SlideRange objSldRng;
            PowerPoint.Shapes objShapes;
            PowerPoint.Shape objShape;
            Graph.Chart objChart;

            //Create a new presentation based on a template.
            objApp = new PowerPoint.Application();
            objApp.Visible = MsoTriState.msoTrue;
            objPresSet = objApp.Presentations;
            objPres = objPresSet.Open(strTemplate, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoTrue);
            objSlides = objPres.Slides;

            //Build Slide #1:
            //Add text to the slide, change the font and insert/position a 
            //picture on the first slide.
            objSlide = objSlides.Add(1, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);

            objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            objTextRng.Text = "My Sample Presentation";
            objTextRng.Font.Name = "Comic Sans MS";
            objTextRng.Font.Size = 48;

            objSlide.Shapes.AddPicture(strPic, MsoTriState.msoFalse, MsoTriState.msoTrue, 150, 150, 128, 128);

            InsertText(objSlide, "HelloWorld", 100, 100, 200, 50);


            //Build Slide #2:
            //Add text to the slide title, format the text. Also add a chart to the
            //slide and change the chart type to a 3D pie chart.
            //objSlide = objSlides.Add(2, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            //objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            //objTextRng.Text = "My Chart";
            //objTextRng.Font.Name = "Comic Sans MS"; 
            //objTextRng.Font.Size = 48;
            //objChart = (Graph.Chart)objSlide.Shapes.AddOLEObject(150, 150, 480, 320,"MSGraph.Chart.8", "", MsoTriState.msoFalse, "", 0, "",MsoTriState.msoFalse).OLEFormat.Object;
            //objChart.ChartType = Graph.XlChartType.xl3DPie;
            //objChart.Legend.Position = Graph.XlLegendPosition.xlLegendPositionBottom;
            //objChart.HasTitle = true;
            //objChart.ChartTitle.Text = "Here it is...";

            //Build Slide #3:
            //Change the background color of this slide only. Add a text effect to the slide
            //and apply various color schemes and shadows to the text effect.
            //objSlide = objSlides.Add(3, PowerPoint.PpSlideLayout.ppLayoutBlank);
            //objSlide.FollowMasterBackground = MsoTriState.msoFalse;
            //objShapes = objSlide.Shapes;
            //objShape = objShapes.AddTextEffect(MsoPresetTextEffect.msoTextEffect27,
            //  "The End", "Impact", 96, MsoTriState.msoFalse, MsoTriState.msoFalse, 230, 200);

            //Modify the slide show transition settings for all 3 slides in the presentation.
            //int[] SlideIdx = new int[3];
            //for (int i = 0; i < 3; i++) SlideIdx[i] = i + 1;
            //objSldRng = objSlides.Range(SlideIdx);
            //objSST = objSldRng.SlideShowTransition;
            //objSST.AdvanceOnTime = MsoTriState.msoTrue;
            //objSST.AdvanceTime = 3;
            //objSST.EntryEffect = PowerPoint.PpEntryEffect.ppEffectBoxOut;

            //Prevent Office Assistant from displaying alert messages:
            bAssistantOn = objApp.Assistant.On;
            objApp.Assistant.On = false;

            //Run the Slide show from slides 1 thru 3.
            objSSS = objPres.SlideShowSettings;
            objSSS.StartingSlide = 1;
            objSSS.EndingSlide = 3;
            objSSS.Run();

            //Wait for the slide show to end.
            objSSWs = objApp.SlideShowWindows;
            while (objSSWs.Count >= 1) System.Threading.Thread.Sleep(100);

            //Reenable Office Assisant, if it was on:
            if (bAssistantOn)
            {
                objApp.Assistant.On = true;
                objApp.Assistant.Visible = false;
            }

            //Close the presentation without saving changes and quit PowerPoint.
            objPres.Close();
            objApp.Quit();
        }

        public void InsertText(PowerPoint._Slide slide, String text, float left, float top, float width, float height)
        {
            PowerPoint.Shape shape = slide.Shapes.AddTextbox(MsoTextOrientation.msoTextOrientationHorizontal, left, top, width, height);
            PowerPoint.TextRange textRng = shape.TextFrame.TextRange;
            textRng.Text = text;
        }

        private void addText()
        {
            string path = @"C:\Users\lk_wjl\Desktop\Blue.pot";
            PowerPoint.Application objApp;
            PowerPoint.Presentations objPresSet;
            PowerPoint._Presentation objPres;
            PowerPoint.Slides objSlides;
            PowerPoint._Slide objSlide;
            PowerPoint.TextRange objTextRng;
            PowerPoint.SlideShowWindows objSSWs;
            PowerPoint.SlideShowTransition objSST;
            PowerPoint.SlideShowSettings objSSS;
            PowerPoint.SlideRange objSldRng;
            //PowerPoint.Shapes objShapes;
            //PowerPoint.Shape objShape;
            Graph.Chart objChart;

            //Create a new presentation based on a template.
            objApp = new PowerPoint.Application();
            objApp.Visible = MsoTriState.msoTrue;
            objPresSet = objApp.Presentations;
            objPres = objPresSet.Open(path, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoTrue);
            objSlides = objPres.Slides;

            //Build Slide #1:
            //Add text to the slide, change the font and insert/position a 
            //picture on the first slide.
            objSlide = objSlides.Add(2, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            string text = "示例文本";
            foreach (PowerPoint._Slide slide in objSlides)
            {
                foreach (PowerPoint.Shape shape in slide.Shapes)
                {
                    shape.TextFrame.TextRange.InsertAfter(text);
                }
            }
            objPres.Close();
            objApp.Quit();
            MessageBox.Show("创建完毕，注意查阅！");
        }
    }
}

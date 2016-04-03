using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SampleGrabberNET;
using Kitware.VTK;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Controls;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using Dicom.Imaging;
using System.Collections;
using LungCare.SupportPlatform.Models;
using ClearCanvas.ImageViewer;
using ClearCanvas.ImageViewer.StudyManagement;
using LungCare.SupportPlatform.Entities;
using LungCare.Airway.WinformUIControls.Entities;
using LungCare.SupportPlatform.SupportPlatformDAO.VTK;
using LungCare.SupportPlatform.SupportPlatformDAO.Utils;

namespace LungCare.SupportPlatform.UI.UserControls.Examination
{
    public partial class CTViewControlSingle : System.Windows.Forms.UserControl
    {
        
        public CTViewControlSingle()
        {

            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            pictureBox.MouseWheel += new MouseEventHandler(pictureBox_MouseWheel);
            pictureBox.MouseLeave += pictureBox_MouseLeave;
            this.Name = "CTViewControl";
            //this.BackColor = Color.Black;
            this.ResumeLayout(false);

            tbFrame = this.trackbarUserControl1.FindName("S1") as Slider;
            tbFrame.Visibility = System.Windows.Visibility.Hidden;
            tbFrame.SmallChange = 1;
            tbFrame.IsSnapToTickEnabled = true;
            //tbFrame.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(tbFrame_ValueChanged);

            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip();
            pictureBox.ContextMenuStrip = contextMenuStrip1;
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            //this.contextMenuStrip1.Size = new System.Drawing.Size(153, 48);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            //this.toolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem1.Text = "Abdomen(350/50)";
            this.toolStripMenuItem1.Click += toolStripMenuItem1_Click;

            ToolStripMenuItem toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            //toolStripMenuItem2.Size = new System.Drawing.Size(152, 22);
            toolStripMenuItem2.Text = "Bone(2500/500)";
            toolStripMenuItem2.Click += toolStripMenuItem2_Click;
            this.contextMenuStrip1.Items.Add(toolStripMenuItem2);

            ToolStripMenuItem toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            //toolStripMenuItem3.Size = new System.Drawing.Size(152, 22);
            toolStripMenuItem3.Text = "Cerebrum(120/40)";
            toolStripMenuItem3.Click += toolStripMenuItem3_Click;
            this.contextMenuStrip1.Items.Add(toolStripMenuItem3);

            ToolStripMenuItem toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            //toolStripMenuItem4.Size = new System.Drawing.Size(152, 22);
            toolStripMenuItem4.Text = "Liver(150/50)";
            toolStripMenuItem4.Click += toolStripMenuItem4_Click;
            this.contextMenuStrip1.Items.Add(toolStripMenuItem4);

            ToolStripMenuItem toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            //toolStripMenuItem5.Size = new System.Drawing.Size(152, 22);
            toolStripMenuItem5.Text = "Lung(1500/-500)";
            toolStripMenuItem5.Click += toolStripMenuItem5_Click;
            this.contextMenuStrip1.Items.Add(toolStripMenuItem5);

            ToolStripMenuItem toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            //toolStripMenuItem6.Size = new System.Drawing.Size(152, 22);
            toolStripMenuItem6.Text = "Mediastinum(300/50)";
            toolStripMenuItem6.Click += toolStripMenuItem6_Click;
            this.contextMenuStrip1.Items.Add(toolStripMenuItem6);

            ToolStripMenuItem toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            //toolStripMenuItem7.Size = new System.Drawing.Size(152, 22);
            toolStripMenuItem7.Text = "Pelvis(400/40)";
            toolStripMenuItem7.Click += toolStripMenuItem7_Click;
            this.contextMenuStrip1.Items.Add(toolStripMenuItem7);

            ToolStripMenuItem toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            //toolStripMenuItem8.Size = new System.Drawing.Size(152, 22);
            toolStripMenuItem8.Text = "Posterior fossa(250/80)";
            toolStripMenuItem8.Click += toolStripMenuItem8_Click;
            this.contextMenuStrip1.Items.Add(toolStripMenuItem8);

            ToolStripMenuItem toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            //toolStripMenuItem9.Size = new System.Drawing.Size(152, 22);
            toolStripMenuItem9.Text = "Low constrast(190/80)";
            toolStripMenuItem9.Click += toolStripMenuItem9_Click;
            this.contextMenuStrip1.Items.Add(toolStripMenuItem9);
        }

        void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetWindowWidthLevel(350 , 50);
        }

        void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SetWindowWidthLevel(2500, 500);
        }

        void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            SetWindowWidthLevel(120, 40);
        }

        void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            SetWindowWidthLevel(150, 50);
        }

        void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            SetWindowWidthLevel(1500, -500);
        }

        void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            SetWindowWidthLevel(300, 50);
        }

        void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            SetWindowWidthLevel(400, 40);
        }

        void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            SetWindowWidthLevel(250, 80);
        }

        void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            SetWindowWidthLevel(190, 80);
        }


        public event EventHandler<WindowWidthLevelArgs> WindowWidthLevelHandler;
        void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            //throw new NotImplementedException();
        }


        private System.Windows.Controls.Primitives.ToggleButton btnEnableJump;
        private Size maxButtonSize = new Size(35, 35);
        private Slider tbFrame;

        private UnsafeBitmap _unsafeBmpCT;
        private Bitmap _bitmapCT, _bitmapAirway;
        private int[] _dimensionsCT;
        private int _windowLevel = -600, _windowWidth = 1000;
        private System.Drawing.Drawing2D.Matrix matrix = new Matrix();
        private double[] _point3D;
        private double[] _point2D;
        private Rectangle currRectagle;
        //private List<Rectangle> listRectangle = new List<Rectangle>();
        private List<RectangleEntity> listRectangle = new List<RectangleEntity>();


        private byte[] _airwayData;
        private MenuStrip menuStrip1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem toolStripMenuItem1;
        private short[] _dicomAlldata;


        public void SwithToLung()
        {
            _windowLevel = -600;
            _windowWidth = 1000;
            if (_dicomImage != null)
            {
                _dicomImage.WindowWidth = _windowWidth;
                _dicomImage.WindowCenter = _windowLevel;
                _bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
                pictureBox.Refresh();
                UpdateImageParameter();
            }
        }


        public void SwithToAbdomen()
        {
            _windowLevel = 50;
            _windowWidth = 350;
            if (_dicomImage != null)
            {
                _dicomImage.WindowWidth = _windowWidth;
                _dicomImage.WindowCenter = _windowLevel;
                _bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
                pictureBox.Refresh();
                UpdateImageParameter();
            }
        }

        private void CTViewControl_Load(object sender, EventArgs e)
        {
            controlPointList = new List<Point>();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true); //解决闪烁
            this.SetStyle(ControlStyles.Opaque, true); //解决背景重绘问题(设置不绘制窗口背景，因为重绘窗口背景会导致性能底下)
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true); //解决闪烁 

            //this.WindowState = FormWindowState.Maximized;
            //Width = Screen.PrimaryScreen.WorkingArea.Width;
            //Height = Screen.PrimaryScreen.WorkingArea.Height;
            //Location = new Point(0, 0);
            SetPictureBoxWidth();
        }

        private OrientationEnum _OrientationEnum = OrientationEnum.Axial;

        public CTViewControlSingle( Bitmap bitmap)
            : this()
        {
            _bitmapCT = bitmap;
            tbFrame.Maximum =0;
            tbFrame.Minimum = 0;
            SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
            pictureBox.Refresh();
           
        }

        public void SetImage(Bitmap bitmap)
        {
            _bitmapCT = bitmap;
            tbFrame.Maximum = 0;
            tbFrame.Minimum = 0;
            SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
            pictureBox.Refresh();

        }
        private ArrayList _listBitmapFiles;
        public CTViewControlSingle(Bitmap bitmap, ArrayList listBitmapFiles)
            : this()
        {
          
            _bitmapCT = bitmap;
            _listBitmapFiles = listBitmapFiles;
            tbFrame.Maximum = listBitmapFiles.Count - 1;
            tbFrame.Minimum = 0;
             SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
            pictureBox.Refresh();
        }


        public void SetImgae(Bitmap bitmap, ArrayList listBitmapFiles)
        {

            _bitmapCT = bitmap;
            _listBitmapFiles = listBitmapFiles;
            tbFrame.Maximum = listBitmapFiles.Count - 1;
            tbFrame.Minimum = 0;
            SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
            pictureBox.Refresh();
        }



        public CTViewControlSingle(ArrayList listBitmapFiles)
            : this()
        {
            if (listBitmapFiles.Count < 1)
            {
                return;
            }
            _dicomImage = new DicomImage((string)listBitmapFiles[0]);
            _dicomImage.WindowWidth = 1000;
            _dicomImage.WindowCenter = -600;
            _bitmapCT = (Bitmap)LungCare.SupportPlatform.SupportPlatformDAO.Images.ImageOperation.CloneImage(_dicomImage.RenderImage()); 
            _listBitmapFiles = listBitmapFiles;
            tbFrame.Maximum = listBitmapFiles.Count - 1;
            tbFrame.Minimum = 0;
            SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
            pictureBox.Refresh();
        }

        public void SetWindowWidthLevel(int windowWidth , int windowLevel)
        {
            _windowWidth = windowWidth;
            _windowLevel = windowLevel;
            if (_dicomImage == null)
            {
                return;
            }
            _dicomImage.WindowWidth = _windowWidth;
            _dicomImage.WindowCenter = _windowLevel;
            _bitmapCT = (Bitmap)LungCare.SupportPlatform.SupportPlatformDAO.Images.ImageOperation.CloneImage(_dicomImage.RenderImage()); 
            pictureBox.Refresh();
            UpdateImageParameter();
        }
        public void SetImageList(ArrayList listBitmapFiles)
        {
            if (listBitmapFiles.Count < 1)
            {
                return;
            }
            try
            {
                _dicomImage = new DicomImage((string)listBitmapFiles[0]);
                _dicomImage.WindowWidth = _windowWidth;
                _dicomImage.WindowCenter = _windowLevel;
                _bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
               //_bitmapCT =  PresentationImageFactory.Create(new ImageSop((string)listBitmapFiles[0]).Frames[1]).DrawToBitmap(512, 512);
              //  _bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
            }
            catch (Exception ex)
            {
                LoggerEntity.Error(typeof(CTViewControlSingle), ex.ToString());
                return;
            }
            
            _listBitmapFiles = listBitmapFiles;
            tbFrame.Maximum = listBitmapFiles.Count - 1;
            tbFrame.Minimum = 0;
            //tbFrame.Value = 0;
            SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
            pictureBox.Refresh();
            UpdateImageParameter();
        }

        public void SetImageList(ArrayList listBitmapFiles, int windowWidth, int windowLevel, int index)
        {
            if (listBitmapFiles.Count < 1)
            {
                return;
            }
            try
            {
                _windowWidth = windowWidth;
                _windowLevel = windowLevel;
                tbFrame.Value = index;
                if (index < listBitmapFiles.Count)
                {
                    _dicomImage = new DicomImage((string)listBitmapFiles[index]);
                    _dicomImage.WindowWidth = _windowWidth;
                    _dicomImage.WindowCenter = _windowLevel;
                    _bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
                }
                //_bitmapCT =  PresentationImageFactory.Create(new ImageSop((string)listBitmapFiles[0]).Frames[1]).DrawToBitmap(512, 512);
                //  _bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
            }
            catch (Exception ex)
            {
                LoggerEntity.Error(typeof(CTViewControlSingle), ex.ToString());
                return;
            }

            _listBitmapFiles = listBitmapFiles;
            tbFrame.Maximum = listBitmapFiles.Count - 1;
            tbFrame.Minimum = 0;
            //tbFrame.Value = 0;
            SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
            pictureBox.Refresh();
            UpdateImageParameter();
        }
        

        public void SetPicturexBoxImage0(string imagePath , int windowWidth , int windowLevel)
        {
            try
            {
                _windowWidth = windowWidth;
                _windowLevel = windowLevel;
                _dicomImage = new DicomImage((string)imagePath);
                _dicomImage.WindowWidth = _windowWidth;
                _dicomImage.WindowCenter = _windowLevel;
                _bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
                //_listBitmapFiles = listBitmapFiles;
                //tbFrame.Maximum = listBitmapFiles.Count - 1;
                //tbFrame.Minimum = 0;
                SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
                pictureBox.Refresh();
            }
            catch (Exception)
            {
            }
            
        }


        public void SetPicturexBoxImage(DicomImage dicomImage)
        {
            try
            {
                _dicomImage = dicomImage;
                
                _dicomImage.WindowWidth = _windowWidth;
                _dicomImage.WindowCenter = _windowLevel;
                _bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
                //_listBitmapFiles = listBitmapFiles;
                //tbFrame.Maximum = listBitmapFiles.Count - 1;
                tbFrame.Minimum = 0;
                SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
                pictureBox.Refresh();
            }
            catch (Exception)
            {
            }

        }


        public void SetPicturexBoxImage(string dicomImage  , int windowWidth , int windowLevel)
        {
            _windowWidth = windowWidth;
            _windowLevel = windowLevel;
            try
            {
                //_bitmapCT = PresentationImageFactory.Create(new ImageSop(dicomImage).Frames[1]).DrawToBitmap(512, 512);
                _dicomImage = new DicomImage(dicomImage);
                _dicomImage.WindowWidth = windowWidth;
                _dicomImage.WindowCenter = windowLevel;
                _bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
                _bitmapCT = (Bitmap)LungCare.SupportPlatform.SupportPlatformDAO.Images.ImageOperation.CloneImage(_bitmapCT);
                //_listBitmapFiles = listBitmapFiles;
                //tbFrame.Maximum = listBitmapFiles.Count - 1;
                
            }
            catch (Exception ex)
            {
                LoggerEntity.Error(typeof(CTViewControlSingle), ex.ToString());
                return;
                //_dicomImage = new DicomImage(dicomImage);
                //_dicomImage.WindowWidth = windowWidth;
                //_dicomImage.WindowCenter = windowLevel;
                //_bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
            }

            tbFrame.Minimum = 0;
            SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
            pictureBox.Refresh();
            UpdateImageParameter();
        }


        public void SetPicturexBoxImage(string dicomImage, int windowWidth, int windowLevel,Matrix matrix0)
        {
            _windowWidth = windowWidth;
            _windowLevel = windowLevel;
            matrix = matrix0;
            try
            {
                //_bitmapCT = PresentationImageFactory.Create(new ImageSop(dicomImage).Frames[1]).DrawToBitmap(512, 512);
                _dicomImage = new DicomImage(dicomImage);
                _dicomImage.WindowWidth = windowWidth;
                _dicomImage.WindowCenter = windowLevel;
                _bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
                _bitmapCT = (Bitmap)LungCare.SupportPlatform.SupportPlatformDAO.Images.ImageOperation.CloneImage(_bitmapCT);
                //_listBitmapFiles = listBitmapFiles;
                //tbFrame.Maximum = listBitmapFiles.Count - 1;

            }
            catch (Exception ex)
            {
                LoggerEntity.Error(typeof(CTViewControlSingle), ex.ToString());
                return;
                //_dicomImage = new DicomImage(dicomImage);
                //_dicomImage.WindowWidth = windowWidth;
                //_dicomImage.WindowCenter = windowLevel;
                //_bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
            }

            //tbFrame.Minimum = 0;
            //SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
            pictureBox.Refresh();
            UpdateImageParameter();
        }


        public void SetPicturexBoxImage(string dicomImage)
        {
           
            try
            {
                _bitmapCT = PresentationImageFactory.Create(new ImageSop(dicomImage).Frames[1]).DrawToBitmap(512, 512);

            }
            catch (Exception ex)
            {
                LoggerEntity.Error(typeof(CTViewControlSingle), ex.ToString());
                _dicomImage = new DicomImage(dicomImage);
                _dicomImage.WindowWidth = _windowWidth;
                _dicomImage.WindowCenter = _windowLevel;
                _bitmapCT = (Bitmap)_dicomImage.RenderImage().Clone();
            }

            tbFrame.Minimum = 0;
            SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
            pictureBox.Refresh();
            UpdateImageParameter();
        }

        public CTDicomInfo _airwayPatient;
        private DicomImage _dicomImage;
        private Bitmap tempbitmap;

        private unsafe void UpdateCTAxialImage()
        {
            if (_listBitmapFiles != null)
            {
                if (tbFrame.Value < _listBitmapFiles.Count)
                {
                    try
                    {
                        _dicomImage = new DicomImage((string)_listBitmapFiles[(int)tbFrame.Value]);
                        _dicomImage.WindowCenter = _windowLevel;
                        _dicomImage.WindowWidth = _windowWidth;
                        tempbitmap = (Bitmap)_dicomImage.RenderImage().Clone();
                        _bitmapCT = (Bitmap)tempbitmap.Clone();
                        pictureBox.Refresh();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            
        }

        private int _frames;
        public void SetData(CTDicomInfo airwayPatient,int totalNumber)
        {
            _airwayPatient = airwayPatient;
            _frames = totalNumber;
            pictureBox.Refresh();
        }


        public void SetData(CTDicomInfo airwayPatient,int currrentIndex , int totalNumber)
        {
            _airwayPatient = airwayPatient;
            _frames = totalNumber;
            tbFrame.Value = currrentIndex;
            pictureBox.Refresh();
        }

        public void UnDoReactangle()
        {
            if (listRectangle != null && listRectangle.Count>0)
            {
                listRectangle.RemoveAt(listRectangle.Count - 1);
                pictureBox.Refresh();
            }
        }
        public void Refresh1()
        {
            this.Focus();
            pictureBox.Focus();
            pictureBox.Refresh();
        }

        private Pen pen;
        private System.Drawing.Pen _pen;
        private Matrix textMatrix = new Matrix();
        private string upOrientationEnumText, downOrientationEnumText, leftOrientationEnumText, rightOrientationEnumText;
        //sSystem.Drawing.Color Mycolor = System.Drawing.Color.FromArgb(128, Color.Red);//说明：1-（128/255）=1-0.5=0.5 透明度为0.5，即50%
        System.Drawing.SolidBrush addPointCursorBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(128, Color.Red));
        System.Drawing.SolidBrush deletePointCursorBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(128, Color.Green));
        Stopwatch time = new Stopwatch();
        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {

            time.Restart();
            //Console.WriteLine("pictureBox_Paint");
            if (matrix != null)
            {
                e.Graphics.Transform = matrix;
            }
            //e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            if (_bitmapCT != null)
            {
                try
                {
                    e.Graphics.DrawImage(_bitmapCT, 0, 0);
                }
                catch (Exception ex)
                {
                    LoggerEntity.Error(typeof(CTViewControlSingle), ex.ToString());
                }
            }

            pen = new Pen(Brushes.Red, 1 / matrix.Elements[3]);
            if (_point2D != null)
            {
                pen.DashStyle = DashStyle.Dash;
                pen.DashPattern = new float[] { 2, 5 };
                e.Graphics.DrawLine(pen, new Point(0, (int)_point2D[1]), new Point(_bitmapCT.Width, (int)_point2D[1]));
                e.Graphics.DrawLine(pen, new Point((int)_point2D[0], 0), new Point((int)_point2D[0], _bitmapCT.Height));
            }
            if (isDrawRectangle)
            {
                pen = new Pen(Brushes.Red, 3 / matrix.Elements[3]);
                pen.DashStyle = DashStyle.Solid;
                if (currRectagle.Width!=0 && currRectagle.Height!=0)
                {
                    e.Graphics.DrawRectangle(pen, currRectagle.X, currRectagle.Y, currRectagle.Size.Width, currRectagle.Size.Height);
                }
                foreach (var item in listRectangle)
                {
                    if (item.studyUID == _airwayPatient.StudyInstanceUID && item.seriesUID == _airwayPatient.SeriesInstanceUID && item.Frame == (int)tbFrame.Value)
                    {
                        e.Graphics.DrawRectangle(pen, item.Rectagle.X, item.Rectagle.Y, item.Rectagle.Size.Width, item.Rectagle.Size.Height);
                    }
                }

            }


            e.Graphics.Transform = textMatrix;
            e.Graphics.DrawString(upOrientationEnumText, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(pictureBox.Width / 2, 2));
            e.Graphics.DrawString(downOrientationEnumText, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(pictureBox.Width / 2, pictureBox.Height - 40));
            e.Graphics.DrawString(leftOrientationEnumText, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, pictureBox.Height / 2));
            //e.Graphics.DrawString((((int)tbFrame.Value)+1).ToString(), new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, pictureBox.Height -20));
            e.Graphics.DrawString(rightOrientationEnumText, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(pictureBox.Width - 30, pictureBox.Height / 2));

            if (_airwayPatient != null)
            {
                e.Graphics.DrawString(_airwayPatient.PatientName, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, 2));
                e.Graphics.DrawString(_airwayPatient.PatientSex, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, 22));
                e.Graphics.DrawString(_airwayPatient.InstitutionName, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, 42));
                e.Graphics.DrawString(_airwayPatient.PatientAge, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, 62));
                e.Graphics.DrawString(_airwayPatient.AcquisitionDate, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, 82));
                e.Graphics.DrawString(_airwayPatient.SliceThickness.ToString() + "mm", new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, 102));
                e.Graphics.DrawString(((int)tbFrame.Value + 1).ToString() + "/" + _frames.ToString(), new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, 122));
            }
            e.Graphics.DrawString("L   :" + _windowLevel, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(pictureBox.Width - 100, pictureBox.Height - 70));
            e.Graphics.DrawString("W :" + _windowWidth, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(pictureBox.Width - 100, pictureBox.Height - 50));




        }


        private void UpdateImage()
        {
            Console.WriteLine("UpdateImage");
            switch (_OrientationEnum)
            {
                case OrientationEnum.Sagittal:
                    break;
                case OrientationEnum.Coronal:
                    break;
                case OrientationEnum.Axial:
                    UpdateCTAxialImage();
                    break;
                case OrientationEnum.Unknown:
                    break;
                default:
                    break;
            }
            pictureBox.Refresh();

            UpdateImageParameter();
        }

        private void tbFrame_Scroll(object sender, EventArgs e)
        {
            UpdateImage();
           
        }
        void tbFrame_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateImage();
            UpdateImageParameter();
            //throw new NotImplementedException();
        }

        private PointF startPointOnImageCoord, startPointOnControlCoord;
        private PointF startPointOnImageCoord1, startPointOnControlCoord1;
        private PointF endPointOnImageCoord,endPointOnControlCoord;
        private LineEntity currentLine;
        private List<LineEntity> listLines = new List<LineEntity>();
        private DateTime lastClickTimeStamp = DateTime.MinValue;
        void pictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((DateTime.Now - lastClickTimeStamp).TotalMilliseconds < 300 || System.Windows.Forms.Control.ModifierKeys == Keys.Control)
            {


                if (e.Delta > 0)
                {
                    if (tbFrame.Value > tbFrame.Minimum)
                    {
                        tbFrame.Value--;

                        UpdateImage();
                    }
                }
                else
                {
                    if (tbFrame.Value < tbFrame.Maximum)
                    {
                        tbFrame.Value++;
                        UpdateImage();
                    }
                }
                
            }

            lastClickTimeStamp = DateTime.Now;
            //throw new NotImplementedException();
        }

        private bool isDrawCloedCurve = false;
        private void btnDrawClosedCurve_Click(object sender, EventArgs e)
        {
            isDrawCloedCurve = true;
            this.Cursor = Cursors.Cross;
            controlPointList = new List<Point>();
            isAddPoint = false;
            isDeletePoint = false;
            isSelectConnectPoints = false;
        }

        private unsafe void pictureBox_DoubleClick(object sender, EventArgs e)
        {
            SetPictureBoxWidth();
        }

        private bool isComputing = false;

        private List<Point> controlPointList = new List<Point>();
        private Point cursorPoint;
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            (sender as PictureBox).Focus();
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                this.contextMenuStrip1.Show(MousePosition);
            }
            startPointOnControlCoord = new System.Drawing.Point(e.X, e.Y);
            startPointOnImageCoord1 = startPointOnImageCoord = Control2Image(matrix, new System.Drawing.Point(e.X, e.Y));
            currRectagle = new Rectangle((int)startPointOnImageCoord1.X,(int) startPointOnImageCoord1.Y, 0 , 0);
            //currRectagle = new Rectangle(new Point(0, 0), new Size(0, 0));
        }

        private int radius = 0;
        private bool _isChangeWindowLevelAndWidth = false;
        public void startWindowLevelAndWidth()
        {
            SetAllOperationDisable();
            _isChangeWindowLevelAndWidth = true;
            
            this.Cursor = System.Windows.Forms.Cursors.NoMove2D;

        }

        public void stopWindowLevelAndWidth()
        {
            SetAllOperationDisable();
            _isChangeWindowLevelAndWidth = false;

            this.Cursor = System.Windows.Forms.Cursors.Arrow;

        }

        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2 , 2) + Math.Pow(y1 - y2 , 2));
        }

        private List<int> _previousPoints = new List<int>();
        private bool _hasSaved = true;
        public bool HasSaved
        {
            get
            {
                return _hasSaved;
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isChangeWindowLevelAndWidth)
            {
                this.Cursor = Cursors.NoMove2D;
            }
            else if (isPanning)
            {
                this.Cursor = Cursors.Hand;
            }
            else if (isZoom)
            {
                this.Cursor = Cursors.SizeAll;
            }
            PointF p = Control2Image(matrix, new Point(e.X, e.Y));

            //if ((System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control && e.Button == System.Windows.Forms.MouseButtons.Left)
            if ((isZoom || (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control) && e.Button == MouseButtons.Left)
            {
                this.Cursor = System.Windows.Forms.Cursors.SizeAll;
                System.Drawing.PointF startPointOnControlCoordBefore = Image2Control(matrix, startPointOnImageCoord);

                int dx = e.X - (int)startPointOnControlCoord.X;
                int dy = e.Y - (int)startPointOnControlCoord.Y;
                matrix.Scale((300 + dx) / 300f, (300 + dx) / 300f);

                System.Drawing.PointF startPointOnControlCoordAfter = Image2Control(matrix, startPointOnImageCoord);

                float diffX = (startPointOnControlCoordAfter.X - startPointOnControlCoordBefore.X) / matrix.Elements[3];
                float diffY = (startPointOnControlCoordAfter.Y - startPointOnControlCoordBefore.Y) / matrix.Elements[3];

                matrix.Translate(-diffX, -diffY);
                UpdateImageParameter();
            }

            //if ((System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control && e.Button == System.Windows.Forms.MouseButtons.Right)
            if ((isPanning || (System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift) && e.Button == MouseButtons.Left)
            {
                this.Cursor = Cursors.Hand;
                matrix.Invert();
                var pts = new PointF[] { startPointOnControlCoord, new PointF(e.X, e.Y) };
                matrix.TransformPoints(pts);
                matrix.Invert();
                matrix.Translate(pts[1].X - pts[0].X, pts[1].Y - pts[0].Y);
                UpdateImageParameter();
            }

            if (_isChangeWindowLevelAndWidth == true && e.Button == MouseButtons.Left)
            {
                
                _windowLevel += (int)(e.X - startPointOnControlCoord.X);
                _windowWidth += (int)(e.Y - startPointOnControlCoord.Y);
                UpdateImage();
                //if (WindowWidthLevelHandler != null)
                //{
                //    WindowWidthLevelHandler(this, new WindowWidthLevelArgs()
                //    {
                //        WindowWidth = _windowWidth,
                //        WindowLevel = _windowLevel,
                //        Index  = (int)tbFrame.Value,
                //        Matrix = matrix
                //    });
                //}
            }

            if (isDrawRectangle && e.Button == MouseButtons.Left)
            {
                int minX = Math.Min((int)startPointOnImageCoord1.X, (int)startPointOnImageCoord.X);
                int minY = Math.Min((int)startPointOnImageCoord1.Y, (int)startPointOnImageCoord.Y);
                int maxX = Math.Max((int)startPointOnImageCoord1.X, (int)startPointOnImageCoord.X);
                int maxY = Math.Max((int)startPointOnImageCoord1.Y, (int)startPointOnImageCoord.Y);
                currRectagle = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            }
            startPointOnControlCoord = new PointF(e.X, e.Y);
            startPointOnImageCoord = Control2Image(matrix, startPointOnControlCoord);
            pictureBox.Refresh();
        }

        private bool isZoom = false;
        private bool isPanning = false;
        private bool isMeasure = false;
        private bool isDrawRectangle = false;

        public void StartDrawRectangle()
        {
            SetAllOperationDisable();
            isDrawRectangle = true;

            this.Cursor = Cursors.Arrow;
        }

        public void StopDrawRectangle()
        {
            SetAllOperationDisable();
            isDrawRectangle = false;

            this.Cursor = Cursors.Arrow;
        }
        public void StartZoom()
        {
            SetAllOperationDisable();
            isZoom = true;
            
            this.Cursor = Cursors.SizeAll;
        }

        public void StartPanning()
        {
            SetAllOperationDisable();
            isPanning = true;
            
            this.Cursor = Cursors.Hand;
        }

        public void StopZoom()
        {
            SetAllOperationDisable();
            isZoom = false;

            this.Cursor = Cursors.Arrow;
        }

        public void StopPanning()
        {
            SetAllOperationDisable();
            isPanning = false;

            this.Cursor = Cursors.Arrow;
        }

        public void StartMeasure()
        {
            SetAllOperationDisable();
            isMeasure = true;
            isAddPoint = false; 
            isDeletePoint = false;
            this.Cursor = Cursors.Arrow;
        }


        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (currRectagle != null)
            {
                if (_airwayPatient != null)
                {
                    RectangleEntity rectangleEntity = new RectangleEntity();
                    rectangleEntity.Rectagle = new Rectangle(currRectagle.X, currRectagle.Y, currRectagle.Width, currRectagle.Height);
                    rectangleEntity.studyUID = _airwayPatient.StudyInstanceUID;
                    rectangleEntity.seriesUID = _airwayPatient.SeriesInstanceUID;
                    rectangleEntity.Frame = (int)tbFrame.Value;

                    listRectangle.Add(rectangleEntity);
                }
            }
            //listRectangle.Add(new Rectangle(currRectagle.X , currRectagle.Y , currRectagle.Width , currRectagle.Height));
            currRectagle = new Rectangle(new Point(0 , 0) , new Size(0 , 0)) ;
            //this.Cursor = System.Windows.Forms.Cursors.Arrow;
            startPointOnControlCoord = new PointF(e.X, e.Y);
            startPointOnImageCoord = Control2Image(matrix, startPointOnControlCoord);
            pictureBox.Refresh();
        }

        public Bitmap ScreenShot()
        {
            Bitmap bm = new Bitmap(pictureBox.Width, pictureBox.Height);
            Rectangle r = new Rectangle(0, 0, pictureBox.Width, pictureBox.Height);
            pictureBox.DrawToBitmap(bm, r);
            //bm.Save("文件名");

            return bm;
        }


        public Bitmap ScreenShot(string saveFile)
        {
            Bitmap bm = new Bitmap(pictureBox.Width, pictureBox.Height);
            Rectangle r = new Rectangle(0, 0, pictureBox.Width, pictureBox.Height);
            pictureBox.DrawToBitmap(bm, r);
            bm.Save(saveFile);

            return bm;
        }
        public void SetPictureBoxWidth()
        {
            if (this.Width > this.Height)
            {
               // splitContainer1.SplitterDistance = pictureBox.Height;
                pictureBox.Width = pictureBox.Height;
            }
            SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);

            pictureBox.Refresh();

            UpdateImageParameter();
        }


        private void UpdateImageParameter()
        {
            if (WindowWidthLevelHandler != null)
            {
                WindowWidthLevelHandler(this, new WindowWidthLevelArgs()
                {
                    WindowWidth = _windowWidth,
                    WindowLevel = _windowLevel,
                    Index = (int)tbFrame.Value,
                    Matrix = matrix
                });
            }
        }

        public void RefreshWWWL()
        {
            _windowLevel = -600;
            _windowWidth = 1000;
            UpdateImage();
        }
        private void pictureBox_Resize(object sender, EventArgs e)
        {
            if (matrix == null || _bitmapCT == null)
            {
                return;
            }      
        }

        private bool isZoomIn = false;

        private void pbZoomIn_MouseDown(object sender, MouseEventArgs e)
        {
            isZoomIn = true;
            while (isZoomIn)
            {
                ZoomIn();
            }
        }


        public void ZoomIn()
        {
            
                if (_point2D == null)
                {
                    return;
                }
                startPointOnImageCoord = new PointF((float)_point2D[0], (float)_point2D[1]);
                System.Drawing.PointF startPointOnControlCoordBefore = Image2Control(matrix, startPointOnImageCoord);

                int dx = -15;
                int dy = -15;
                matrix.Scale((300 + dx) / 300f, (300 + dx) / 300f);

                System.Drawing.PointF startPointOnControlCoordAfter = Image2Control(matrix, startPointOnImageCoord);

                float diffX = (startPointOnControlCoordAfter.X - startPointOnControlCoordBefore.X) / matrix.Elements[3];
                float diffY = (startPointOnControlCoordAfter.Y - startPointOnControlCoordBefore.Y) / matrix.Elements[3];

                matrix.Translate(-diffX, -diffY);
                pictureBox.Refresh();
                Thread.Sleep(50);
                Application.DoEvents();
            
        }
        private void pbZoomIn_MouseUp(object sender, MouseEventArgs e)
        {
            isZoomIn = false;
        }

        private bool isZoomOut = false;

        public void pbZoomOut_MouseDown(object sender, MouseEventArgs e)
        {
            isZoomOut = true;
            while (isZoomOut)
            {

                ZoomOut();
            }
        }

        public void ZoomOut()
        {
                if (_point2D == null)
                {
                    return;
                }
                startPointOnImageCoord = new PointF((float)_point2D[0], (float)_point2D[1]);
                System.Drawing.PointF startPointOnControlCoordBefore = Image2Control(matrix, startPointOnImageCoord);

                int dx = 15;
                int dy = 15;
                matrix.Scale((300 + dx) / 300f, (300 + dx) / 300f);

                System.Drawing.PointF startPointOnControlCoordAfter = Image2Control(matrix, startPointOnImageCoord);

                float diffX = (startPointOnControlCoordAfter.X - startPointOnControlCoordBefore.X) / matrix.Elements[3];
                float diffY = (startPointOnControlCoordAfter.Y - startPointOnControlCoordBefore.Y) / matrix.Elements[3];

                matrix.Translate(-diffX, -diffY);
                pictureBox.Refresh();
                Thread.Sleep(50);
                Application.DoEvents();
            
        }

        private void pbZoomOut_MouseUp(object sender, MouseEventArgs e)
        {
            isZoomOut = false;
        }
        private void pbReload_Click(object sender, EventArgs e)
        {
        }

        private bool isMoveLeft = false;
        private bool isMoveRight = false;
        private bool isMoveDown = false;
        private bool isMoveUp = false;
        private void pbLeft_Click(object sender, EventArgs e)
        {
            isMoveLeft = true;
        }
        private void pbLeft_MouseDown(object sender, MouseEventArgs e)
        {
            isMoveLeft = true;
            while (isMoveLeft)
            {
                if (_point2D == null)
                {
                    return;
                }
                matrix.Translate(-10, 0);
                pictureBox.Refresh();
                Thread.Sleep(50);
                Application.DoEvents();
            }
        }

        private void pbLeft_MouseUp(object sender, MouseEventArgs e)
        {
            isMoveLeft = false;
        }







        private void pbRight_MouseDown(object sender, MouseEventArgs e)
        {
            isMoveRight = true;
            while (isMoveRight)
            {
                if (_point2D == null)
                {
                    return;
                }
                matrix.Translate(10, 0);
                pictureBox.Refresh();
                Thread.Sleep(50);
                Application.DoEvents();
            }
        }

        private void pbRight_MouseUp(object sender, MouseEventArgs e)
        {
            isMoveRight = false;
        }


        private void pbUp_MouseDown(object sender, MouseEventArgs e)
        {
            isMoveUp = true;
            while (isMoveUp)
            {
                if (_point2D == null)
                {
                    return;
                }
                matrix.Translate(0, -10);
                pictureBox.Refresh();
                Thread.Sleep(50);
                Application.DoEvents();
            }
        }


        private void pbUp_MouseUp(object sender, MouseEventArgs e)
        {
            isMoveUp = false;
        }

        private void pbDown_MouseDown(object sender, MouseEventArgs e)
        {
            isMoveDown = true;
            while (isMoveDown)
            {
                if (_point2D == null)
                {
                    return;
                }
                matrix.Translate(0, 10);
                pictureBox.Refresh();
                Thread.Sleep(50);
                Application.DoEvents();
            }
        }

        private void pbDown_MouseUp(object sender, MouseEventArgs e)
        {
            isMoveDown = false;
        }


        public static System.Drawing.PointF Control2Image(System.Drawing.Drawing2D.Matrix image2controlMatrix, System.Drawing.PointF controlP)
        {
            PointF[] point = new PointF[] { controlP };
            image2controlMatrix.Invert();
            image2controlMatrix.TransformPoints(point);
            image2controlMatrix.Invert();
            return point[0];
        }

        public static System.Drawing.Point Control2Image(System.Drawing.Drawing2D.Matrix image2controlMatrix, System.Drawing.Point controlP)
        {
            Point[] point = new Point[] { controlP };
            image2controlMatrix.Invert();
            image2controlMatrix.TransformPoints(point);
            image2controlMatrix.Invert();
            return point[0];
        }

        public static System.Drawing.PointF Image2Control(System.Drawing.Drawing2D.Matrix image2controlMatrix, System.Drawing.PointF controlP)
        {
            PointF[] point = new PointF[] { controlP };
            image2controlMatrix.TransformPoints(point);
            return point[0];
        }

        public void SetPictureBoxScaleLevel(Bitmap bitmap, PictureBox pictureBox, ref System.Drawing.Drawing2D.Matrix matrix)
        {
            if (bitmap == null)
            {
                return;
            }

            Bitmap bitmap1 = (Bitmap)bitmap.Clone();
            Console.WriteLine(pictureBox.Width + "  " + pictureBox.Height);
            matrix = new System.Drawing.Drawing2D.Matrix();
            float a = (float)pictureBox.Width / bitmap1.Width;
            float b = (float)pictureBox.Height / bitmap1.Height;
            if (a <= b)
            {
                matrix.Scale(a, a);
                PointF start = new PointF((float)(bitmap1.Width * a / 2), (float)(bitmap1.Height * a / 2));
                PointF end = new PointF((float)(pictureBox.Width / 2), (float)(pictureBox.Height / 2));


                matrix.Invert();
                var pts = new PointF[] { start, end };
                matrix.TransformPoints(pts);
                matrix.Invert();
                matrix.Translate(pts[1].X - pts[0].X, pts[1].Y - pts[0].Y);
            }
            else
            {
                Console.WriteLine("width");
                matrix.Scale(b, b);

                PointF start = new PointF((float)(bitmap1.Width * b / 2), (float)(bitmap1.Height * b / 2));
                PointF end = new PointF((float)(pictureBox.Width / 2), (float)(pictureBox.Height / 2));
                matrix.Invert();
                var pts = new PointF[] { start, end };
                matrix.TransformPoints(pts);
                matrix.Invert();
                matrix.Translate(pts[1].X - pts[0].X, pts[1].Y - pts[0].Y);
                

            }
            UpdateImageParameter();
        }

        private bool isAddPoint = false , isDeletePoint = false;
        //private void btnAddPoint_Click(object sender, EventArgs e)

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.O)
            {
               // btnOpenCT.PerformClick();
            }

            if (keyData == Keys.M)
            {
                //btnOpenAirway.PerformClick();
            }
            if (keyData == Keys.C)
            {
                //btnDrawClosedCurve.PerformClick();
            }
            if (keyData == Keys.A)
            {
                //btnAddPoint.PerformClick();
            }

            if (keyData == Keys.D)
            {
                //btnDeletePoint.PerformClick();
            }

            if (keyData == Keys.L)
            {
                //btnDrawPolyline.PerformClick();
            }
            if (keyData == Keys.Down || keyData == Keys.X)
            {
                if (tbFrame.Value < tbFrame.Maximum)
                {
                    tbFrame.Value++;
                    UpdateImage();
                }
            }

            if (keyData == Keys.Up || keyData == Keys.Z)
            {
                if (tbFrame.Value > tbFrame.Minimum)
                {
                    tbFrame.Value--;
                    UpdateImage();
                }
            }

            if (keyData == Keys.PageUp)
            {
                //if (trackBarDrawRadius.Value < trackBarDrawRadius.Maximum)
                //{
                //    trackBarDrawRadius.Value++;
                //    radius++;
                //}
            }

            if (keyData == Keys.PageDown)
            {
                //if (trackBarDrawRadius.Value > trackBarDrawRadius.Minimum)
                //{
                //    trackBarDrawRadius.Value--;
                //    radius--;
                //}
            }
            if (keyData == Keys.E)
            {
               
            }
            return true;
            //return base.ProcessCmdKey(ref msg, keyData);
        }
        //private void btnDeletePoint_Click(object sender, EventArgs e)
        public void DeletePoint()
        {
            SetAllOperationDisable();
            isDeletePoint = true;
            isAddPoint = false;
            this.Cursor = Cursors.Cross;
        }





        private bool isDrawLesion = false;

        private void SetAllOperationDisable()
        {
            isZoom = false;
            isPanning = false;
            _isChangeWindowLevelAndWidth = false;
            //isAddPoint = false;
            //isDrawLesion = false;
            isDeletePoint = false;
            isDrawCloedCurve = false;
            isDrawPolyline = false;
            isSelectConnectPoints = false;
            isMeasure = false;
            isDrawRectangle = false;
        }


        private bool isHasOtherOperation()
        {
            return isZoom || isPanning || _isChangeWindowLevelAndWidth;
        }
        //private void btnDrawLesion_Click(object sender, EventArgs e)

        private void rbAxial_CheckedChanged(object sender, EventArgs e)
        {
            //if (rbAxial.Checked)
            {
                this._OrientationEnum = OrientationEnum.Axial;
            }
        }


        private void rbSagital_CheckedChanged(object sender, EventArgs e)
        {
            //if (rbSagital.Checked)
            {
                this._OrientationEnum = OrientationEnum.Sagittal;
            }
        }


        private void trackBarDrawRadius_Scroll(object sender, EventArgs e)
        {
            //radius = trackBarDrawRadius.Value;
        }

        public void SetDrawRadius(int _radius)
        {
            radius = _radius;
            pictureBox.Refresh();
        }


        private void rbCoronal_CheckedChanged(object sender, EventArgs e)
        {
            {
                this._OrientationEnum = OrientationEnum.Coronal;
            }
        }


        private bool isDrawPolyline = false;
        private void btnDrawPolyline_Click(object sender, EventArgs e)
        {
            isDrawPolyline = true;
            isAddPoint = false;
            isDeletePoint = false;
            isDrawCloedCurve = false;
            isSelectConnectPoints = false;
            controlPointList = new List<Point>();
            this.Cursor = Cursors.Cross;
        }


        private bool isSelectConnectPoints = false;
        private int[] _pointA, _pointB;

        void RenderWindowInteractor_ModifiedEvt(vtkObject sender, vtkObjectEventArgs e)
        {
        }
        public void UpdateCamera(double[] cameraPosition, double[] focalPosition)
        {
            pictureBox.Refresh();
        }

        public event EventHandler<PickEventArg> Picked;
        void _rendererPackage_Picked(object sender, PickEventArg e)
        {
            if ((DateTime.Now - lastClickTimeStamp).TotalMilliseconds < 300 || System.Windows.Forms.Control.ModifierKeys == Keys.Control)
            {
                
                    UpdateCamera(e.Position, e.Position);

                    
                   
            }
            lastClickTimeStamp = DateTime.Now;
        }

        public event EventHandler<SegemntFileEventArgs> SavedSegment;


        private bool _isDrawUnder400CT = false;


    }

    
}

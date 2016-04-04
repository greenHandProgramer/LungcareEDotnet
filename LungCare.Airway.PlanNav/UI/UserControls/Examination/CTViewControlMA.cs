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
using LungCare.Airway.WinformUIControls.Entities;
using LungCare.SupportPlatform.SupportPlatformDAO.VTK;
using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.SupportPlatformDAO;
using LungCare.SupportPlatform.SupportPlatformDAO.Utils;
using LungCare.SupportPlatform.SupportPlatformDAO.Files;
using LungCare.SupportPlatform.SupportPlatformDAO.Logs;
using AirwayCT.Entity;
using LungCare.SupportPlatform.SupportPlatformDAO.Algorithm;

namespace LungCare.SupportPlatform.UI.UserControls.Examination
{
    public partial class CTViewControlMA : System.Windows.Forms.UserControl
    {
        
        public CTViewControlMA()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            pictureBox.MouseWheel += new MouseEventHandler(pictureBox_MouseWheel);
            this.Name = "CTViewControl";
            //this.BackColor = Color.Black;
            this.ResumeLayout(false);


            splitContainer1.SplitterDistance = splitContainer1.Width - 2;
            toolStripItemEnable = new ToolStripMenuItem();
            toolStripItemEnable.Image = global::LungCare.SupportPlatform.Properties.Resources.qiu;
            toolStripItemEnable.Text = "开";
            toolStripItemEnable.Click += new EventHandler(toolStripItemEnable_Click);

            toolStripItemUnEnable = new ToolStripMenuItem();
            toolStripItemUnEnable.Text = "关";
            toolStripItemUnEnable.Click += new EventHandler(toolStripItemUnEnable_Click);

            MenuStrip menuStrip = new MenuStrip();
            //splitButton1.SplitMenu = menuStrip
            ContextMenuStrip contextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            contextMenuStrip.Items.Add(toolStripItemEnable);
            contextMenuStrip.Items.Add(toolStripItemUnEnable);
            //splitBtnEnableJump.SplitMenuStrip = contextMenuStrip;
            //pbHelp.MouseHover += new EventHandler(pbHelp_MouseHover);
            //pictureBox7.SizeMode = PictureBoxSizeMode.Zoom;
            //pictureBox7.Image = global::ImagedataToMPR.Properties.Resources.reload;

            //pictureBox8.SizeMode = PictureBoxSizeMode.Zoom;
            //pictureBox8.Image = global::ImagedataToMPR.Properties.Resources.;
            string helpContent = "\r\n  放缩\r\n" + "鼠标左键按住放缩键不放，或者点击图像并按住Control + 鼠标左键移动，视图将一直放缩，直到鼠标松开\r\n"
                + "  滚动\r\n" + "鼠标拖动拉条或者点击图像滑动鼠标滚轮\r\n"
                + "  平移\r\n" + "鼠标左键按住平移键不放，或者点击图像并按住Control + 鼠标右键移动，视图将一直平移，直到鼠标松开\r\n";

            tbFrame = this.trackbarUserControl1.FindName("S1") as Slider;
            tbFrame.SmallChange = 1;
            //btnEnableJump = this.toggleBtn1.FindName("tgbtn") as System.Windows.Controls.Primitives.ToggleButton;
            //btnEnableJump.Checked += new System.Windows.RoutedEventHandler(btnEnableJump_Checked);
            //btnEnableJump.Unchecked += new System.Windows.RoutedEventHandler(btnEnableJump_Unchecked);
            tbFrame.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(tbFrame_ValueChanged);

            
        }

        void btnEnableJump_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            isJumpToEnable = true;
            Console.WriteLine(isJumpToEnable);
            //throw new NotImplementedException();
        }

        void btnEnableJump_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            isJumpToEnable = false;
            Console.WriteLine(isJumpToEnable);
            
            //throw new NotImplementedException();
        }

        private System.Windows.Controls.Primitives.ToggleButton btnEnableJump;
        private Size maxButtonSize = new Size(35, 35);
        private Slider tbFrame;
        private ToolStripMenuItem toolStripItemEnable, toolStripItemUnEnable;
        void toolStripItemUnEnable_Click(object sender, EventArgs e)
        {
            isJumpToEnable = false;
            toolStripItemEnable.Image = null;
            toolStripItemUnEnable.Image = global::LungCare.SupportPlatform.Properties.Resources.qiu;
            //splitBtnEnableJump.Text = "N";
            //throw new NotImplementedException();
        }

        void toolStripItemEnable_Click(object sender, EventArgs e)
        {
            isJumpToEnable = true;
            toolStripItemEnable.Image = global::LungCare.SupportPlatform.Properties.Resources.qiu;
            toolStripItemUnEnable.Image = null;
            //splitBtnEnableJump.Text = "Y";
            //throw new NotImplementedException();
        }




        private UnsafeBitmap _unsafeBmpCT , _unsafeBmpAirway;
        private Bitmap _bitmapCT , _bitmapAirway;
        private int windowLevel = -600, windowWidth = 1000;
        private int[] _dimensionsCT;
        private double[] _dataSpacingCT;
        private short[] _dicomAlldata ;
        private byte[] _airwayData;
        private FastImageDataFetcherShort _fastImageDataFetcher;
        private System.Drawing.Drawing2D.Matrix matrix = new Matrix();
        private double[] _point3D;
        private double[] _point2D;
        private vtkImageData _imageAllData ,_airwayImageData;
        private void CTViewControl_Load(object sender, EventArgs e)
        {
            controlPointList = new List<Point>();
            //DialogResult dr = System.Windows.Forms.MessageBox.Show("是否打开上次的数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (dr == DialogResult.Yes)
            //{
            //    string lastCTPath = File.ReadAllText("lastOpenCTFilePath.txt");
            //    if (!string.IsNullOrEmpty(lastCTPath))
            //    {
            //        LoadCTData(lastCTPath);
            //    }
            //    string lastAirwayPath = File.ReadAllText("lastOpenAirwayFilePath.txt");
            //    if (!string.IsNullOrEmpty(lastAirwayPath))
            //    {
            //        LoadAirwayData(lastAirwayPath);
            //    }

            //    if (File.Exists("lastGenerateAirwayVtpFileName.txt"))
            //    {
            //        string lastGenerateAirwayVtpFileName = File.ReadAllText("lastGenerateAirwayVtpFileName.txt");
            //        if (!string.IsNullOrEmpty(lastGenerateAirwayVtpFileName))
            //        {
            //            _connectiveAirwayVtpNewFilePath = lastGenerateAirwayVtpFileName;
            //        }
            //    }

            //    if (File.Exists("lastGenerateLesionVtpFileName.txt"))
            //    {
            //        string lastGenerateLesionVtpFileName = File.ReadAllText("lastGenerateLesionVtpFileName.txt");
            //        if (!string.IsNullOrEmpty(lastGenerateLesionVtpFileName))
            //        {
            //            _lesionVtpNewFilePath = lastGenerateLesionVtpFileName;
            //        }
            //    }
            //}

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true); //解决闪烁
            this.SetStyle(ControlStyles.Opaque, true); //解决背景重绘问题(设置不绘制窗口背景，因为重绘窗口背景会导致性能底下)
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true); //解决闪烁 

            //this.WindowState = FormWindowState.Maximized;
            Width = Screen.PrimaryScreen.WorkingArea.Width;
            Height = Screen.PrimaryScreen.WorkingArea.Height;
            Location = new Point(0, 0);
        }


        private OrientationEnum _OrientationEnum = OrientationEnum.Unknown;


        public CTViewControlMA(OrientationEnum OrientationEnum, vtkImageData imageData)
            : this()
        {
            if (OrientationEnum == OrientationEnum.Unknown)
            {
                throw new ArgumentException(@"OrientationEnum cannot be unknown", "OrientationEnum");
            }

           
            InitialData(OrientationEnum, imageData);
           
            //tbFrame.Value = tbFrame.Maximum / 2;


        }

        private string workDirectory, _ctOpenFileName, _airwayOpenFilePath, _airwayMhaNewFilePath, _connectiveAirwayVtpNewFilePath, _lesionMhaNewFilePath, _lesionVtpNewFilePath;
        private void InitialData(OrientationEnum OrientationEnum, vtkImageData imageAllData )
        {
            _imageAllData = imageAllData;
            _OrientationEnum = OrientationEnum;
            // double[] bounds = imageData.GetBounds();
            _dimensionsCT = _imageAllData.GetDimensions();
            _dataSpacingCT = _imageAllData.GetSpacing();
            matrix = new Matrix();
            _point3D = new double[] { _dimensionsCT[0] / 2, _dimensionsCT[1] / 2, _dimensionsCT[2] / 2 };
            _fastImageDataFetcher = new FastImageDataFetcherShort(_imageAllData);
            _dicomAlldata = _fastImageDataFetcher.GetAllScalar();

            switch (_OrientationEnum)
            {
                case OrientationEnum.Sagittal:
                    _unsafeBmpCT = new UnsafeBitmap(_dimensionsCT[1], _dimensionsCT[2]);

                    tbFrame.Maximum = _dimensionsCT[0] - 1;
                    tbFrame.Value = tbFrame.Maximum / 2;
                    UpdateCTSagitalImage();
                    _point2D = PositionOperation.Position_3D_2_SagitalCT(_point3D, _dataSpacingCT, _unsafeBmpCT.Bitmap.Width, _unsafeBmpCT.Bitmap.Height);
                    upOrientationEnumText = "上";
                    downOrientationEnumText = "下";
                    leftOrientationEnumText = "前";
                    rightOrientationEnumText = "后";
                    break;
                case OrientationEnum.Coronal:
                    _unsafeBmpCT = new UnsafeBitmap(_dimensionsCT[0], _dimensionsCT[2]);
                    tbFrame.Maximum = _dimensionsCT[1] - 1;
                    tbFrame.Value = tbFrame.Maximum / 2;
                    UpdateCTCoronalImage();
                    _point2D = PositionOperation.Position_3D_2_ConoralCT(_point3D, _dataSpacingCT, _unsafeBmpCT.Bitmap.Width, _unsafeBmpCT.Bitmap.Height);
                    upOrientationEnumText = "上";
                    downOrientationEnumText = "下";
                    leftOrientationEnumText = "左";
                    rightOrientationEnumText = "右";
                    break;
                case OrientationEnum.Axial:
                    _unsafeBmpCT = new UnsafeBitmap(_dimensionsCT[0], _dimensionsCT[1]);
                    tbFrame.Maximum = _dimensionsCT[2] - 1;
                    tbFrame.Value = tbFrame.Maximum / 2;
                    UpdateCTAxialImage();
                    _point2D = PositionOperation.Position_3D_2_Axial_CT(_point3D, _dataSpacingCT, _unsafeBmpCT.Bitmap.Width, _unsafeBmpCT.Bitmap.Height);
                    upOrientationEnumText = "前";
                    downOrientationEnumText = "后";
                    leftOrientationEnumText = "右";
                    rightOrientationEnumText = "左";
                    break;
                case OrientationEnum.Unknown:
                    break;
                default:
                    break;
            }
           
            SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);
            pictureBox.Refresh();
        }
        //private byte[, ,] Color3D;
        private List<Bitmap> listAirwayBitmap;
        public unsafe void ChangeColour(ref Bitmap bmp, byte inColourR, byte inColourG, byte inColourB, byte outColourR, byte outColourG, byte outColourB, int k)
        {
            // Specify a pixel format.
            PixelFormat pxf = PixelFormat.Format24bppRgb;

            // Lock the bitmap's bits.
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);

            var sourceBp = (byte*)bmpData.Scan0.ToPointer();

            for (int i = 0; i < bmpData.Height; i++)
            {
                for (int j = 0; j < bmpData.Width; j++)
                {
                    if (sourceBp[i * bmpData.Stride + j * 3] != inColourB && sourceBp[i * bmpData.Stride + j * 3 + 1] != inColourG && sourceBp[i * bmpData.Stride + j * 3 + 2] != inColourR)
                    {
                        sourceBp[i * bmpData.Stride + j * 3] = outColourB;  //B
                        sourceBp[i * bmpData.Stride + j * 3 + 1] = outColourG; //G
                        sourceBp[i * bmpData.Stride + j * 3 + 2] = outColourR;  //R
                        //Color3D[j, i, k] = 1;
                    }
                    else
                    {
                        sourceBp[i * bmpData.Stride + j * 3] = 0;
                        sourceBp[i * bmpData.Stride + j * 3 + 1] = 0;
                        sourceBp[i * bmpData.Stride + j * 3 + 2] = 0;
                        //Color3D[j, i, k] = 0;
                    }
                }
            }
            // Unlock the bits.
            bmp.UnlockBits(bmpData);
        }


        public void SetData(OrientationEnum OrientationEnum, vtkImageData rowCTimageData)
        {
            InitialData(OrientationEnum, rowCTimageData);
        }

        public AirwayPatient _airwayPatient;
        public void SetData(OrientationEnum OrientationEnum, vtkImageData rowCTimageData , string airwayPath,AirwayPatient airwayPatient)
        {
            _airwayPatient = airwayPatient;
            InitialData(OrientationEnum, rowCTimageData);
            LoadAirwayData(airwayPath , true);
        }
        private unsafe void UpdateCTAxialImage()
        {

            int startIdx = _dimensionsCT[0] * _dimensionsCT[1] * (int)tbFrame.Value;

            //if (_unsafeBmpCT == null)
            {
                _unsafeBmpCT = new UnsafeBitmap(_dimensionsCT[0], _dimensionsCT[1]);
            }
            int width = _unsafeBmpCT.Bitmap.Width;
            int height = _unsafeBmpCT.Bitmap.Height;
           
            _unsafeBmpCT.LockBitmap();
            if (_airwayData == null)
            {
                //_bitmapAirway = new Bitmap(width , height);
                for (int row = 0; row < height; ++row)
                {
                    for (int col = 0; col < width; ++col)
                    {
                        //short rawData = _data[width * height * row + col + width * tbFrame.Value ];
                        int index = startIdx + row * width + col;
                        if (index >= _dicomAlldata.Length)
                        {
                            continue;
                        }
                        short rawData = _dicomAlldata[index];
                        byte rgb = Short2UChar(windowLevel, windowWidth, rawData);
                        var color = new PixelData();
                        color.red = color.green = color.blue = rgb;

                        _unsafeBmpCT.SetPixel(col, height - 1 - row, color);
                    }
                }
            }
            else if (_airwayData != null)
            {
                _bitmapAirway = new Bitmap(width , height);
                BitmapData bitmapData = _bitmapAirway.LockBits(new Rectangle(0, 0, width - 1, height - 1), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                var pointer = (byte*)bitmapData.Scan0.ToPointer();

                for (int row = 0; row < height; ++row)
                {
                    for (int col = 0; col < width; ++col)
                    {
                        //short rawData = _data[width * height * row + col + width * tbFrame.Value ];
                        int index = startIdx + row * width + col;
                        short rawData = _dicomAlldata[index];
                        byte rgb = Short2UChar(windowLevel, windowWidth, rawData);
                        var color = new PixelData();
                        color.red = color.green = color.blue = rgb;

                        _unsafeBmpCT.SetPixel(col, height - 1 - row, color);

                        if (_airwayData != null)
                        {
                            if (index >= 0 && index < _airwayData.Length && _airwayData[index] !=0)
                            {
                                color.red = (byte)Math.Min(color.red + 100, byte.MaxValue);

                                pointer[(height - 1 - row) * bitmapData.Stride +col * 3] = 0;
                                pointer[(height - 1 - row) * bitmapData.Stride + col * 3 + 1] = 0;
                                pointer[(height - 1 - row) * bitmapData.Stride + col * 3 + 2] = color.red;
                            }

                        }
                    }
                }
                _bitmapAirway.UnlockBits(bitmapData);
                _bitmapAirway.MakeTransparent(Color.Black);
            }
            _unsafeBmpCT.UnlockBitmap();
            _bitmapCT = new Bitmap(_unsafeBmpCT.Bitmap, (int)width, (int)height);

            // pictureBox.Image = _bmp.Bitmap;
            //pictureBox.Refresh();
        }


        private unsafe void UpdateCTSagitalImage()
        {
            int startIdx = (int)tbFrame.Value;
            _unsafeBmpCT = new UnsafeBitmap(_dimensionsCT[1], _dimensionsCT[2]);
            int width = _unsafeBmpCT.Bitmap.Width;
            int height = _unsafeBmpCT.Bitmap.Height;
            int s = _dimensionsCT[0] * _dimensionsCT[1];
            _unsafeBmpCT.LockBitmap();

            if (_airwayData == null)
            {
                for (int row = 0; row < height; ++row)
                {
                    for (int col = 0; col < width; ++col)
                    {
                        //short rawData = _data[width * height * row + col + width * tbFrame.Value ];

                        short rawData = _dicomAlldata[startIdx + row * s + col * _dimensionsCT[0]];
                        byte rgb = Short2UChar(windowLevel, windowWidth, rawData);
                        var color = new PixelData();
                        color.red = color.green = color.blue = rgb;

                        _unsafeBmpCT.SetPixel(width - 1 - col, row, color);
                    }
                }
            }
            else if (_airwayData != null)
            {
                _bitmapAirway = new Bitmap(width, height);
                BitmapData bitmapData = _bitmapAirway.LockBits(new Rectangle(0, 0, width - 1, height - 1), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                var pointer = (byte*)bitmapData.Scan0.ToPointer();

                for (int row = 0; row < height; ++row)
                {
                    for (int col = 0; col < width; ++col)
                    {
                        //short rawData = _data[width * height * row + col + width * tbFrame.Value ];
                        int index = startIdx + row * s + col * _dimensionsCT[0];
                        if (index >= _dicomAlldata.Length)
                        {
                            continue;
                        }
                        //Console.WriteLine("update sagital _airwayData index : " + index);
                        short rawData = _dicomAlldata[index];
                        byte rgb = Short2UChar(windowLevel, windowWidth, rawData);
                        var color = new PixelData();
                        color.red = color.green = color.blue = rgb;

                        _unsafeBmpCT.SetPixel(width - 1 - col, row, color);
                        if (_airwayData != null)
                        {
                            if (index >= 0 && index < _airwayData.Length && _airwayData[index] !=0)
                            {
                                color.red = (byte)Math.Min(color.red + 100, byte.MaxValue);

                                pointer[ row * bitmapData.Stride + (width - 1 - col) * 3] = 0;
                                pointer[ row * bitmapData.Stride + (width - 1 - col) * 3 + 1] = 0;
                                pointer[row * bitmapData.Stride + (width - 1 - col) * 3 + 2] = color.red;
                            }

                        }
                    }
                }
                _bitmapAirway.UnlockBits(bitmapData);
                _bitmapAirway.MakeTransparent(Color.Black);
                //_bitmapAirway = new Bitmap(_bitmapAirway, (int)width, (int)(height * _dataSpacingCT[2] / _dataSpacingCT[0]));
            }
            _unsafeBmpCT.UnlockBitmap();

            //_bitmapCT = new Bitmap(_unsafeBmpCT.Bitmap, (int)width, (int)(height * _dataSpacingCT[2] / _dataSpacingCT[0]));
            _bitmapCT = _unsafeBmpCT.Bitmap;
            // pictureBox.Image = _bmp.Bitmap;
            //pictureBox.Refresh();

        }
        private unsafe void UpdateCTCoronalImage()
        {

            int startIdx = _dimensionsCT[0] * (_dimensionsCT[1] - (int)tbFrame.Value - 1);
            _unsafeBmpCT = new UnsafeBitmap(_dimensionsCT[1], _dimensionsCT[2]);
            int width = _unsafeBmpCT.Bitmap.Width;
            int height = _unsafeBmpCT.Bitmap.Height;
            int s = _dimensionsCT[0] * _dimensionsCT[1];
            _unsafeBmpCT.LockBitmap();

            if (_airwayData == null)
            {
                for (int row = 0; row < height; ++row)
                {
                    for (int col = 0; col < width; ++col)
                    {
                        //short rawData = _data[width * height * row + col + width * tbFrame.Value ];
                        short rawData = _dicomAlldata[startIdx + row * s + col];
                        byte rgb = Short2UChar(windowLevel, windowWidth, rawData);
                        var color = new PixelData();
                        color.red = color.green = color.blue = rgb;

                        _unsafeBmpCT.SetPixel(col, row, color);
                    }
                }

            }
            else if (_airwayData != null)
            {
                _bitmapAirway = new Bitmap(width, height);
                BitmapData bitmapData = _bitmapAirway.LockBits(new Rectangle(0, 0, width - 1, height - 1), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                var pointer = (byte*)bitmapData.Scan0.ToPointer();
                for (int row = 0; row < height; ++row)
                {
                    for (int col = 0; col < width; ++col)
                    {
                        //short rawData = _data[width * height * row + col + width * tbFrame.Value ];ndex
                        int index = startIdx + row * s + col;
                        if (index >= _dicomAlldata.Length)
                        {
                            continue;
                        }
                        short rawData = _dicomAlldata[index];
                        byte rgb = Short2UChar(windowLevel, windowWidth, rawData);
                        var color = new PixelData();
                        color.red = color.green = color.blue = rgb;

                        _unsafeBmpCT.SetPixel(col, row, color);

                        if (_airwayData != null)
                        {
                            if (index >= 0 && index < _airwayData.Length && _airwayData[index] != 0)
                            {
                                color.red = (byte)Math.Min(color.red + 100, byte.MaxValue);

                                pointer[row * bitmapData.Stride + col * 3] = 0;
                                pointer[row * bitmapData.Stride + col* 3 + 1] = 0;
                                pointer[row * bitmapData.Stride + col * 3 + 2] = color.red;
                            }

                        }
                    }
                }
                _bitmapAirway.UnlockBits(bitmapData);
                _bitmapAirway.MakeTransparent(Color.Black);
            }
            _unsafeBmpCT.UnlockBitmap();
            _bitmapCT = new Bitmap(_unsafeBmpCT.Bitmap, width, height);
            // pictureBox.Image = _bmp.Bitmap;
            //pictureBox.Refresh();
        }





        public static byte Short2UChar(int windowLevel, int windowWidth, object inputvalue)
        {
            int lower = windowLevel - windowWidth / 2;
            int upper = windowLevel + windowWidth / 2;

            double value = System.Convert.ToDouble(inputvalue);
            //Console.WriteLine(value);

            if (value < lower)
            {
                return 0;
            }
            else if (value > upper)
            {
                return 255;
            }
            else
            {
                var pixel = (byte)((value - lower) * 255 / windowWidth);

                return pixel;
            }
        }
        public event EventHandler<PositionChangedEventArgs> PositionChangedEvent;
        private bool isJumpToEnable = true;
        /// <summary>
        /// This method will be used when external component asks CTViewControl to update position.
        /// 
        /// Scenorary:
        /// 1. To be called in navigation mode. Main engine get a new sensor position, will ask ct view component to show the new sensor position.
        /// 2. There will be three CT view controls in main form. Once user picked a new position in one control, the other twos should be updated to new position accordingly.
        /// </summary>
        /// <param name="position">New position</param>
        public void JumpTo(double[] position)
        {
            if (isJumpToEnable == false)
            {
                return;
            }
            int frame = 0;
            _point3D = position;
            switch (_OrientationEnum)
            {
                case OrientationEnum.Sagittal:

                    _point2D = PositionOperation.Position_3D_2_SagitalCT(_point3D, _dataSpacingCT, _unsafeBmpCT.Bitmap.Width, _unsafeBmpCT.Bitmap.Height);

                    frame = (int)_point2D[2];
                    if (frame >= tbFrame.Minimum && frame <= tbFrame.Maximum)
                    {
                        tbFrame.Value = frame;
                    }
                    UpdateCTSagitalImage();
                    break;
                case OrientationEnum.Coronal:
                    _point2D = PositionOperation.Position_3D_2_ConoralCT(_point3D, _dataSpacingCT, _unsafeBmpCT.Bitmap.Width, _unsafeBmpCT.Bitmap.Height);
                    frame = (int)_point2D[2];
                    if (frame >= tbFrame.Minimum && frame <= tbFrame.Maximum)
                    {
                        tbFrame.Value = frame;
                    }
                    UpdateCTCoronalImage();
                    break;
                case OrientationEnum.Axial:
                    _point2D = PositionOperation.Position_3D_2_Axial_CT(_point3D, _dataSpacingCT, _unsafeBmpCT.Bitmap.Width, _unsafeBmpCT.Bitmap.Height);
                    frame = (int)_point2D[2];
                    if (frame >= tbFrame.Minimum && frame <= tbFrame.Maximum)
                    {
                        tbFrame.Value = frame;
                    }
                    UpdateCTAxialImage();
                    break;
                case OrientationEnum.Unknown:
                    break;
                default:
                    break;
            }
            Console.WriteLine(_point2D[0] + "  " + _point2D[1] + "  " + _point2D[2] + "  ");
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
                catch (Exception)
                {
                }
                
                if (_bitmapAirway != null)
                {
                    e.Graphics.DrawImage(_bitmapAirway, 0, 0);
                }
            }
            pen = new Pen(Brushes.Yellow, 0.5f);
            if (isAddPoint&& !isHasOtherOperation())
            {
                e.Graphics.FillEllipse(addPointCursorBrush, new Rectangle((int)startPointOnImageCoord.X - radius / 2, (int)startPointOnImageCoord.Y - radius / 2, radius, radius));
                e.Graphics.DrawEllipse(pen, (int)startPointOnImageCoord.X - radius / 2, (int)startPointOnImageCoord.Y - radius / 2, radius, radius);
            }

            if (isDeletePoint && !isHasOtherOperation())
            {
                e.Graphics.FillEllipse(deletePointCursorBrush, new Rectangle((int)startPointOnImageCoord.X - radius / 2, (int)startPointOnImageCoord.Y - radius / 2, radius, radius));
                e.Graphics.DrawEllipse(pen, (int)startPointOnImageCoord.X - radius / 2, (int)startPointOnImageCoord.Y - radius / 2, radius, radius);
            }
           // Console.WriteLine("paiting : "+time.ElapsedMilliseconds);
            
            pen = new Pen(Brushes.Red, 1 / matrix.Elements[3]);
            if (_point2D != null)
            {
                pen.DashStyle = DashStyle.Dash;
                pen.DashPattern = new float[] { 2, 5 };
                e.Graphics.DrawLine(pen, new Point(0, (int)_point2D[1]), new Point(_bitmapCT.Width, (int)_point2D[1]));
                e.Graphics.DrawLine(pen, new Point((int)_point2D[0], 0), new Point((int)_point2D[0], _bitmapCT.Height));
            }

            //if (isDrawRectangle)
            {
                pen = new Pen(Brushes.Yellow, 2 / matrix.Elements[3]);
                pen.DashStyle = DashStyle.Solid;
                if (currRectagle.Width != 0 && currRectagle.Height != 0)
                {
                    e.Graphics.DrawRectangle(pen, currRectagle.X, currRectagle.Y, currRectagle.Size.Width, currRectagle.Size.Height);
                }
                foreach (var item in listRectangle)
                {
                    if (item.Frame == (int)tbFrame.Value)
                    {
                        e.Graphics.DrawRectangle(pen, item.Rectagle.X, item.Rectagle.Y, item.Rectagle.Size.Width, item.Rectagle.Size.Height);
                    }
                }

            }

            if (true)
            {
                pen.DashStyle = DashStyle.Solid;
                switch (_OrientationEnum)
                {
                    case OrientationEnum.Sagittal:
                        foreach (var item in listLinesSagital)
                        {
                            if (item.Index == (int)tbFrame.Value)
                            {
                                pen = new Pen(Brushes.Red, 1 / matrix.Elements[3]);
                                e.Graphics.DrawLine(pen, item.startPoint, item.endPoint);
                                pen = new Pen(Brushes.Yellow, 1 / matrix.Elements[3]);
                                e.Graphics.FillEllipse(Brushes.Yellow, (int)item.startPoint.X - 1, (int)item.startPoint.Y - 1, 2, 2);
                                e.Graphics.FillEllipse(Brushes.Yellow, (int)item.endPoint.X - 1, (int)item.endPoint.Y - 1, 2, 2);
                                e.Graphics.DrawString(item.length + " mm", new Font("Arial", 10 / matrix.Elements[3], FontStyle.Regular), Brushes.Red, item.endPoint);
                            }
                        }
                        break;
                    case OrientationEnum.Coronal:
                        foreach (var item in listLinesCoronal)
                        {
                            if (item.Index == (int)tbFrame.Value)
                            {
                                pen = new Pen(Brushes.Red, 1 / matrix.Elements[3]);
                                e.Graphics.DrawLine(pen, item.startPoint, item.endPoint);
                                pen = new Pen(Brushes.Yellow, 1 / matrix.Elements[3]);
                                e.Graphics.FillEllipse(Brushes.Yellow, (int)item.startPoint.X - 1, (int)item.startPoint.Y - 1, 2, 2);
                                e.Graphics.FillEllipse(Brushes.Yellow, (int)item.endPoint.X - 1, (int)item.endPoint.Y - 1, 2, 2);
                                e.Graphics.DrawString(item.length + " mm", new Font("Arial", 10 / matrix.Elements[3], FontStyle.Regular), Brushes.Red, item.endPoint);
                            }
                        }
                        break;
                    case OrientationEnum.Axial:
                        foreach (var item in listLinesAxial)
                        {
                            if (item.Index == (int)tbFrame.Value)
                            {
                                pen = new Pen(Brushes.Red, 1 / matrix.Elements[3]);
                                e.Graphics.DrawLine(pen, item.startPoint, item.endPoint);
                                pen = new Pen(Brushes.Yellow, 1 / matrix.Elements[3]);
                                e.Graphics.FillEllipse(Brushes.Yellow, (int)item.startPoint.X - 1, (int)item.startPoint.Y - 1, 2, 2);
                                e.Graphics.FillEllipse(Brushes.Yellow, (int)item.endPoint.X - 1, (int)item.endPoint.Y - 1, 2, 2);
                                e.Graphics.DrawString(item.length + " mm", new Font("Arial", 10 / matrix.Elements[3], FontStyle.Regular), Brushes.Red, item.endPoint);
                            }
                        }
                        break;
                    case OrientationEnum.Unknown:
                        break;
                    default:
                        break;
                }
                
                if (currentLine != null)
                {
                    if (currentLine.startPoint.X != 0 && currentLine.startPoint.Y != 0)
                    {
                        pen = new Pen(Brushes.Red, 1 / matrix.Elements[3]);
                        e.Graphics.DrawLine(pen, currentLine.startPoint, currentLine.endPoint);
                        pen = new Pen(Brushes.Yellow, 1 / matrix.Elements[3]);
                        e.Graphics.FillEllipse(Brushes.Yellow, (int)currentLine.startPoint.X-1, (int)currentLine.startPoint.Y-1, 2, 2);
                        e.Graphics.FillEllipse(Brushes.Yellow, (int)currentLine.endPoint.X-1, (int)currentLine.endPoint.Y-1, 2, 2);
                        e.Graphics.DrawString(currentLine.length +" mm",new Font("Arial" , 10/matrix.Elements[3] , FontStyle.Regular) , Brushes.Red , currentLine.endPoint );
                    }
                }
                
            }
            
            if (controlPointList.Count > 0 && isDrawCloedCurve || isDrawPolyline)
            {
                List<Point> tempControlPointList = new List<Point>(controlPointList);
                if (tempControlPointList.Count(item => item.X == cursorPoint.X && item.Y == cursorPoint.Y) == 0)
                {
                    tempControlPointList.Add(cursorPoint);
                }

                pen = new Pen(Brushes.Red, 1 / matrix.Elements[3]);
                if (tempControlPointList.Count == 2)
                {
                    e.Graphics.DrawLine(pen, tempControlPointList[0], tempControlPointList[1]);
                }

                pen = new Pen(Brushes.Yellow, 1 / matrix.Elements[3]);
                foreach (Point point in tempControlPointList)
                {
                    e.Graphics.DrawEllipse(pen, point.X - 2 / matrix.Elements[3], point.Y - 2 / matrix.Elements[3], 4 / matrix.Elements[3], 4 / matrix.Elements[3]);
                }

                if (cursorPoint != Point.Empty)
                {
                    pen = new Pen(Brushes.Black, 1 / matrix.Elements[3]);
                    e.Graphics.DrawEllipse(pen, cursorPoint.X - 2 / matrix.Elements[3], cursorPoint.Y - 2 / matrix.Elements[3], 4 / matrix.Elements[3], 4 / matrix.Elements[3]);
                }

                pen = new Pen(Brushes.Red, 1 / matrix.Elements[3]);
                if (tempControlPointList.Count > 2)
                {
                    e.Graphics.DrawCurve(pen, tempControlPointList.ToArray());
                }
            }

            if (cursorPoint != Point.Empty)
            {
                pen = new Pen(Brushes.Black, 1 / matrix.Elements[3]);
                e.Graphics.DrawEllipse(pen, cursorPoint.X - 2 / matrix.Elements[3], cursorPoint.Y - 2 / matrix.Elements[3], 4 / matrix.Elements[3], 4 / matrix.Elements[3]);
            }

            if (isSelectConnectPoints)
            {
                if (_pointA != null )
                {
                    if (Math.Abs(_pointA[2] - tbFrame.Value) < 1)
                    {
                        e.Graphics.FillEllipse(Brushes.Red, new Rectangle(_pointA[0] - (int)(5 / matrix.Elements[3]), _pointA[1] - (int)(5 / matrix.Elements[3]), (int)(5 * 2 / matrix.Elements[3]), (int)(10 / matrix.Elements[3])));
                    }
                }
                if (_pointB != null)
                {
                    if (Math.Abs(_pointB[2] - tbFrame.Value) < 1)
                    {
                        e.Graphics.FillEllipse(Brushes.Red, new Rectangle(_pointB[0] - (int)(5 / matrix.Elements[3]), _pointB[1] - (int)(5 / matrix.Elements[3]), (int)(5 * 2 / matrix.Elements[3]), (int)(10 / matrix.Elements[3])));
                    }
                }
            }

            e.Graphics.Transform = textMatrix;
            e.Graphics.DrawString(upOrientationEnumText, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(pictureBox.Width / 2, 2));
            e.Graphics.DrawString(downOrientationEnumText, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(pictureBox.Width / 2, pictureBox.Height - 40));
            e.Graphics.DrawString(leftOrientationEnumText, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, pictureBox.Height / 2));
            e.Graphics.DrawString(((int)tbFrame.Value).ToString(), new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, pictureBox.Height -40));
            if (mouseOverIndex < _dicomAlldata.Length)
            {
                e.Graphics.DrawString("Hu : " + (_dicomAlldata[mouseOverIndex]).ToString(), new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(pictureBox.Width - 100, pictureBox.Height - 20));
            }
            e.Graphics.DrawString(rightOrientationEnumText, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(pictureBox.Width - 30, pictureBox.Height / 2));

            if (_airwayPatient != null)
            {
                e.Graphics.DrawString(_airwayPatient.Name, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, 2));
                e.Graphics.DrawString(_airwayPatient.Sex, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, 22));
                e.Graphics.DrawString(_airwayPatient.Institution, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, 42));
                e.Graphics.DrawString(_airwayPatient.Age, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(2, 62));
            }
            e.Graphics.DrawString("L   :"+ windowLevel, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(pictureBox.Width-100, pictureBox.Height-70));
            e.Graphics.DrawString("W :" + windowWidth, new Font("微软雅黑", 14, FontStyle.Regular), Brushes.LightGreen, new PointF(pictureBox.Width - 100, pictureBox.Height-50));

            if (isComputing)
            {
                e.Graphics.DrawString("正在处理数据，请稍等......", new Font("微软雅黑", 30, FontStyle.Regular), Brushes.LightGreen, new PointF(10, 100));
            }
        }

        public void StartHandleData()
        {
            isComputing = true;
            pictureBox.Refresh();
        }


        public void StopHandleData()
        {
            isComputing = false;
            pictureBox.Refresh();
        }
        public Bitmap ScreenShot()
        {
            Bitmap bm = new Bitmap(pictureBox.Width, pictureBox.Height);
            Rectangle r = new Rectangle(0, 0, pictureBox.Width, pictureBox.Height);
            pictureBox.DrawToBitmap(bm, r);
            bm.Save("文件名");

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
        public void SetOritation(OrientationEnum oritation)
        {
            _OrientationEnum = oritation;
            switch (_OrientationEnum)
            {
                case OrientationEnum.Sagittal:
                    tbFrame.Maximum = _dimensionsCT[0] - 1;
                    break;
                case OrientationEnum.Coronal:
                    tbFrame.Maximum = _dimensionsCT[1] - 1;
                    break;
                case OrientationEnum.Axial:
                    tbFrame.Maximum = _dimensionsCT[2] - 1;
                    break;
                case OrientationEnum.Unknown:
                    break;
                default:
                    break;
            }
            UpdateImage();
            JumpTo(_point3D);
        }
        private void UpdateImage()
        {
            Console.WriteLine("UpdateImage");
            switch (_OrientationEnum)
            {
                case OrientationEnum.Sagittal:
                    UpdateCTSagitalImage();
                    break;
                case OrientationEnum.Coronal:
                    UpdateCTCoronalImage();
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
            if (_point2D != null)
            {
                _point2D = new double[] { _point2D[0], _point2D[1], tbFrame.Value };
                double[] position = PositionOperation.CT2D_2_3D(_point2D, _OrientationEnum, _dataSpacingCT, _bitmapCT.Width, _bitmapCT.Height);
                if (Picked != null)
                {
                    Picked(this, new PickEventArg()
                    {
                        Position = position
                    });
                }
            }
        }

        private void tbFrame_Scroll(object sender, EventArgs e)
        {
            if (listAirwayBitmap.Count != 0 && listAirwayBitmap != null)
            {
                _bitmapAirway = listAirwayBitmap[(int)tbFrame.Value];
            }
            UpdateImage();
           
        }
        void tbFrame_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (listAirwayBitmap != null && listAirwayBitmap.Count != 0)
            {
                _bitmapAirway = listAirwayBitmap[(int)tbFrame.Value];
            }
            UpdateImage();
            //throw new NotImplementedException();
        }

        private PointF startPointOnImageCoord, startPointOnControlCoord;
        private PointF startPointOnImageCoord1, startPointOnControlCoord1;
        private PointF endPointOnImageCoord,endPointOnControlCoord;
        private LineEntity currentLine;
        private List<LineEntity> listLinesAxial = new List<LineEntity>();
        private List<LineEntity> listLinesCoronal = new List<LineEntity>();
        private List<LineEntity> listLinesSagital = new List<LineEntity>();
        void pictureBox_MouseWheel(object sender, MouseEventArgs e)
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
            //throw new NotImplementedException();
        }

        private bool isDrawCloedCurve = false;
        private bool isDrawRectangle = false;
        private Rectangle currRectagle;

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

            _point2D = new double[] { startPointOnImageCoord.X , startPointOnImageCoord.Y , tbFrame.Value};
            _point3D = PositionOperation.CT2D_2_3D(_point2D, _OrientationEnum, _dataSpacingCT, _bitmapCT.Width, _bitmapCT.Height);
            if(Picked!=null){
                Picked(this , new PickEventArg(){
                    Position = _point3D
                });
            }
            UpdateCamera(_point3D, _point3D);
            
            if (isDrawCloedCurve)
            {
                isComputing = true; Refresh();
                this.Cursor = Cursors.WaitCursor;
                controlPointList.RemoveAt(controlPointList.Count - 1);
                controlPointList.Add(controlPointList[0]);
                int minX = 10000, maxX = 0, minY = 10000, maxY = 0;

                for (int i = 0; i < controlPointList.Count; i++)
                {
                    if (controlPointList[i].X < minX)
                    {
                        minX = controlPointList[i].X;
                    }
                    if (controlPointList[i].X > maxX)
                    {
                        maxX = controlPointList[i].X;
                    }

                    if (controlPointList[i].Y < minY)
                    {
                        minY = controlPointList[i].Y;
                    }
                    if (controlPointList[i].Y > maxY)
                    {
                        maxY = controlPointList[i].Y;
                    }

                }

                for (int height = minY; height < maxY; height++)
                {
                    for (int width = minX; width < maxX; width++)
                    {
                        Point point = new Point(width , height);
                        if (isInCurveRegion(controlPointList.ToArray(), point))
                        {
                           // if (rbAddCurveColor.Checked)
                            if(false)
                            {
                                if (_OrientationEnum == OrientationEnum.Axial)
                                {
                                    int index = ((int)(tbFrame.Value)) * _dimensionsCT[0] * _dimensionsCT[1] + _dimensionsCT[0] * (int)(_bitmapAirway.Height - 1 - point.Y) + (int)point.X;
                                    _airwayData[index] = 255;
                                    //_airwayImageData.SetScalarComponentFromDouble((int)point.X, (int)(_bitmapAirway.Height - 1 - point.Y), (int)tbFrame.Value, 0, 255);
                                }
                                else if (_OrientationEnum == OrientationEnum.Sagittal)
                                {
                                    int startIdx = (int)tbFrame.Value;
                                    int s = _dimensionsCT[0] * _dimensionsCT[1];

                                    int index = (int)(startIdx + point.Y * s + (_bitmapAirway.Width - 1 - point.X) * _dimensionsCT[0]);
                                    _airwayData[index] = 255;
                                   // _airwayImageData.SetScalarComponentFromDouble((int)tbFrame.Value, (int)(_bitmapAirway.Width - 1 - point.X), (int)(point.Y), 0, 255);
                                }
                                else if (_OrientationEnum == OrientationEnum.Coronal)
                                {
                                    int startIdx = _dimensionsCT[0] * (_dimensionsCT[1] - (int)tbFrame.Value - 1);
                                    int s = _dimensionsCT[0] * _dimensionsCT[1];
                                    int index = startIdx + point.Y * s + point.X;
                                    _airwayData[index] = 255;
                                }
                            }
                            else
                            {
                                if (_OrientationEnum == OrientationEnum.Axial)
                                {
                                    int index = ((int)(tbFrame.Value)) * _dimensionsCT[0] * _dimensionsCT[1] + _dimensionsCT[0] * (int)(_bitmapAirway.Height - 1 - point.Y) + (int)point.X;
                                    _airwayData[index] = 0;
                                   // _airwayImageData.SetScalarComponentFromDouble((int)point.X, (int)(_bitmapAirway.Height - 1 - point.Y), (int)tbFrame.Value, 0, 0);
                                }
                                else if (_OrientationEnum == OrientationEnum.Sagittal)
                                {
                                    int startIdx = (int)tbFrame.Value;
                                    int s = _dimensionsCT[0] * _dimensionsCT[1];

                                    int index = (int)(startIdx + point.Y * s + (_bitmapAirway.Width - 1 - point.X) * _dimensionsCT[0]);
                                    _airwayData[index] = 0;
                                    //_airwayImageData.SetScalarComponentFromDouble((int)tbFrame.Value, (int)(_bitmapAirway.Width - 1 - point.X), (int)(point.Y), 0, 0);
                                }
                                else if (_OrientationEnum == OrientationEnum.Coronal)
                                {
                                    int startIdx = _dimensionsCT[0] * (_dimensionsCT[1] - (int)tbFrame.Value - 1);
                                    int s = _dimensionsCT[0] * _dimensionsCT[1];
                                    int index = startIdx + point.Y * s + point.X;
                                    _airwayData[index] = 0;
                                }
                            }
                        }
                    }
                }
                isDrawCloedCurve = false;
                isDrawPolyline = false;
                UpdateImage();
                controlPointList = new List<Point>();
                isComputing = false;
                this.Cursor = Cursors.Cross;
            }

            if (isDrawPolyline)
            {
                isComputing = true; Refresh();
                this.Cursor = Cursors.WaitCursor;
                isInPolylineRegion();
                UpdateImage();
                isDrawPolyline = false;
                isDrawCloedCurve = false;
                controlPointList = new List<Point>();
                isComputing = false;
                this.Cursor = Cursors.Cross;
            }

            if (isSelectConnectPoints)
            {
                isComputing = true;
                Refresh();

                this.Cursor = Cursors.WaitCursor;
                if (_pointA != null && _pointB != null)
                {
                    ConnectTowPoints(_pointA , _pointB);
                }
                isComputing = false;
                Refresh();
                this.Cursor = Cursors.Cross;
            }

            pictureBox.Refresh();
            isSelectCharact = true;
            //btnDrawClosedCurve.PerformClick();
        }
        private bool isComputing = false;
        public static bool isInCurveRegion(Point[] points, Point point)
        {

            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.Reset();
            graphicsPath.AddPolygon(points);

            Region region = new Region();
            region.MakeEmpty();
            region.Union(graphicsPath);
            if (region.IsVisible(point))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private List<Point> controlPointList = new List<Point>();
        private Point cursorPoint;
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            (sender as PictureBox).Focus();

            startPointOnControlCoord = new System.Drawing.Point(e.X, e.Y);
            startPointOnImageCoord1 = startPointOnImageCoord = Control2Image(matrix, new System.Drawing.Point(e.X, e.Y));
            if (isMeasure)
            {
                currentLine = new LineEntity(_dataSpacingCT , _OrientationEnum);
                currentLine.startPoint = new Point((int)startPointOnImageCoord.X,(int) startPointOnImageCoord.Y);
                currentLine.Index = (int)tbFrame.Value;
            }
            if ((System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                return;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (controlPointList.Count > 0)
                {
                    controlPointList.RemoveAt(controlPointList.Count - 1);
                    Console.WriteLine("remove " + DateTime.Now + "count : "+controlPointList.Count);
                    return;
                }

                if (isSelectConnectPoints)
                {
                    if (_pointB != null && _pointA!= null)
                    {
                        _pointB = null;
                    }
                    else if (_pointB == null && _pointA != null)
                    {
                        _pointA = null;
                    }
                }
            }

            if (isSelectConnectPoints && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (_pointA == null)
                {
                    _pointA = new int[]
                    {
                        (int)startPointOnImageCoord.X , (int)startPointOnImageCoord.Y , (int)(tbFrame.Value)
                    };
                }
                else 
                {
                    _pointB = new int[]
                    {
                        (int)startPointOnImageCoord.X , (int)startPointOnImageCoord.Y, (int)(tbFrame.Value)
                    };
                }
            }
            if (isDrawCloedCurve || isDrawPolyline)
            {
                if (e.Button == MouseButtons.Left)
                {
                    controlPointList.Add(Control2Image(matrix , e.Location));
                }
                else
                {
                    if (controlPointList.Count >= 1)
                    {
                        controlPointList.RemoveAt(controlPointList.Count - 1);
                    }
                }

                Refresh();

                //listCurvePoints.Add(new Point((int)startPointOnImageCoord.X , (int)startPointOnImageCoord.Y));
                //pictureBox.Refresh();
            }

            _previousPoints = new List<int>();

            currRectagle = new Rectangle((int)startPointOnImageCoord1.X, (int)startPointOnImageCoord1.Y, 0, 0);
        }
        private int AxialToArrayIndex(int x, int y, int z, double[] dimension , int width , int height)
        {
            return (int)(z * dimension[0] * dimension[1] + dimension[0] * (height - 1 - y) + x);
        }

        private int SagitalToArrayIndex(int x, int y, int z, double[] dimension, int width, int height)
        {
            return (int)(z + y * dimension[0] * dimension[1] + (width - 1 - x) * dimension[0]);
        }

        private int CoronalToArrayIndex(int x, int y, int z, double[] dimension, int width, int height)
        {
            return (int)(dimension[0] * (dimension[1] - z - 1) + y * dimension[0] * dimension[1] + x);
        }

        private int GetArrayIndex(int x, int y, int z, int[] dimension, int width, int height)
        {
            switch (_OrientationEnum)
            {
                case OrientationEnum.Sagittal:
                    return (int)(z + y * dimension[0] * dimension[1] + (width - 1 - x) * dimension[0]);
                case OrientationEnum.Coronal:
                    return (int)(dimension[0] * (dimension[1] - z - 1) + y * dimension[0] * dimension[1] + x);
                case OrientationEnum.Axial:
                    return (int)(z * dimension[0] * dimension[1] + dimension[0] * (height - 1 - y) + x);
                case OrientationEnum.Unknown:
                    return -1;
                default:
                    break;
            }
            return -1;
        }


        private double[] ArrayIndexToPoint2D(int index, int[] dimension, int width, int height)
        {
            switch (_OrientationEnum)
            {
                case OrientationEnum.Sagittal:
                    //return (int)(z + y * dimension[0] * dimension[1] + (width - 1 - x) * dimension[0]);
                    return null;
                case OrientationEnum.Coronal:
                    //return (int)(dimension[0] * (dimension[1] - z - 1) + y * dimension[0] * dimension[1] + x);
                    return null;
                case OrientationEnum.Axial:
                    //return (int)(z * dimension[0] * dimension[1] + dimension[0] * (height - 1 - y) + x);
                    int z = index / (dimension[0] * dimension[1]);
                    int y = height - 1 - (index - z * dimension[0] * dimension[1]) / dimension[1];
                    int x = (index - z * dimension[0] * dimension[1] - (height - 1 - y) * dimension[0]);
                    return new double[] { x , y , z};
                case OrientationEnum.Unknown:
                    return null;
                default:
                    break;
            }
            return null;
        }
        private void ConnectTowPoints(int[] pointA, int[] pointB)
        {
            int[] direction = new int[]
                {
                    pointB[0] - pointA[0],
                    pointB[1] - pointA[1],
                    pointB[2] - pointA[2]
                };
            if (direction[0] == 0 && direction[1] == 0 && direction[2] == 0)
            {
                return;
            }

            double length = Math.Sqrt(direction[0] * direction[0] + direction[1] * direction[1] + direction[2] * direction[2]);
            Console.WriteLine("length : " + length);
            int n = 0;
            for (int i = 0; i <= length; ++i)
            {
                int[] newPoint = new int[]
                    {
                        pointA[0] + (int)(direction[0] * i / length),
                        pointA[1] + (int)(direction[1] * i / length),
                        pointA[2] + (int)(direction[2] * i / length),
                    };
                if (newPoint[0] < 0 || newPoint[1] < 0 || newPoint[2] < 0)
                {
                    Console.WriteLine("newPoint is less 0");
                }
               // Console.WriteLine("newPoint :" + newPoint[0] +" "+newPoint[1] +" "+newPoint[2] +" ");
                for (int y = newPoint[1] - radius /2; y <= newPoint[1]+ radius / 2; y++)
                {
                    for (int x = newPoint[0] - radius / 2; x <= newPoint[0] + radius / 2; x++)
                    {
                        int index = newPoint[2] * _dimensionsCT[0] * _dimensionsCT[1] + _dimensionsCT[0] * (int)(_bitmapAirway.Height - 1 - y) + x;
                        //int index = GetArrayIndex(x, y, newPoint[2], _dimensionsCT, _bitmapCT.Width, _bitmapCT.Height);
                        Console.WriteLine(n++ +" index : "+index + " total : "+_airwayData.Length);
                        if (index >= 0 && index < _airwayData.Length)
                        {
                            //if (rbAddCurveColor.Checked)
                            {
                                _airwayData[index] = 255;
                            }
                            //else if(rbDeleteCurveColor.Checked)
                            {
                                _airwayData[index] = 0;
                            }
                            Console.WriteLine("index : " + index + "n : "+ newPoint[2]);
                        }
                    }
                }
               
            }
        }

        private int radius = 0;
        private bool _isChangeWindowLevelAndWidth = false;
        public void startWindowLevelAndWidth()
        {
            SetAllOperationDisable();
            _isChangeWindowLevelAndWidth = true;
            
            this.Cursor = System.Windows.Forms.Cursors.NoMove2D;

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
           
            PointF p = Control2Image(matrix, new Point(e.X, e.Y));

            if (isAddPoint && ((System.Windows.Forms.Control.ModifierKeys & Keys.Control) != Keys.Control) && ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) != Keys.Shift) && !isHasOtherOperation() && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _hasSaved = false;
                if (_OrientationEnum == OrientationEnum.Axial)
                {

                    int s = radius* radius / 4;
                    for (int y = (int)p.Y - radius/2; y <= p.Y+radius/2 ; y++)
                    {
                        for (int x = (int)p.X - radius/2; x <= p.X+radius/2; x++)
                        {
                            double d = GetDistance(x , y , p.X , p.Y);
                            if (d>radius/2)
                            {
                                continue;
                            };
                            
                            //int index = ((int)(tbFrame.Value)) * _dimensionsCT[0] * _dimensionsCT[1] + _dimensionsCT[0] * (int)(_bitmapAirway.Height - 1 - y) + x;
                            int index = GetArrayIndex(x, y, (int)(tbFrame.Value), _dimensionsCT, _bitmapAirway.Height, _bitmapAirway.Width);
                            if (index > _airwayData.Length - 1)
                            {
                                continue;
                            }
                            if (_isDrawUnder400CT)
                            {
                                if (_dicomAlldata[index] <= -400)
                                {
                                    _airwayData[index] = 255;
                                }
                            }
                            else if (!_isDrawUnder400CT)
                            {
                                if (_airwayData[index] != 255)
                                {
                                    _previousPoints.Add(index);
                                }
                                _airwayData[index] = 255;
                                
                            }
                            // _airwayImageData.SetScalarComponentFromDouble(x, (int)(_bitmapAirway.Height - 1 - y), (int)tbFrame.Value, 0, 255);    
                        }
                    }
                }
                else if (_OrientationEnum == OrientationEnum.Sagittal)
                {
                    int startIdx = (int)tbFrame.Value;
                    int s = _dimensionsCT[0] * _dimensionsCT[1];
                    for (int y = (int)p.Y - radius; y <= p.Y ; y++)
                    {
                        for (int x = (int)p.X - radius; x <= p.X ; x++)
                        {
                            //int index = (int)(startIdx + y * s + (_bitmapAirway.Width - 1 - x) * _dimensionsCT[0]);
                            double d = GetDistance(x, y, p.X, p.Y);
                            if (d > radius / 2)
                            {
                                continue;
                            };

                            //int index = ((int)(tbFrame.Value)) * _dimensionsCT[0] * _dimensionsCT[1] + _dimensionsCT[0] * (int)(_bitmapAirway.Height - 1 - y) + x;
                            int index = GetArrayIndex(x, y, (int)(tbFrame.Value), _dimensionsCT, _bitmapAirway.Width, _bitmapAirway.Height);
                            if (index > _airwayData.Length - 1)
                            {
                                continue;
                            }
                            if (_isDrawUnder400CT)
                            {
                                if (_dicomAlldata[index] <= -400)
                                {
                                    _airwayData[index] = 255;
                                }
                            }
                            else if (!_isDrawUnder400CT)
                            {
                                if (_airwayData[index] != 255)
                                {
                                    _previousPoints.Add(index);
                                }
                                _airwayData[index] = 255;

                            }
                            //_airwayImageData.SetScalarComponentFromDouble((int)tbFrame.Value, (int)(_bitmapAirway.Width - 1 - x), y, 0, 255);
                        }
                    }
                }
                else if (_OrientationEnum == OrientationEnum.Coronal)
                {
                    int startIdx = _dimensionsCT[0] * (_dimensionsCT[1] - (int)tbFrame.Value - 1);
                    int s = _dimensionsCT[0] * _dimensionsCT[1];
                    for (int y = (int)p.Y - radius; y <= p.Y ; y++)
                    {
                        for (int x = (int)p.X - radius; x <= p.X ; x++)
                        {
                            //int index  = startIdx + y * s + x;
                            double d = GetDistance(x, y, p.X, p.Y);
                            if (d > radius / 2)
                            {
                                continue;
                            };

                            //int index = ((int)(tbFrame.Value)) * _dimensionsCT[0] * _dimensionsCT[1] + _dimensionsCT[0] * (int)(_bitmapAirway.Height - 1 - y) + x;
                            int index = GetArrayIndex(x, y, (int)(tbFrame.Value), _dimensionsCT, _bitmapAirway.Height, _bitmapAirway.Width);
                            if (index > _airwayData.Length - 1)
                            {
                                continue;
                            }
                            if (_isDrawUnder400CT)
                            {
                                if (_dicomAlldata[index] <= -400)
                                {
                                    _airwayData[index] = 255;
                                }
                            }
                            else if (!_isDrawUnder400CT)
                            {
                                if (_airwayData[index] != 255)
                                {
                                    _previousPoints.Add(index);
                                }
                                _airwayData[index] = 255;

                            }
                            //_airwayImageData.SetScalarComponentFromDouble((int)tbFrame.Value, (int)(_bitmapAirway.Width - 1 - x), y, 0, 255);
                        }
                    }
                }
                

                UpdateImage();
                //pictureBox.Refresh();
            }

            if (isDeletePoint && ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) != Keys.Shift) && ((System.Windows.Forms.Control.ModifierKeys & Keys.Control) != Keys.Control) && !isHasOtherOperation() && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (_OrientationEnum == OrientationEnum.Axial)
                {
                    for (int y = (int)p.Y - radius/2; y <= p.Y + radius/2; y++)
                    {
                        for (int x = (int)p.X - radius/2; x <= p.X + radius/2; x++)
                        {
                            double d = GetDistance(x, y, p.X, p.Y);
                            if (d > radius / 2)
                            {
                                continue;
                            };
                            //int index = ((int)(tbFrame.Value)) * _dimensionsCT[0] * _dimensionsCT[1] + _dimensionsCT[0] * (int)(_bitmapAirway.Height - 1 - y) + x;
                            int index = GetArrayIndex(x, y, (int)(tbFrame.Value), _dimensionsCT, _bitmapAirway.Height, _bitmapAirway.Width);
                            if (index > _airwayData.Length - 1)
                            {
                                continue;
                            }
                            if (_airwayData[index] != 0)
                            {
                                _previousPoints.Add(index);
                            }
                            _airwayData[index] = 0;
                            
                           // _airwayImageData.SetScalarComponentFromDouble(x, (int)(_bitmapAirway.Height - 1 - y), (int)tbFrame.Value, 0, 0);
                        }
                    }
                }
                else if (_OrientationEnum == OrientationEnum.Sagittal)
                {
                    int startIdx = (int)tbFrame.Value;
                    int s = _dimensionsCT[0] * _dimensionsCT[1];

                    //int index = (int)(startIdx + (p.Y * _dataSpacingCT[0] / _dataSpacingCT[2]) * s + (_bitmapAirway.Width - 1 - p.X) * _dimensionsCT[0]);
                    for (int y = (int)p.Y - radius; y <= p.Y + radius; y++)
                    {
                        for (int x = (int)p.X - radius; x <= p.X + radius; x++)
                        {
                            double d = GetDistance(x, y, p.X, p.Y);
                            if (d > radius / 2)
                            {
                                continue;
                            };
                            //int index = ((int)(tbFrame.Value)) * _dimensionsCT[0] * _dimensionsCT[1] + _dimensionsCT[0] * (int)(_bitmapAirway.Height - 1 - y) + x;
                            int index = GetArrayIndex(x, y, (int)(tbFrame.Value), _dimensionsCT, _bitmapAirway.Width, _bitmapAirway.Height);
                            if (index > _airwayData.Length - 1)
                            {
                                continue;
                            }
                            if (_airwayData[index] != 0)
                            {
                                _previousPoints.Add(index);
                            }
                            _airwayData[index] = 0;
                        }
                    }
                }
                else if (_OrientationEnum == OrientationEnum.Coronal)
                {
                    int startIdx = _dimensionsCT[0] * (_dimensionsCT[1] - (int)tbFrame.Value - 1);
                    int s = _dimensionsCT[0] * _dimensionsCT[1];
                    for (int y = (int)p.Y - radius; y <= p.Y + radius; y++)
                    {
                        for (int x = (int)p.X - radius; x <= p.X + radius; x++)
                        {
                            double d = GetDistance(x, y, p.X, p.Y);
                            if (d > radius / 2)
                            {
                                continue;
                            };
                            //int index = ((int)(tbFrame.Value)) * _dimensionsCT[0] * _dimensionsCT[1] + _dimensionsCT[0] * (int)(_bitmapAirway.Height - 1 - y) + x;
                            int index = GetArrayIndex(x, y, (int)(tbFrame.Value), _dimensionsCT, _bitmapAirway.Height, _bitmapAirway.Width);
                            if (index > _airwayData.Length - 1)
                            {
                                continue;
                            }
                            if (_airwayData[index] != 0)
                            {
                                _previousPoints.Add(index);
                            }
                            _airwayData[index] = 0;
                        }
                    }
                }
                //_bitmapAirway.SetPixel((int)p.X, (int)p.Y, Color.Red);
                UpdateImage();
            }

            if (isDrawCloedCurve || isDrawPolyline || isSelectConnectPoints) 
            {
                cursorPoint = Control2Image(matrix , e.Location);
                Refresh();
            }

            if (isDrawPolyline)
            {
                if (e.Button == MouseButtons.Left)
                {
                    //controlPointList.Add(Control2Image(matrix, e.Location));
                    //isInPolylineRegion();
                    //UpdateImage();
                }
            }

            if (isDrawRectangle && e.Button == MouseButtons.Left)
            {
                int minX = Math.Min((int)startPointOnImageCoord1.X, (int)startPointOnImageCoord.X);
                int minY = Math.Min((int)startPointOnImageCoord1.Y, (int)startPointOnImageCoord.Y);
                int maxX = Math.Max((int)startPointOnImageCoord1.X, (int)startPointOnImageCoord.X);
                int maxY = Math.Max((int)startPointOnImageCoord1.Y, (int)startPointOnImageCoord.Y);
                currRectagle = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            }
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
            }

            if (_isChangeWindowLevelAndWidth == true && e.Button == MouseButtons.Left)
            {
                
                windowLevel += (int)(e.X - startPointOnControlCoord.X);
                windowWidth += (int)(e.Y - startPointOnControlCoord.Y);
                UpdateImage();
            }

            
            startPointOnControlCoord = new PointF(e.X, e.Y);
            startPointOnImageCoord = Control2Image(matrix, startPointOnControlCoord);
            mouseOverIndex = GetArrayIndex((int)startPointOnImageCoord.X, (int)startPointOnImageCoord.Y, (int)(tbFrame.Value), _dimensionsCT, _bitmapAirway.Height, _bitmapAirway.Width);
            if (isMeasure)
            {
                endPointOnImageCoord = startPointOnImageCoord;
                if (currentLine != null)
                {
                    currentLine.endPoint = new Point((int)startPointOnImageCoord.X, (int)startPointOnImageCoord.Y);
                }
            }
            pictureBox.Refresh();
        }

        private int mouseOverIndex = 0;
        private bool isZoom = false;
        private bool isPanning = false;
        private bool isMeasure = false;
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

        public void StartMeasure()
        {
            LesionAlgorithmDAO.ModifyLesionBytes(listRectangle , _dimensionsCT , _dimensionsCT[1]
                , _dicomAlldata, ref _airwayData);
            pictureBox.Refresh();
            return;
                
            SetAllOperationDisable();
            isMeasure = true;
            isAddPoint = false;
            isDeletePoint = false;
            this.Cursor = Cursors.Arrow;
        }

        public void Previous()
        {
            if (isMeasure)
            {
                switch (_OrientationEnum)
                {
                    case OrientationEnum.Sagittal:
                        if (listLinesSagital.Count > 0)
                        {
                            listLinesSagital.RemoveAt(listLinesSagital.Count - 1);
                        }
                        break;
                    case OrientationEnum.Coronal:
                        if (listLinesCoronal.Count > 0)
                        {
                            listLinesCoronal.RemoveAt(listLinesCoronal.Count - 1);
                        }
                        break;
                    case OrientationEnum.Axial:
                        if (listLinesAxial.Count > 0)
                        {
                            listLinesAxial.RemoveAt(listLinesAxial.Count - 1);
                        }
                        break;
                    case OrientationEnum.Unknown:
                        break;
                    default:
                        break;
                }


                pictureBox.Refresh();
            }
            else if (isDrawRectangle)
            {
                if (listRectangle != null && listRectangle.Count > 0)
                {
                    listRectangle.RemoveAt(listRectangle.Count - 1);
                    pictureBox.Refresh();
                }
            }
            else if (_previousPoints != null)
            {
                if (isAddPoint)
                {
                    for (int i = 0; i < _previousPoints.Count; i++)
                    {
                        _airwayData[_previousPoints[i]] = 0;
                    }
                }
                else if(isDeletePoint)
                {
                    for (int i = 0; i < _previousPoints.Count; i++)
                    {
                        _airwayData[_previousPoints[i]] = 255;
                    }
                }
                UpdateImage();
            }
            
            
        }
        private bool isInPolylineRegion()
        {
            if (controlPointList.Count <= 2)
            {
                return false;
            }
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddCurve(controlPointList.ToArray());
            graphicsPath.Widen(new Pen(Color.Blue, 2));

            Region region = new Region();
            region.MakeEmpty();
            region.Union(graphicsPath);

            int minX = 10000, maxX = 0, minY = 10000, maxY = 0;

            for (int i = 0; i < controlPointList.Count; i++)
            {
                if (controlPointList[i].X < minX)
                {
                    minX = controlPointList[i].X;
                }
                if (controlPointList[i].X > maxX)
                {
                    maxX = controlPointList[i].X;
                }

                if (controlPointList[i].Y < minY)
                {
                    minY = controlPointList[i].Y;
                }
                if (controlPointList[i].Y > maxY)
                {
                    maxY = controlPointList[i].Y;
                }
            }
            for (int height = minY; height <= maxY; height++)
            {
                for (int width = minX; width <= maxX; width++)
                {
                    Point point = new Point(width, height);
                    if (region.IsVisible(point))
                    {
                        if (_OrientationEnum == OrientationEnum.Axial)
                        {
                            int index = ((int)(tbFrame.Value)) * _dimensionsCT[0] * _dimensionsCT[1] + _dimensionsCT[0] * (int)(_bitmapAirway.Height - 1 - point.Y) + point.X;
                            _airwayData[index] = 255;
                        }
                        else if (_OrientationEnum == OrientationEnum.Coronal)
                        {
                            int startIdx = _dimensionsCT[0] * (_dimensionsCT[1] - (int)tbFrame.Value - 1);
                            int s = _dimensionsCT[0] * _dimensionsCT[1];
                            int index = startIdx + point.Y * s + point.X;
                            _airwayData[index] = 255;
                        }
                        else if (_OrientationEnum == OrientationEnum.Sagittal)
                        {
                            int startIdx = (int)tbFrame.Value;
                            int s = _dimensionsCT[0] * _dimensionsCT[1];
                            int index = (int)(startIdx + point.Y * s + (_bitmapAirway.Width - 1 - point.X) * _dimensionsCT[0]);
                            _airwayData[index] = 255;
                        }
                    }
                }
            }
            graphicsPath.Dispose();
            region.Dispose();
            return true;
        }

        private List<RectangleEntity> listRectangle = new List<RectangleEntity>();
        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (currRectagle != null && isDrawRectangle && currRectagle.Width>1)
            {
                if (_airwayPatient != null)
                {
                    RectangleEntity rectangleEntity = new RectangleEntity();
                    rectangleEntity.Orientation = _OrientationEnum;
                    rectangleEntity.Rectagle = new Rectangle(currRectagle.X, currRectagle.Y, currRectagle.Width, currRectagle.Height);
                    
                    //rectangleEntity.studyUID = _airwayPatient.StudyInstanceUID;
                    //rectangleEntity.seriesUID = _airwayPatient.SeriesInstanceUID;
                    rectangleEntity.Frame = (int)tbFrame.Value;

                    listRectangle.Add(rectangleEntity);

                    //LesionAlgorithmDAO.TestLesionBytes(listRectangle , _dimensionsCT , _bitmapAirway.Height ,ref _airwayData);
                }
            }
            //listRectangle.Add(new Rectangle(currRectagle.X , currRectagle.Y , currRectagle.Width , currRectagle.Height));
            currRectagle = new Rectangle(new Point(0, 0), new Size(0, 0));
            //this.Cursor = System.Windows.Forms.Cursors.Arrow;
            startPointOnControlCoord = new PointF(e.X, e.Y);
            startPointOnImageCoord = Control2Image(matrix, startPointOnControlCoord);
            if (isMeasure)
            {
                currentLine.endPoint = new Point((int)startPointOnImageCoord.X, (int)startPointOnImageCoord.Y);
                switch (_OrientationEnum)
                {
                    case OrientationEnum.Sagittal:
                        listLinesSagital.Add(currentLine);
                        break;
                    case OrientationEnum.Coronal:
                        listLinesCoronal.Add(currentLine);
                        break;
                    case OrientationEnum.Axial:
                        listLinesAxial.Add(currentLine);
                        break;
                    case OrientationEnum.Unknown:
                        break;
                    default:
                        break;
                }
                //listLinesAxial.Add(currentLine);
                currentLine = new LineEntity(_dataSpacingCT , _OrientationEnum);
            }


            pictureBox.Refresh();
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
        }


        public void RefreshWWWL()
        {
            windowLevel = -600;
            windowWidth = 1000;
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
            if (_point2D == null)
            {
                return;
            }
            _dimensionsCT = _imageAllData.GetDimensions();
            _dataSpacingCT = _imageAllData.GetSpacing();
            matrix = new Matrix();
            _point3D = new double[] { _dimensionsCT[0] / 2, _dimensionsCT[1] / 2, _dimensionsCT[2] / 2 };
            _fastImageDataFetcher = new FastImageDataFetcherShort(_imageAllData);
            _dicomAlldata = _fastImageDataFetcher.GetAllScalar();

            switch (_OrientationEnum)
            {
                case OrientationEnum.Sagittal:
                    _unsafeBmpCT = new UnsafeBitmap(_dimensionsCT[1], _dimensionsCT[2]);

                    tbFrame.Maximum = _dimensionsCT[0] - 1;
                    tbFrame.Value = tbFrame.Maximum / 2;
                    UpdateCTSagitalImage();
                    _point2D = PositionOperation.Position_3D_2_SagitalCT(_point3D, _dataSpacingCT, _unsafeBmpCT.Bitmap.Width, _unsafeBmpCT.Bitmap.Height);
                    break;
                case OrientationEnum.Coronal:
                    _unsafeBmpCT = new UnsafeBitmap(_dimensionsCT[0], _dimensionsCT[2]);
                    tbFrame.Maximum = _dimensionsCT[1] - 1;
                    tbFrame.Value = tbFrame.Maximum / 2;
                    UpdateCTCoronalImage();
                    _point2D = PositionOperation.Position_3D_2_ConoralCT(_point3D, _dataSpacingCT, _unsafeBmpCT.Bitmap.Width, _unsafeBmpCT.Bitmap.Height);
                    break;
                    break;
                case OrientationEnum.Axial:
                    _unsafeBmpCT = new UnsafeBitmap(_dimensionsCT[0], _dimensionsCT[1]);
                    tbFrame.Maximum = _dimensionsCT[2] - 1;
                    tbFrame.Value = tbFrame.Maximum / 2;
                    UpdateCTAxialImage();
                    _point2D = PositionOperation.Position_3D_2_Axial_CT(_point3D, _dataSpacingCT, _unsafeBmpCT.Bitmap.Width, _unsafeBmpCT.Bitmap.Height);
                    break;
                case OrientationEnum.Unknown:
                    break;
                default:
                    break;
            }
            SetPictureBoxScaleLevel(_bitmapCT, pictureBox, ref matrix);

            pictureBox.Refresh();
            //matrix.RotateAt(10, new PointF((float)_point2D[0], (float)_point2D[1]));
            //pictureBox.Refresh();
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
            Console.WriteLine(pictureBox.Width + "  " + pictureBox.Height);
            matrix = new System.Drawing.Drawing2D.Matrix();
            float a = (float)pictureBox.Width / bitmap.Width;
            float b = (float)pictureBox.Height / bitmap.Height;
            if (a <= b)
            {
                matrix.Scale(a, a);
                PointF start = new PointF((float)(bitmap.Width * a / 2), (float)(bitmap.Height * a / 2));
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

                PointF start = new PointF((float)(bitmap.Width * b / 2), (float)(bitmap.Height * b / 2));
                PointF end = new PointF((float)(pictureBox.Width / 2), (float)(pictureBox.Height / 2));
                matrix.Invert();
                var pts = new PointF[] { start, end };
                matrix.TransformPoints(pts);
                matrix.Invert();
                matrix.Translate(pts[1].X - pts[0].X, pts[1].Y - pts[0].Y);
                

            }
        }

        private bool isAddPoint = false , isDeletePoint = false;
        public void StopDrawAirway()
        {
            isAddPoint = false;
            
        }
        //private void btnAddPoint_Click(object sender, EventArgs e)
        public void AddAirwayPoint()
        {
            if (isDrawLesion)
            {
                if (!_hasSaved)
                {

                    DialogResult result = MessageBox.Show("您的数据还未保存，是否需要保存？", "提示：", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {
                        SaveResult();
                    }
                    else if (result == DialogResult.No)
                    {

                    }
                    else if (result == DialogResult.Cancel)
                    {
                        return;
                    }

                }
                isComputing = true; Refresh();
                this.Cursor = Cursors.WaitCursor;
                string airwayMhd = workDirectory + "\\segmented.mha";
                LoadAirwayData(airwayMhd , false);
            }

            SetAllOperationDisable();
            isAddPoint = true;
            isDrawLesion = false;
            isComputing = false;
            isDeletePoint = false;
            this.Cursor = Cursors.Cross;
            _hasSaved = true;
        }


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

        private void btnGetMhd_Click(object sender, EventArgs e)
        {
            isComputing = true; Refresh();
            this.Cursor = Cursors.WaitCursor;
            if (!Directory.Exists(workDirectory))
            {
                Directory.CreateDirectory(workDirectory);
            }

            _airwayFast.SetAllScalar(_airwayData);
            try
            {
                if (!isDrawLesion)
                {
                    string mhaFileName = string.Format("[{0}]_[{1}]_[{2}]_airway_segment.mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    string vtpFileName = string.Format("[{0}]_[{1}]_[{2}]_airway_segment.vtp", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    _airwayMhaNewFilePath = workDirectory + "\\" + mhaFileName;
                    _connectiveAirwayVtpNewFilePath = workDirectory + "\\" + vtpFileName;
                    File.WriteAllText("lastGenerateAirwayVtpFileName.txt", _connectiveAirwayVtpNewFilePath);
                    
                    FileOperation.WriteImageData(_airwayMhaNewFilePath, _connectiveAirwayVtpNewFilePath, _airwayImageData);
                }
                else
                {
                    string mhaFileName = string.Format("[{0}]_[{1}]_[{2}]_lesion.mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    string vtpFileName = string.Format("[{0}]_[{1}]_[{2}]_lesion.vtp", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    _lesionMhaNewFilePath = workDirectory + "\\" + mhaFileName;
                    _lesionVtpNewFilePath = workDirectory + "\\" + vtpFileName;
                    File.WriteAllText("lastGenerateLesionVtpFileName.txt", _lesionVtpNewFilePath);
                    FileOperation.WriteImageData(_lesionMhaNewFilePath, _lesionVtpNewFilePath, _airwayImageData);
                }
                
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                //throw;
            }
            
            isComputing = false; this.Cursor = Cursors.Arrow;
            Process.Start(workDirectory);
        }

        
        private void btnOpenCT_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            if(file.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText("lastOpenCTFilePath.txt",file.FileName);
                
                LoadCTData(file.FileName);
            }
        }

        private void LoadCTData(string filePath)
        {
            workDirectory = new FileInfo(filePath).Directory.FullName;
            isComputing = true;
            Refresh();
            this.Cursor = Cursors.WaitCursor;
            _ctOpenFileName = filePath;
            vtkMetaImageReader readerCT = new vtkMetaImageReader();
            readerCT.SetFileName(filePath);
            readerCT.Update();
            vtkImageData imageCTdata = readerCT.GetOutput();
            readerCT.Dispose();

            if (true)
            {
                this.SetData(OrientationEnum.Axial, imageCTdata);
            }
            else if (false)
            {
                this.SetData(OrientationEnum.Sagittal, imageCTdata);
            }
            isComputing = false;
            this.Cursor = Cursors.Arrow;
        }

        private FastImageDataFetcherUChar _airwayFast;
        private void btnOpenAirway_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText("lastOpenAirwayFilePath.txt", file.FileName);
                _airwayOpenFilePath = file.FileName;
               LoadAirwayData(file.FileName , true);
               isDrawLesion = false;
            }
            
           
        }

        private void LoadAirwayData(string airwayFilePath , bool isShow3DAirway)
        {
            workDirectory = new FileInfo(airwayFilePath).Directory.FullName;
            isComputing = true; Refresh();
            this.Cursor = Cursors.WaitCursor;

            _airwayImageData = FileOperation.ReadMetaImageData(airwayFilePath);
            int[] dims = _airwayImageData.GetDimensions();
            _airwayFast = new FastImageDataFetcherUChar(_airwayImageData);
            _airwayData = _airwayFast.GetAllScalar();

            UpdateImage();
            if (isShow3DAirway)
            {
                //SaveAndLoad3DAirway();
            }
            //fatherPath = filePath.Substring(0, filePath.LastIndexOf("\\"));
            //dataDirectory = fatherPath + "\\data";
            isComputing = false;
           // MessageBox.Show("打开成功！");
            this.Cursor = Cursors.Arrow;
        }


        private bool isDrawLesion = false;

        public void SetAllOperationDisable()
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
            this.Cursor = Cursors.Arrow;
        }


        private bool isHasOtherOperation()
        {
            return isZoom || isPanning || _isChangeWindowLevelAndWidth;
        }
        //private void btnDrawLesion_Click(object sender, EventArgs e)
        public void StopDrawLesion()
        {
            isDrawLesion = true;
            isAddPoint = false;
        }
        public void DrawLesion()
        {
            if (!_hasSaved)
            {
                if (!isDrawLesion)
                {
                    DialogResult result = MessageBox.Show("您的数据还未保存，是否需要保存？", "提示：", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {
                        SaveResult();
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        return;
                    }
                }
                else
                {

                }
            }
            if (_imageAllData == null)
            {
                return;
            }

            
            

            if (!isDrawLesion)
            {
                this.Cursor = Cursors.WaitCursor;
                _airwayImageData = new vtkImageData();
                string mhaFileName = workDirectory+"\\lesion.0.fixed.mha";
                if (File.Exists(mhaFileName))
                {
                    _airwayImageData = VTKUtil.ReadMetaImage(mhaFileName);
                }
                else
                {
                    _airwayImageData.SetDimensions(_dimensionsCT[0], _dimensionsCT[1], _dimensionsCT[2]);
                    _airwayImageData.SetSpacing(_dataSpacingCT[0], _dataSpacingCT[1], _dataSpacingCT[2]);
                    _airwayImageData.SetScalarTypeToUnsignedChar();
                    _airwayImageData.SetNumberOfScalarComponents(1);
                    _airwayImageData.AllocateScalars();
                }
                _airwayFast = new FastImageDataFetcherUChar(_airwayImageData);
                _airwayData = _airwayFast.GetAllScalar();
                UpdateImage();
            }
            _isDrawUnder400CT = false;

            SetAllOperationDisable();
            isAddPoint = true;
            isDrawLesion = true;
            _hasSaved = true;
            
            //isDrawLesion = false;
            this.Cursor = Cursors.Cross;
            
            //string lesionsPath = _ctOpenFileName.Substring(0, _ctOpenFileName.LastIndexOf("\\")) +"\\lesions";
            //string lesionPath = lesionsPath + "\\lesion " + string.Format("{0:yyyy-MM-dd hh-mm-ss}", DateTime.Now) + "\\data";
            //if (!Directory.Exists(lesionPath))
            //{
            //    Directory.CreateDirectory(lesionPath);
            //}

            //string tempBitmap = @"temp.bmp";
            //for (int i = 0; i < _dimensionsCT[2]; i++)
            //{
            //    string newName = Path.Combine(lesionPath, i.ToString().PadLeft(3, '0') + ".bmp");
            //    File.Copy(tempBitmap, newName, true);
            //}

           // workDirectory = lesionPath;
            //LoadAirwayBitmap(bitmap8Directory);
            
        }

        private void rbAxial_CheckedChanged(object sender, EventArgs e)
        {
            //if (rbAxial.Checked)
            {
                this._OrientationEnum = OrientationEnum.Axial;
                this.SetData(_OrientationEnum , _imageAllData);
            }
        }


        private void rbSagital_CheckedChanged(object sender, EventArgs e)
        {
            //if (rbSagital.Checked)
            {
                this._OrientationEnum = OrientationEnum.Sagittal;
                this.SetData(_OrientationEnum, _imageAllData);
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
                this.SetData(_OrientationEnum, _imageAllData);
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


        private void btnAddAirwayMhd_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Cursor = Cursors.WaitCursor;

                vtkImageData imagedata  = FileOperation.ReadMetaImageData(file.FileName);
                FastImageDataFetcherUChar fast = new FastImageDataFetcherUChar(imagedata);
                byte[] bytes  = fast.GetAllScalar();
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (bytes[i] != 0)
                    {
                        _airwayData[i] = 255;
                    }
                }
                //fatherPath = file.FileName.Substring(0, file.FileName.LastIndexOf("\\"));
                //dataDirectory = fatherPath + "\\data";
                UpdateImage();
                MessageBox.Show("Merge 成功！");
            }
            this.Cursor = Cursors.Arrow;
        }

        private bool isSelectConnectPoints = false;
        private int[] _pointA, _pointB;
        private void btnSelectPointsToConnect_Click(object sender, EventArgs e)
        {
            isSelectConnectPoints = true;
           
            isAddPoint = false;
            isDeletePoint = false;
            isDrawCloedCurve = false;
            isDrawPolyline = false;
            this.Cursor = Cursors.Cross;
        }

        private RendererPackage _rendererPackage;
        private CrossPackage _crossPackage;
        private vtkRenderer _rendererPackage3DTopMostRenderer;
        private XmlPolyDataPackage _marchingCubeConnectiveWholeAirway;
        private XmlPolyDataPackage _marchingCubeWholeAirway;
        private XmlPolyDataPackage _marchingCubeWholeLesion;
        private void btnShow3D_Click(object sender, EventArgs e)
        {

           
        }
        
        void RenderWindowInteractor_ModifiedEvt(vtkObject sender, vtkObjectEventArgs e)
        {
        }
        public void UpdateCamera(double[] cameraPosition, double[] focalPosition)
        {
            //if (_rendererPackage == null)
            //{
            //    return;
            //}

            //if (_crossPackage != null)
            //{
            //    _crossPackage.SetPosition(cameraPosition);
            //}
            _point2D = PositionOperation.ThreeD_2_2D(cameraPosition, _OrientationEnum, _dataSpacingCT, _bitmapCT.Width, _bitmapCT.Height);

            tbFrame.Value = _point2D[2];
            pictureBox.Refresh();
            //_rendererPackage.ResetCameraClippingRange();
            //_rendererPackage.RefreshAll();
        }

        public event EventHandler<PickEventArg> Picked;
        private DateTime lastClickTimeStamp = DateTime.MinValue;
        void _rendererPackage_Picked(object sender, PickEventArg e)
        {
            if ((DateTime.Now - lastClickTimeStamp).TotalMilliseconds < 300 || System.Windows.Forms.Control.ModifierKeys == Keys.Control)
            {
                
                    UpdateCamera(e.Position, e.Position);

                    
                   
            }
            lastClickTimeStamp = DateTime.Now;
        }

        public event EventHandler<SegemntFileEventArgs> SavedSegment;
        public void SaveAndLoad3DAirway()
        {
            //if (cbIsShow3D.Checked)
            if(true)
            {
                isComputing = true; Refresh(); 
                this.Cursor = Cursors.WaitCursor;

                splitContainer1.SplitterDistance = (int)(splitContainer1.Width *0.7);
                if (_rendererPackage == null)
                {
                    _rendererPackage = new RendererPackage(splitContainer1.Panel2);
                    _rendererPackage.cursor.SetColor(1, 0, 0);
                    _rendererPackage.cursor.SetRadius(1);
                    _rendererPackage.cursor.SetOpacity(1f);
                    _rendererPackage.ParallelProjectionOn();
                    _rendererPackage.GradientOn();
                    _rendererPackage.Picked += _rendererPackage_Picked;

                    _rendererPackage3DTopMostRenderer = vtkRenderer.New();
                    _rendererPackage3DTopMostRenderer.SetLayer(1); // top layer    
                    _rendererPackage3DTopMostRenderer.InteractiveOff();



                    //_crossPackage = new CrossPackage(20, _rendererPackage3DTopMostRenderer);//内存错误
                    //_crossPackage.SetColor(1, 0, 0);

                    _rendererPackage.Renderer.RemoveActor(_rendererPackage.cursor.Actor);
                    _rendererPackage3DTopMostRenderer.AddActor(_rendererPackage.cursor.Actor);

                    _rendererPackage.Renderer.SetLayer(0);
                    _rendererPackage.Renderer.GetRenderWindow().SetNumberOfLayers(2);
                    _rendererPackage.Renderer.GetRenderWindow().AddRenderer(_rendererPackage3DTopMostRenderer);
                    _rendererPackage3DTopMostRenderer.SetActiveCamera(_rendererPackage.Renderer.GetActiveCamera());
                    //VMTKUtil.ShowPolydata(_airwayVtpNewFilePath, _lesionVtpNewFilePath);

                    _rendererPackage.RenderWindowInteractor.ModifiedEvt += RenderWindowInteractor_ModifiedEvt;
                }
                if (!isDrawLesion)
                {
                    if (_airwayData == null)
                    {
                        MessageBox.Show("请先加载Segment数据！");
                        return;
                    }

                    if (!Directory.Exists(workDirectory))
                    {
                        Directory.CreateDirectory(workDirectory);
                    }

                    _airwayFast.SetAllScalar(_airwayData);

                   

                    string mhaFileName = string.Format("[{0}]_[{1}]_[{2}]_airway_segment.mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    string connectiveVtpFileName = string.Format("[{0}]_[{1}]_[{2}]_connective_airway_segment.vtp", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    string vtpFileName = string.Format("[{0}]_[{1}]_[{2}]_airway_segment.vtp", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    _airwayMhaNewFilePath = workDirectory + "\\" + mhaFileName;
                    //_connectiveAirwayVtpNewFilePath = workDirectory + "\\" + connectiveVtpFileName;
                    _connectiveAirwayVtpNewFilePath = workDirectory+"\\airway.vtp";

                    string airwayVTPFile = workDirectory + "\\" + vtpFileName;
                    File.WriteAllText("lastGenerateAirwayVtpFileName.txt", _connectiveAirwayVtpNewFilePath);
                    FileOperation.WriteImageData(_airwayMhaNewFilePath, _connectiveAirwayVtpNewFilePath,airwayVTPFile , _airwayImageData);

                   
                    string lesionPath = workDirectory + "\\lesion.0.fixed.vtp";
                    if (SavedSegment != null)
                    {
                        SavedSegment(this ,new SegemntFileEventArgs()
                        {
                            airwayUnconnectiveVtpFileName = vtpFileName,
                            airwayConnectiveVtpFileName = connectiveVtpFileName,
                            lesionVTPFilename = lesionPath
                        });
                    }


                    if (File.Exists(airwayVTPFile))
                    {
                        _marchingCubeWholeAirway = new XmlPolyDataPackage(airwayVTPFile, _rendererPackage.Renderer);
                        _marchingCubeWholeAirway.正面观();
                        _marchingCubeWholeAirway.红色();

                    }
                    if (File.Exists(_connectiveAirwayVtpNewFilePath))
                    {
                        //_airwayVtpNewFilePath = @"C:\AirwayVE\Db\Fang^Binghong_M_2014910171827_7b19e03467354d64929771afc39ffee8\airway.vtp";
                        _marchingCubeConnectiveWholeAirway = new XmlPolyDataPackage(_connectiveAirwayVtpNewFilePath, _rendererPackage.Renderer);
                        _marchingCubeConnectiveWholeAirway.正面观();
                        _marchingCubeConnectiveWholeAirway.粉色();
                    }

                    if (File.Exists(lesionPath))
                    {
                        _marchingCubeWholeLesion = new XmlPolyDataPackage(lesionPath , _rendererPackage.Renderer);
                        //_marchingCubeWholeLesion.正面观();
                        _marchingCubeWholeLesion.黄色();
                    }

                   
                    _rendererPackage.FireModifiedEvent();
                    //_marchingCubeWholeAirway.粉色();
                }
                else
                {
                    _marchingCubeConnectiveWholeAirway.ReplacePolyData(_connectiveAirwayVtpNewFilePath.ReadPolyData());
                }


                _rendererPackage.FireModifiedEvent();
                //_rendererPackage.StartRefreshAll();
                _rendererPackage.ResetCameraAndRefreshAll();
                isComputing = false; this.Cursor = Cursors.Arrow;
                Refresh();

                Application.DoEvents();
            }
            else
            {
                splitContainer1.SplitterDistance = splitContainer1.Width - 2;
            }
        }


        public void Save3DAirwayLX()
        {
            //if (cbIsShow3D.Checked)
            if (true)
            {
                isComputing = true; Refresh();
                this.Cursor = Cursors.WaitCursor;

                
                if (!isDrawLesion)
                {
                    if (_airwayData == null)
                    {
                        MessageBox.Show("请先加载Segment数据！");
                        return;
                    }

                    if (!Directory.Exists(workDirectory))
                    {
                        Directory.CreateDirectory(workDirectory);
                    }

                    _airwayFast.SetAllScalar(_airwayData);

                    string mhaFileName = "segmented.mha";
                    _airwayMhaNewFilePath = workDirectory + "\\" + mhaFileName;
                    if (File.Exists(_airwayMhaNewFilePath))
                    {
                        File.Copy(_airwayMhaNewFilePath, workDirectory + "\\" + string.Format("segmented_[{0}]_[{1}]_[{2}].mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
                    }

                   // string mhaFileName = string.Format("[{0}]_[{1}]_[{2}]_airway_segment.mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    string connectiveVtpFileName = string.Format("[{0}]_[{1}]_[{2}]_connective_airway_segment.vtp", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    string vtpFileName = string.Format("[{0}]_[{1}]_[{2}]_airway_segment.vtp", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    _airwayMhaNewFilePath = workDirectory + "\\" + mhaFileName;
                    //_connectiveAirwayVtpNewFilePath = workDirectory + "\\" + connectiveVtpFileName;
                    _connectiveAirwayVtpNewFilePath = workDirectory + "\\airway.vtp";
                    if (File.Exists(_connectiveAirwayVtpNewFilePath))
                    {
                        File.Copy(_airwayMhaNewFilePath, workDirectory + "\\" + string.Format("airway_[{0}]_[{1}]_[{2}].mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
                    }
                    string airwayUnconnectiveVTPFile = workDirectory + "\\" + vtpFileName;
                    File.WriteAllText("lastGenerateAirwayVtpFileName.txt", _connectiveAirwayVtpNewFilePath);
                    FileOperation.WriteImageData(_airwayMhaNewFilePath, _connectiveAirwayVtpNewFilePath, airwayUnconnectiveVTPFile, _airwayImageData);


                    string lesionPath = workDirectory + "\\lesion.0.fixed.vtp";
                    if (SavedSegment != null)
                    {
                        SavedSegment(this, new SegemntFileEventArgs()
                        {
                            airwayUnconnectiveVtpFileName =airwayUnconnectiveVTPFile,
                            airwayConnectiveVtpFileName = _connectiveAirwayVtpNewFilePath,
                            lesionVTPFilename = workDirectory+"\\"+ lesionPath
                        });
                    }


                    
                }
                else
                {
                    
                }


                isComputing = false; this.Cursor = Cursors.Arrow;
                Refresh();

                Application.DoEvents();
            }
            else
            {
                splitContainer1.SplitterDistance = splitContainer1.Width - 2;
            }
        }

        private void Load3DAirway()
        {
            //if (cbIsShow3D.Checked)
            if (true)
            {
                isComputing = true; Refresh();
                this.Cursor = Cursors.WaitCursor;

                splitContainer1.SplitterDistance = splitContainer1.Width / 2;
                if (_rendererPackage == null)
                {
                    _rendererPackage = new RendererPackage(splitContainer1.Panel2);
                    _rendererPackage.cursor.SetColor(1, 0, 0);
                    _rendererPackage.cursor.SetRadius(1);
                    _rendererPackage.cursor.SetOpacity(1f);
                    _rendererPackage.ParallelProjectionOn();
                    _rendererPackage.GradientOn();
                    _rendererPackage.Picked += _rendererPackage_Picked;

                    _rendererPackage3DTopMostRenderer = vtkRenderer.New();
                    _rendererPackage3DTopMostRenderer.SetLayer(1); // top layer    
                    _rendererPackage3DTopMostRenderer.InteractiveOff();



                    _crossPackage = new CrossPackage(20, _rendererPackage3DTopMostRenderer);
                    _crossPackage.SetColor(1, 0, 0);

                    _rendererPackage.Renderer.RemoveActor(_rendererPackage.cursor.Actor);
                    _rendererPackage3DTopMostRenderer.AddActor(_rendererPackage.cursor.Actor);

                    _rendererPackage.Renderer.SetLayer(0);
                    _rendererPackage.Renderer.GetRenderWindow().SetNumberOfLayers(2);
                    _rendererPackage.Renderer.GetRenderWindow().AddRenderer(_rendererPackage3DTopMostRenderer);
                    _rendererPackage3DTopMostRenderer.SetActiveCamera(_rendererPackage.Renderer.GetActiveCamera());
                    //VMTKUtil.ShowPolydata(_airwayVtpNewFilePath, _lesionVtpNewFilePath);

                    _rendererPackage.RenderWindowInteractor.ModifiedEvt += RenderWindowInteractor_ModifiedEvt;
                }
                if (!isDrawLesion)
                {
                    if (_airwayData == null)
                    {
                        MessageBox.Show("请先加载Segment数据！");
                        return;
                    }

                    if (!Directory.Exists(workDirectory))
                    {
                        Directory.CreateDirectory(workDirectory);
                    }




                    string mhaFileName = string.Format("[{0}]_[{1}]_[{2}]_airway_segment.mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    string connectiveVtpFileName = string.Format("[{0}]_[{1}]_[{2}]_connective_airway_segment.vtp", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    string vtpFileName = string.Format("[{0}]_[{1}]_[{2}]_airway_segment.vtp", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    _airwayMhaNewFilePath = workDirectory + "\\" + mhaFileName;
                    _connectiveAirwayVtpNewFilePath = workDirectory + "\\" + connectiveVtpFileName;

                    string airwayVTPFile = workDirectory + "\\" + vtpFileName;
                    File.WriteAllText("lastGenerateAirwayVtpFileName.txt", _connectiveAirwayVtpNewFilePath);
                    FileOperation.WriteImageData(_airwayMhaNewFilePath, _connectiveAirwayVtpNewFilePath, airwayVTPFile, _airwayImageData);

                    if (File.Exists(airwayVTPFile))
                    {
                        _marchingCubeWholeAirway = new XmlPolyDataPackage(airwayVTPFile, _rendererPackage.Renderer);
                        _marchingCubeWholeAirway.正面观();
                        _marchingCubeWholeAirway.红色();

                    }
                    if (File.Exists(_connectiveAirwayVtpNewFilePath))
                    {
                        //_airwayVtpNewFilePath = @"C:\AirwayVE\Db\Fang^Binghong_M_2014910171827_7b19e03467354d64929771afc39ffee8\airway.vtp";
                        _marchingCubeConnectiveWholeAirway = new XmlPolyDataPackage(_connectiveAirwayVtpNewFilePath, _rendererPackage.Renderer);
                        _marchingCubeConnectiveWholeAirway.正面观();
                        _marchingCubeConnectiveWholeAirway.粉色();
                    }


                    _rendererPackage.FireModifiedEvent();
                    //_marchingCubeWholeAirway.粉色();
                }
                else
                {
                    _marchingCubeConnectiveWholeAirway.ReplacePolyData(_connectiveAirwayVtpNewFilePath.ReadPolyData());
                }


                _rendererPackage.FireModifiedEvent();
                //_rendererPackage.StartRefreshAll();
                _rendererPackage.ResetCameraAndRefreshAll();
                isComputing = false; this.Cursor = Cursors.Arrow;
                Refresh();

                Application.DoEvents();
                Process.Start(workDirectory);
            }
            else
            {
                splitContainer1.SplitterDistance = splitContainer1.Width - 2;
            }
        }

        private 解刨点Index _Index解刨点 ;
        private bool isSelectCharact = false;
        private void btn隆突_Click(object sender, EventArgs e)
        {
            if (_Index解刨点 == null)
            {
                _Index解刨点 = new 解刨点Index();
            }
            SetCharactValue(ref _Index解刨点.Index隆突 , sender);

        }

        private void btn左上叶_Click(object sender, EventArgs e)
        {
            if (_Index解刨点 == null)
            {
                _Index解刨点 = new 解刨点Index();
            }
            SetCharactValue(ref _Index解刨点.Index左上叶, sender);
        }

        private void btn左舌叶_Click(object sender, EventArgs e)
        {
            if (_Index解刨点 == null)
            {
                _Index解刨点 = new 解刨点Index();
            }
            SetCharactValue(ref _Index解刨点.Index左舌叶, sender);
        }



        private void btn左背段_Click(object sender, EventArgs e)
        {
            if (_Index解刨点 == null)
            {
                _Index解刨点 = new 解刨点Index();
            }
            SetCharactValue(ref _Index解刨点.Index左背段, sender);

        }

        private void btn左基底_Click(object sender, EventArgs e)
        {
            if (_Index解刨点 == null)
            {
                _Index解刨点 = new 解刨点Index();
            }
            SetCharactValue(ref _Index解刨点.Index左基底, sender);

            
        }

        private void SetCharactValue(ref int? charactValue,object sender)
        {
            if (isSelectCharact)
            {
                charactValue = GetArrayIndex((int)_point2D[0], (int)_point2D[1], (int)(tbFrame.Value), _dimensionsCT, _bitmapAirway.Height, _bitmapAirway.Width);
                (sender as System.Windows.Forms.Button).BackColor = Color.Gray;
                hasSaveCharactValue = false;
            }
            else if (charactValue.HasValue)
            {
                _point2D = ArrayIndexToPoint2D(charactValue.Value, _dimensionsCT, _bitmapAirway.Height, _bitmapAirway.Width);
                double[] point3D = PositionOperation.CT2D_2_3D(_point2D, OrientationEnum.Axial, _dataSpacingCT, _bitmapAirway.Width, _bitmapAirway.Height);
                JumpTo(point3D);
                pictureBox.Refresh();
            }
            
        }
        private void btn右上叶_Click(object sender, EventArgs e)
        {
            if (_Index解刨点 == null)
            {
                _Index解刨点 = new 解刨点Index();
            }
            SetCharactValue(ref _Index解刨点.Index右上叶, sender);

        }

        private void btn右中叶_Click(object sender, EventArgs e)
        {
            if (_Index解刨点 == null)
            {
                _Index解刨点 = new 解刨点Index();
            }
            SetCharactValue(ref _Index解刨点.Index右中叶, sender);

        }

        private void btn右背段_Click(object sender, EventArgs e)
        {
            if (_Index解刨点 == null)
            {
                _Index解刨点 = new 解刨点Index();
            }
            SetCharactValue(ref _Index解刨点.Index右背段, sender);

            
        }

        private void btn右基底_Click(object sender, EventArgs e)
        {
            if (_Index解刨点 == null)
            {
                _Index解刨点 = new 解刨点Index();
            }
            SetCharactValue(ref _Index解刨点.Index右基底, sender);

            
        }

        private bool hasSaveCharactValue = true;
        private void btn保存特征点_Click(object sender, EventArgs e)
        {
            if (_Index解刨点 != null)
            {
                try
                {
                    解刨点Index.Save(_Index解刨点 , workDirectory);
                    MessageBox.Show("解剖特征点保存成功！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("解剖特征点保存失败：" + ex.ToString());
                }

                hasSaveCharactValue = true;
            }
            else
            {
                MessageBox.Show("请先选择特征点！");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {

            CheckCharactSave();
        }

        public void CheckCharactSave()
        {
        }

        private Point panelPosition , newPanelPosition;
        private bool isMovePanel = false;
        private void panelCharactButtons_MouseDown(object sender, MouseEventArgs e)
        {
            panelPosition = new Point(e.X , e.Y);
            isMovePanel = true;
        }

        private void panelCharactButtons_MouseMove(object sender, MouseEventArgs e)
        {
            if(isMovePanel)
            {
                if (Math.Abs(e.X - panelPosition.X) > 0.2 || Math.Abs(e.Y - panelPosition.Y) > 0.2)
                {
                }
            }
            //panelCharactButtons.Location = new Point(panelCharactButtons.Location.X + 10 , panelCharactButtons.Location.Y+10);
        }

      

        private void panelCharactButtons_MouseUp(object sender, MouseEventArgs e)
        {

            isMovePanel = false;
        }

        private void btn清除_Click(object sender, EventArgs e)
        {
            
        }
        public void SaveResult()
        {
           
            if (isDrawLesion)
            {
                SaveLesion();
            }
            else
            {
                Save3DAirwayLX();
            }

            _hasSaved = true;
        }
        //private void btnSaveAirwayMha_Click(object sender, EventArgs e)
        public void SaveAirwayMha()
        {
            isComputing = true; Refresh();
            this.Cursor = Cursors.WaitCursor;
            if (!Directory.Exists(workDirectory))
            {
                Directory.CreateDirectory(workDirectory);
            }

            _airwayFast.SetAllScalar(_airwayData);
            try
            {
                string mhaFileName = "segmented.mha";
                string airwayVTPfile = "airway.vtp";
                //sstring mhaFileName = string.Format("[{0}]_[{1}]_[{2}]_airway_segment.mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                string vtpFileName = string.Format("[{0}]_[{1}]_[{2}]_airway_segment.vtp", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                _airwayMhaNewFilePath = workDirectory + "\\" + mhaFileName;
                airwayVTPfile = workDirectory + "\\" + airwayVTPfile;
                if (File.Exists(_airwayMhaNewFilePath))
                {
                    File.Copy(_airwayMhaNewFilePath, workDirectory + "\\" + string.Format("segmented_[{0}]_[{1}]_[{2}].mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));

                }
                if (File.Exists(airwayVTPfile))
                {
                    File.Copy(airwayVTPfile, workDirectory + "\\" + string.Format("airway_[{0}]_[{1}]_[{2}].mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
                }
                //_connectiveAirwayVtpNewFilePath = workDirectory + "\\" + vtpFileName;
                File.WriteAllText("lastGenerateAirwayVtpFileName.txt", _connectiveAirwayVtpNewFilePath);

                FileOperation.WriteImageData(_airwayMhaNewFilePath, _airwayImageData);

            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                //throw;
            }

            isComputing = false;
            //this.Cursor = Cursors.Arrow;
            //Process.Start(workDirectory);
        }

        //private void btnLesion_Click(object sender, EventArgs e)
        public void SaveLesion()
        {
            isComputing = true; Refresh();
            this.Cursor = Cursors.WaitCursor;
            if (!Directory.Exists(workDirectory))
            {
                Directory.CreateDirectory(workDirectory);
            }

            _airwayFast.SetAllScalar(_airwayData);
            try
            {
                string mhaFileName = "lesion.0.fixed.mha";
                    //string mhaFileName = string.Format("[{0}]_[{1}]_[{2}]_lesion.mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                string vtpFileName = "lesion.0.fixed.vtp";

                string vtpFileName1 = "lesion.0.vtp";
                //string vtpFileName = string.Format("[{0}]_[{1}]_[{2}]_lesion.vtp", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    _lesionMhaNewFilePath = workDirectory + "\\" + mhaFileName;
                    _lesionVtpNewFilePath = workDirectory + "\\" + vtpFileName;
                    string _lesionVtpNewFilePath1 = workDirectory + "\\" + vtpFileName1;
                    if (File.Exists(_lesionMhaNewFilePath))
                    {
                        File.Copy(_lesionMhaNewFilePath,workDirectory+"\\"+ string.Format("lesion_0_fixed[{0}]_[{1}]_[{2}].mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
                    }
                    if (File.Exists(_lesionVtpNewFilePath1))
                    {
                        File.Copy(_lesionVtpNewFilePath1, workDirectory + "\\" + string.Format("lesion_0_[{0}]_[{1}]_[{2}].mha", "PatientName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
                    }
                    File.WriteAllText("lastGenerateLesionVtpFileName.txt", _lesionVtpNewFilePath);
                    FileOperation.WriteImageData(_lesionMhaNewFilePath, _lesionVtpNewFilePath, _airwayImageData);
                    File.Copy(_lesionVtpNewFilePath , _lesionVtpNewFilePath1 ,true);

                    if (SavedSegment != null)
                    {
                        SavedSegment(this, new SegemntFileEventArgs()
                        {
                            lesionVTPFilename = workDirectory+"\\"+ _lesionVtpNewFilePath1
                        });
                    }
                    if (File.Exists(_lesionVtpNewFilePath1))
                    {
                        ///显示lesiob.vtp
                        if (_marchingCubeWholeLesion != null)
                        {
                            _rendererPackage.Renderer.RemoveActor(_marchingCubeWholeLesion.Actor);
                            _marchingCubeWholeLesion.Dispose();
                            _marchingCubeWholeLesion = null;
                        }
                        _marchingCubeWholeLesion = new XmlPolyDataPackage(_lesionVtpNewFilePath1, _rendererPackage.Renderer);
                        //_marchingCubeWholeLesion.正面观();
                        _marchingCubeWholeLesion.黄色();

                        _rendererPackage.RefreshAll();
                    }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }

            isComputing = false; this.Cursor = Cursors.Cross;
            
            //Process.Start(workDirectory);
        }



        private bool _isDrawUnder400CT = false;
        private void cbDrawUnder400CT_CheckedChanged(object sender, EventArgs e)
        {
            if(false)
            {
                _isDrawUnder400CT = true;
            }
            else
            {
                _isDrawUnder400CT = false;
            }
        }
    }

    
}

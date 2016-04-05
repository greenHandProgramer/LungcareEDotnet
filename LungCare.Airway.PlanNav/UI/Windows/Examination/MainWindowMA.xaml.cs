using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.ServiceModel;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Net;
using Kitware.VTK;
using System.Drawing;
using System.Diagnostics;
using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.UI.UserControls.Examination;
using LungCare.SupportPlatform.SupportPlatformDAO.VTK;
using AirwayCT.Entity;
using LungCare_Airway_PlanNav.ZLF.Admin;
using LungCare.SupportPlatform.SupportPlatformService;

namespace LungCare.SupportPlatform.UI.Windows.Examination
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class MainWindowMA : Window
    {
        public static MainWindowMA Instance;

        
        public MainWindowMA()
        {
            this.InitializeComponent();
            this.Width = SystemParameters.WorkArea.Width - 100;
            this.Height = SystemParameters.WorkArea.Height - 60;
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;// 获得窗体的 样式

            int oldstyle = MedSys.PresentationCore.AdjustWindow.NativeMethods.GetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE, oldstyle & ~MedSys.PresentationCore.AdjustWindow.NativeMethods.WS_CAPTION);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetLayeredWindowAttributes(hwnd, 1 | 2 << 8 | 3 << 16, 0, MedSys.PresentationCore.AdjustWindow.NativeMethods.LWA_ALPHA);

            this.RefreshRoundWindowRect();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.SizeChanged += MainWindow_SizeChanged;
            headBorder.MouseDown+=headBorder_MouseDown;
            logoImage.Source = GetImageIcon(global::LungCare.SupportPlatform.Properties.Resources.lungcare_white);

            InitButton();
            Instance = this;
        }

        private AirwayPatient _airwayPatient;
        private CTViewControlMA _CTViewControl2;
        private LesionEntity _lesionEntity;
        private string _BaseResultFolder;
        public MainWindowMA(AirwayPatient SelectedPatient, OrientationEnum OrientationEnum, vtkImageData rowCTimageData, string airwayPath)
        {
            this.InitializeComponent();
            //this.Cursor = Cursors.Arrow;
            //if (SystemParameters.WorkArea.Width < this.Width)
            {
                this.Width = SystemParameters.WorkArea.Width - 100;
            }

            //if (SystemParameters.WorkArea.Height < this.Height)
            {
                this.Height = SystemParameters.WorkArea.Height - 20;
            }
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;// 获得窗体的 样式

            int oldstyle = MedSys.PresentationCore.AdjustWindow.NativeMethods.GetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE, oldstyle & ~MedSys.PresentationCore.AdjustWindow.NativeMethods.WS_CAPTION);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetLayeredWindowAttributes(hwnd, 1 | 2 << 8 | 3 << 16, 0, MedSys.PresentationCore.AdjustWindow.NativeMethods.LWA_ALPHA);

            this.RefreshRoundWindowRect();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.SizeChanged += MainWindow_SizeChanged;
            headBorder.MouseDown += headBorder_MouseDown;
            logoImage.Source = GetImageIcon(global::LungCare.SupportPlatform.Properties.Resources.lungcare_white);

            InitButton();

            tbFrame = this.tbRadius.FindName("S1") as Slider;
            tbFrame.Minimum = 1;
           
            tbFrame.Value = 4;
            tbFrame.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(tbFrame_ValueChanged);

            _CTViewControl2 = new CTViewControlMA();
            ctViewControlHost.Child = _CTViewControl2;
            _CTViewControl2.SetDrawRadius((int)tbFrame.Value);
            _airwayPatient = SelectedPatient;
            _CTViewControl2.SavedSegment += _CTViewControl2_SavedSegment;
            _CTViewControl2.Picked += _CTViewControl2_Picked;
            //labelPatientName.Text = "病人姓名："+_airwayPatient.Name;
            //labelDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _CTViewControl2.SetData(OrientationEnum, rowCTimageData, airwayPath, _airwayPatient);

            if (!string.IsNullOrEmpty(airwayPath))
            {
                _BaseResultFolder = Path.GetDirectoryName(airwayPath);
            }
            SaveAndLoad3DAirway(_airwayPatient.AirwayVTP_FileName ,"" , false);

            _lesionEntity = new LesionEntity();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1;
            timer.Tick += timer_Tick;
            timer.Start();

            Instance = this;

            ResumeGUI();
        }
        System.Windows.Forms.Timer timer;

        private double oldHeight = 0;
        public void SwitchSnap()
        {
            oldHeight = this.Height;
            this.Height =( this.Width  - 180)/2;
            
        }


        public void SwithOld()
        {
           // this.Height = oldHeight;
        }
        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            SaveAndLoad3DAirway(_airwayPatient.AirwayVTP_FileName, "" , true);
        }

        public double[] _position;
        void _CTViewControl2_Picked(object sender, PickEventArg e)
        {
            if (_crossPackage != null)
            {
                this._position = e.Position;
                _crossPackage.SetPosition(e.Position);
                _rendererPackage.RefreshAll();
            }
        }

        
        void _CTViewControl2_SavedSegment(object sender, SegemntFileEventArgs e)
        {
            SaveAndLoad3DAirway(e.airwayConnectiveVtpFileName , e.airwayUnconnectiveVtpFileName , false);
        }
        void tbFrame_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (_CTViewControl2 != null)
            {
                _CTViewControl2.SetDrawRadius((int)tbFrame.Value);
            }
        }
        private Slider tbFrame;
        private RibbonStyle.RibbonMenuButton btnWidthAndLevel;
        private RibbonStyle.RibbonMenuButton btnReturn;
        private RibbonStyle.RibbonMenuButton btnSaveAirwayOrLesionResult;
        private RibbonStyle.RibbonMenuButton btnAddAirwayPoint;
        private RibbonStyle.RibbonMenuButton btnUndo;
        private RibbonStyle.RibbonMenuButton btnDrawRectangle;
        
        private RibbonStyle.RibbonMenuButton btnAddLesionPoint;
        private RibbonStyle.RibbonMenuButton btnPanning;
        private RibbonStyle.RibbonMenuButton btnRefresh;
        private RibbonStyle.RibbonMenuButton btnZoomOut;
        private RibbonStyle.RibbonMenuButton btnDeletePoint;
        private RibbonStyle.RibbonMenuButton btnMeasure;
        private RibbonStyle.RibbonMenuButton btnProductPPT;
        private RibbonStyle.RibbonMenuButton btnLesionManage;
        private RibbonStyle.RibbonMenuButton btnUploadRefineResult;
        private void InitButton()
        {
            this.btnWidthAndLevel = new RibbonStyle.RibbonMenuButton();
            this.btnReturn = new RibbonStyle.RibbonMenuButton();
            this.btnSaveAirwayOrLesionResult = new RibbonStyle.RibbonMenuButton();
            this.btnAddAirwayPoint = new RibbonStyle.RibbonMenuButton();
            this.btnUndo = new RibbonStyle.RibbonMenuButton();
            this.btnDeletePoint = new RibbonStyle.RibbonMenuButton();
            this.btnPanning = new RibbonStyle.RibbonMenuButton();
            this.btnAddLesionPoint = new RibbonStyle.RibbonMenuButton();
            this.btnZoomOut = new RibbonStyle.RibbonMenuButton();
            this.btnRefresh = new RibbonStyle.RibbonMenuButton();
            this.btnMeasure = new RibbonStyle.RibbonMenuButton();
            this.btnProductPPT = new RibbonStyle.RibbonMenuButton();
            this.btnLesionManage = new RibbonStyle.RibbonMenuButton();
            this.btnUploadRefineResult = new RibbonStyle.RibbonMenuButton();
            this.btnDrawRectangle = new RibbonStyle.RibbonMenuButton();


            this.btnWidthAndLevel.AllowDrop = true;
            this.btnWidthAndLevel.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnWidthAndLevel.BackColor = System.Drawing.Color.Transparent;
            this.btnWidthAndLevel.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnWidthAndLevel.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnWidthAndLevel.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnWidthAndLevel.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnWidthAndLevel.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnWidthAndLevel.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnWidthAndLevel.FadingSpeed = 20;
            this.btnWidthAndLevel.FlatAppearance.BorderSize = 0;
            this.btnWidthAndLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWidthAndLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnWidthAndLevel.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnWidthAndLevel.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnWidthAndLevel.Image = global::LungCare.SupportPlatform.Properties.Resources.窗宽窗位;
            this.btnWidthAndLevel.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnWidthAndLevel.ImageOffset = 5;
            this.btnWidthAndLevel.IsPressed = false;
            this.btnWidthAndLevel.KeepPress = false;
            // this.widthAndLevel.Location = new System.Drawing.Point(29, 256);
            this.btnWidthAndLevel.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnWidthAndLevel.MenuPos = new System.Drawing.Point(0, 0);
            this.btnWidthAndLevel.Name = "widthAndLevel";
            this.btnWidthAndLevel.Radius = 8;
            this.btnWidthAndLevel.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnWidthAndLevel.Size = new System.Drawing.Size(50, 50);
            this.btnWidthAndLevel.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnWidthAndLevel.SplitDistance = 0;
            this.btnWidthAndLevel.TabIndex = 102;
            this.btnWidthAndLevel.Title = "";
            this.btnWidthAndLevel.UseVisualStyleBackColor = true;
            this.btnWidthAndLevel.Visible = true;
            // 
            // btnReturn
            // 
            this.btnReturn.AllowDrop = true;
            this.btnReturn.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnReturn.BackColor = System.Drawing.Color.Transparent;
            this.btnReturn.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnReturn.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnReturn.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnReturn.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnReturn.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnReturn.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnReturn.FadingSpeed = 20;
            this.btnReturn.FlatAppearance.BorderSize = 0;
            this.btnReturn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReturn.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnReturn.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnReturn.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnReturn.Image = global::LungCare.SupportPlatform.Properties.Resources.Quit;
            this.btnReturn.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnReturn.ImageOffset = 5;
            this.btnReturn.IsPressed = false;
            this.btnReturn.KeepPress = false;
            this.btnReturn.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnReturn.MenuPos = new System.Drawing.Point(0, 0);
            this.btnReturn.Name = "btnReturn";
            this.btnReturn.Radius = 8;
            this.btnReturn.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnReturn.Size = new System.Drawing.Size(50, 50);
            this.btnReturn.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnReturn.SplitDistance = 0;
            this.btnReturn.TabIndex = 94;
            this.btnReturn.Title = "";
            this.btnReturn.UseVisualStyleBackColor = true;
            // 
            // btnSaveLesionResult
            // 
            this.btnSaveAirwayOrLesionResult.AllowDrop = true;
            this.btnSaveAirwayOrLesionResult.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnSaveAirwayOrLesionResult.BackColor = System.Drawing.Color.Transparent;
            this.btnSaveAirwayOrLesionResult.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.保存;
            this.btnSaveAirwayOrLesionResult.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnSaveAirwayOrLesionResult.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnSaveAirwayOrLesionResult.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnSaveAirwayOrLesionResult.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnSaveAirwayOrLesionResult.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnSaveAirwayOrLesionResult.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnSaveAirwayOrLesionResult.FadingSpeed = 20;
            this.btnSaveAirwayOrLesionResult.FlatAppearance.BorderSize = 0;
            this.btnSaveAirwayOrLesionResult.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveAirwayOrLesionResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnSaveAirwayOrLesionResult.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnSaveAirwayOrLesionResult.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnSaveAirwayOrLesionResult.Image = global::LungCare.SupportPlatform.Properties.Resources.保存;
            this.btnSaveAirwayOrLesionResult.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnSaveAirwayOrLesionResult.ImageOffset = 5;
            this.btnSaveAirwayOrLesionResult.IsPressed = false;
            this.btnSaveAirwayOrLesionResult.KeepPress = false;
            this.btnSaveAirwayOrLesionResult.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnSaveAirwayOrLesionResult.MenuPos = new System.Drawing.Point(0, 0);
            this.btnSaveAirwayOrLesionResult.Name = "btnSaveLesionResult";
            this.btnSaveAirwayOrLesionResult.Radius = 8;
            this.btnSaveAirwayOrLesionResult.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnSaveAirwayOrLesionResult.Size = new System.Drawing.Size(50, 50);
            this.btnSaveAirwayOrLesionResult.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnSaveAirwayOrLesionResult.SplitDistance = 0;
            this.btnSaveAirwayOrLesionResult.TabIndex = 101;
            this.btnSaveAirwayOrLesionResult.Title = "";
            this.btnSaveAirwayOrLesionResult.UseVisualStyleBackColor = true;
            // 
            // btnAddPoint
            // 
            this.btnAddAirwayPoint.AllowDrop = true;
            this.btnAddAirwayPoint.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnAddAirwayPoint.BackColor = System.Drawing.Color.Transparent;
            this.btnAddAirwayPoint.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.支气管加点;
            this.btnAddAirwayPoint.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnAddAirwayPoint.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnAddAirwayPoint.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnAddAirwayPoint.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnAddAirwayPoint.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnAddAirwayPoint.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnAddAirwayPoint.FadingSpeed = 20;
            this.btnAddAirwayPoint.FlatAppearance.BorderSize = 0;
            this.btnAddAirwayPoint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddAirwayPoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnAddAirwayPoint.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnAddAirwayPoint.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnAddAirwayPoint.Image = global::LungCare.SupportPlatform.Properties.Resources._7205_气管_2_;
            this.btnAddAirwayPoint.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnAddAirwayPoint.ImageOffset = 5;
            this.btnAddAirwayPoint.IsPressed = false;
            this.btnAddAirwayPoint.KeepPress = false;
            this.btnAddAirwayPoint.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnAddAirwayPoint.MenuPos = new System.Drawing.Point(0, 0);
            this.btnAddAirwayPoint.Name = "btnAddPoint";
            this.btnAddAirwayPoint.Radius = 8;
            this.btnAddAirwayPoint.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnAddAirwayPoint.Size = new System.Drawing.Size(50, 50);
            this.btnAddAirwayPoint.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnAddAirwayPoint.SplitDistance = 0;
            this.btnAddAirwayPoint.TabIndex = 96;
            this.btnAddAirwayPoint.Title = "";
            this.btnAddAirwayPoint.UseVisualStyleBackColor = true;
            // 
            // btnSaveAirwayResult
            // 
            this.btnUndo.AllowDrop = true;
            this.btnUndo.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnUndo.BackColor = System.Drawing.Color.Transparent;
            this.btnUndo.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.确认肿瘤位置;
            this.btnUndo.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnUndo.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnUndo.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnUndo.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnUndo.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnUndo.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnUndo.FadingSpeed = 20;
            this.btnUndo.FlatAppearance.BorderSize = 0;
            this.btnUndo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUndo.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnUndo.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnUndo.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnUndo.Image = global::LungCare.SupportPlatform.Properties.Resources._43;
            this.btnUndo.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnUndo.ImageOffset = 5;
            this.btnUndo.IsPressed = false;
            this.btnUndo.KeepPress = false;
            this.btnUndo.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnUndo.MenuPos = new System.Drawing.Point(0, 0);
            this.btnUndo.Name = "btnSaveAirwayResult";
            this.btnUndo.Radius = 8;
            this.btnUndo.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnUndo.Size = new System.Drawing.Size(50, 50);
            this.btnUndo.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnUndo.SplitDistance = 0;
            this.btnUndo.TabIndex = 100;
            this.btnUndo.Title = "";
            this.btnUndo.UseVisualStyleBackColor = true;


            this.btnRefresh.AllowDrop = true;
            this.btnRefresh.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnRefresh.BackColor = System.Drawing.Color.Transparent;
            this.btnRefresh.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.确认肿瘤位置;
            this.btnRefresh.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnRefresh.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnRefresh.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnRefresh.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnRefresh.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnRefresh.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnRefresh.FadingSpeed = 20;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnRefresh.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnRefresh.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnRefresh.Image = global::LungCare.SupportPlatform.Properties.Resources.png_0651;
            this.btnRefresh.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnRefresh.ImageOffset = 5;
            this.btnRefresh.IsPressed = false;
            this.btnRefresh.KeepPress = false;
            this.btnRefresh.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnRefresh.MenuPos = new System.Drawing.Point(0, 0);
            this.btnRefresh.Name = "btnSaveAirwayResult";
            this.btnRefresh.Radius = 8;
            this.btnRefresh.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnRefresh.Size = new System.Drawing.Size(50, 50);
            this.btnRefresh.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnRefresh.SplitDistance = 0;
            this.btnRefresh.TabIndex = 100;
            this.btnRefresh.Title = "";
            this.btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnDeletePoint
            // 
            this.btnDeletePoint.AllowDrop = true;
            this.btnDeletePoint.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnDeletePoint.BackColor = System.Drawing.Color.Transparent;
            this.btnDeletePoint.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnDeletePoint.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnDeletePoint.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnDeletePoint.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnDeletePoint.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnDeletePoint.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnDeletePoint.FadingSpeed = 20;
            this.btnDeletePoint.FlatAppearance.BorderSize = 0;
            this.btnDeletePoint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeletePoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnDeletePoint.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnDeletePoint.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnDeletePoint.Image = global::LungCare.SupportPlatform.Properties.Resources.擦除2;
            this.btnDeletePoint.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnDeletePoint.ImageOffset = 5;
            this.btnDeletePoint.IsPressed = false;
            this.btnDeletePoint.KeepPress = false;
            this.btnDeletePoint.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnDeletePoint.MenuPos = new System.Drawing.Point(0, 0);
            this.btnDeletePoint.Name = "btnDeletePoint";
            this.btnDeletePoint.Radius = 8;
            this.btnDeletePoint.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnDeletePoint.Size = new System.Drawing.Size(50, 50);
            this.btnDeletePoint.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnDeletePoint.SplitDistance = 0;
            this.btnDeletePoint.TabIndex = 95;
            this.btnDeletePoint.Title = "";
            this.btnDeletePoint.UseVisualStyleBackColor = true;
            // 
            // btnZoomIn
            // 
            this.btnPanning.AllowDrop = true;
            this.btnPanning.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnPanning.BackColor = System.Drawing.Color.Transparent;
            this.btnPanning.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.确认肿瘤位置;
            this.btnPanning.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnPanning.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnPanning.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnPanning.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnPanning.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnPanning.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnPanning.FadingSpeed = 20;
            this.btnPanning.FlatAppearance.BorderSize = 0;
            this.btnPanning.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPanning.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnPanning.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnPanning.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnPanning.Image = global::LungCare.SupportPlatform.Properties.Resources.hand;
            this.btnPanning.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnPanning.ImageOffset = 5;
            this.btnPanning.IsPressed = false;
            this.btnPanning.KeepPress = false;
            this.btnPanning.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnPanning.MenuPos = new System.Drawing.Point(0, 0);
            this.btnPanning.Name = "btnZoomIn";
            this.btnPanning.Radius = 8;
            this.btnPanning.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnPanning.Size = new System.Drawing.Size(50, 50);
            this.btnPanning.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnPanning.SplitDistance = 0;
            this.btnPanning.TabIndex = 98;
            this.btnPanning.Title = "";
            this.btnPanning.UseVisualStyleBackColor = true;
            // 
            // btnDrawLesion
            // 
            this.btnAddLesionPoint.AllowDrop = true;
            this.btnAddLesionPoint.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnAddLesionPoint.BackColor = System.Drawing.Color.Transparent;
            this.btnAddLesionPoint.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.确认肿瘤位置;
            this.btnAddLesionPoint.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnAddLesionPoint.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnAddLesionPoint.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnAddLesionPoint.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnAddLesionPoint.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnAddLesionPoint.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnAddLesionPoint.FadingSpeed = 20;
            this.btnAddLesionPoint.FlatAppearance.BorderSize = 0;
            this.btnAddLesionPoint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddLesionPoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnAddLesionPoint.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnAddLesionPoint.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnAddLesionPoint.Image = global::LungCare.SupportPlatform.Properties.Resources.病灶;
            this.btnAddLesionPoint.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnAddLesionPoint.ImageOffset = 5;
            this.btnAddLesionPoint.IsPressed = false;
            this.btnAddLesionPoint.KeepPress = false;
            this.btnAddLesionPoint.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnAddLesionPoint.MenuPos = new System.Drawing.Point(0, 0);
            this.btnAddLesionPoint.Name = "btnDrawLesion";
            this.btnAddLesionPoint.Radius = 8;
            this.btnAddLesionPoint.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnAddLesionPoint.Size = new System.Drawing.Size(50, 50);
            this.btnAddLesionPoint.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnAddLesionPoint.SplitDistance = 0;
            this.btnAddLesionPoint.TabIndex = 99;
            this.btnAddLesionPoint.Title = "";
            this.btnAddLesionPoint.UseVisualStyleBackColor = true;
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.AllowDrop = true;
            this.btnZoomOut.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnZoomOut.BackColor = System.Drawing.Color.Transparent;
            this.btnZoomOut.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.确认肿瘤位置;
            this.btnZoomOut.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnZoomOut.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnZoomOut.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnZoomOut.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnZoomOut.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnZoomOut.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnZoomOut.FadingSpeed = 20;
            this.btnZoomOut.FlatAppearance.BorderSize = 0;
            this.btnZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnZoomOut.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnZoomOut.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnZoomOut.Image = global::LungCare.SupportPlatform.Properties.Resources.png_00051;
            this.btnZoomOut.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnZoomOut.ImageOffset = 5;
            this.btnZoomOut.IsPressed = false;
            this.btnZoomOut.KeepPress = false;
            this.btnZoomOut.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnZoomOut.MenuPos = new System.Drawing.Point(0, 0);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Radius = 8;
            this.btnZoomOut.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnZoomOut.Size = new System.Drawing.Size(50, 50);
            this.btnZoomOut.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnZoomOut.SplitDistance = 0;
            this.btnZoomOut.TabIndex = 97;
            this.btnZoomOut.Title = "";
            this.btnZoomOut.UseVisualStyleBackColor = true;

            this.btnMeasure.AllowDrop = true;
            this.btnMeasure.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnMeasure.BackColor = System.Drawing.Color.Transparent;
            this.btnMeasure.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.ruler1;
            this.btnMeasure.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnMeasure.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnMeasure.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnMeasure.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnMeasure.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnMeasure.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnMeasure.FadingSpeed = 20;
            this.btnMeasure.FlatAppearance.BorderSize = 0;
            this.btnMeasure.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMeasure.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnMeasure.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnMeasure.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnMeasure.Image = global::LungCare.SupportPlatform.Properties.Resources.ruler1;
            this.btnMeasure.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnMeasure.ImageOffset = 5;
            this.btnMeasure.IsPressed = false;
            this.btnMeasure.KeepPress = false;
            this.btnMeasure.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnMeasure.MenuPos = new System.Drawing.Point(0, 0);
            this.btnMeasure.Name = "btnMeasure";
            this.btnMeasure.Radius = 8;
            this.btnMeasure.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnMeasure.Size = new System.Drawing.Size(50, 50);
            this.btnMeasure.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnMeasure.SplitDistance = 0;
            this.btnMeasure.TabIndex = 97;
            this.btnMeasure.Title = "";
            this.btnMeasure.UseVisualStyleBackColor = true;


            this.btnProductPPT.AllowDrop = true;
            this.btnProductPPT.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnProductPPT.BackColor = System.Drawing.Color.Transparent;
            this.btnProductPPT.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.ruler1;
            this.btnProductPPT.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnProductPPT.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnProductPPT.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnProductPPT.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnProductPPT.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnProductPPT.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnProductPPT.FadingSpeed = 20;
            this.btnProductPPT.FlatAppearance.BorderSize = 0;
            this.btnProductPPT.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProductPPT.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnProductPPT.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnProductPPT.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnProductPPT.Image = global::LungCare.SupportPlatform.Properties.Resources.ppt;
            this.btnProductPPT.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnProductPPT.ImageOffset = 5;
            this.btnProductPPT.IsPressed = false;
            this.btnProductPPT.KeepPress = false;
            this.btnProductPPT.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnProductPPT.MenuPos = new System.Drawing.Point(0, 0);
            this.btnProductPPT.Name = "btnProductPPT";
            this.btnProductPPT.Radius = 8;
            this.btnProductPPT.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnProductPPT.Size = new System.Drawing.Size(50, 50);
            this.btnProductPPT.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnProductPPT.SplitDistance = 0;
            this.btnProductPPT.TabIndex = 97;
            this.btnProductPPT.Title = "";
            this.btnProductPPT.UseVisualStyleBackColor = true;

            this.btnLesionManage.AllowDrop = true;
            this.btnLesionManage.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnLesionManage.BackColor = System.Drawing.Color.Transparent;
            this.btnLesionManage.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.ruler1;
            this.btnLesionManage.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnLesionManage.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnLesionManage.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnLesionManage.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnLesionManage.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnLesionManage.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnLesionManage.FadingSpeed = 20;
            this.btnLesionManage.FlatAppearance.BorderSize = 0;
            this.btnLesionManage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLesionManage.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnLesionManage.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnLesionManage.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnLesionManage.Image = global::LungCare.SupportPlatform.Properties.Resources._48;
            this.btnLesionManage.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnLesionManage.ImageOffset = 5;
            this.btnLesionManage.IsPressed = false;
            this.btnLesionManage.KeepPress = false;
            this.btnLesionManage.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnLesionManage.MenuPos = new System.Drawing.Point(0, 0);
            this.btnLesionManage.Name = "btnAddLesion";
            this.btnLesionManage.Radius = 8;
            this.btnLesionManage.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnLesionManage.Size = new System.Drawing.Size(50, 50);
            this.btnLesionManage.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnLesionManage.SplitDistance = 0;
            this.btnLesionManage.TabIndex = 97;
            this.btnLesionManage.Title = "";
            this.btnLesionManage.UseVisualStyleBackColor = true;
            
            this.btnUploadRefineResult.AllowDrop = true;
            this.btnUploadRefineResult.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnUploadRefineResult.BackColor = System.Drawing.Color.Transparent;
            this.btnUploadRefineResult.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.ruler1;
            this.btnUploadRefineResult.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnUploadRefineResult.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnUploadRefineResult.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnUploadRefineResult.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnUploadRefineResult.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnUploadRefineResult.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnUploadRefineResult.FadingSpeed = 20;
            this.btnUploadRefineResult.FlatAppearance.BorderSize = 0;
            this.btnUploadRefineResult.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUploadRefineResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnUploadRefineResult.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnUploadRefineResult.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnUploadRefineResult.Image = global::LungCare.SupportPlatform.Properties.Resources.upload;
            this.btnUploadRefineResult.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnUploadRefineResult.ImageOffset = 5;
            this.btnUploadRefineResult.IsPressed = false;
            this.btnUploadRefineResult.KeepPress = false;
            this.btnUploadRefineResult.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnUploadRefineResult.MenuPos = new System.Drawing.Point(0, 0);
            this.btnUploadRefineResult.Name = "btnUploadRefineResult";
            this.btnUploadRefineResult.Radius = 8;
            this.btnUploadRefineResult.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnUploadRefineResult.Size = new System.Drawing.Size(50, 50);
            this.btnUploadRefineResult.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnUploadRefineResult.SplitDistance = 0;
            this.btnUploadRefineResult.TabIndex = 97;
            this.btnUploadRefineResult.Title = "";
            this.btnUploadRefineResult.UseVisualStyleBackColor = true;

            
            this.btnDrawRectangle.AllowDrop = true;
            this.btnDrawRectangle.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnDrawRectangle.BackColor = System.Drawing.Color.Transparent;
            this.btnDrawRectangle.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.ruler1;
            this.btnDrawRectangle.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnDrawRectangle.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnDrawRectangle.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnDrawRectangle.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnDrawRectangle.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnDrawRectangle.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnDrawRectangle.FadingSpeed = 20;
            this.btnDrawRectangle.FlatAppearance.BorderSize = 0;
            this.btnDrawRectangle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDrawRectangle.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnDrawRectangle.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnDrawRectangle.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnDrawRectangle.Image = global::LungCare.SupportPlatform.Properties.Resources.画矩形;
            this.btnDrawRectangle.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnDrawRectangle.ImageOffset = 5;
            this.btnDrawRectangle.IsPressed = false;
            this.btnDrawRectangle.KeepPress = false;
            this.btnDrawRectangle.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnDrawRectangle.MenuPos = new System.Drawing.Point(0, 0);
            this.btnDrawRectangle.Name = "btnDrawRectangle";
            this.btnDrawRectangle.Radius = 8;
            this.btnDrawRectangle.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnDrawRectangle.Size = new System.Drawing.Size(50, 50);
            this.btnDrawRectangle.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnDrawRectangle.SplitDistance = 0;
            this.btnDrawRectangle.TabIndex = 97;
            this.btnDrawRectangle.Title = "";
            this.btnDrawRectangle.UseVisualStyleBackColor = true;

            btnAddAirwayPoint.Click += btnAddAirwayPoint_Click;
            btnAddLesionPoint.Click += btnAddLesionPoint_Click;
            btnLesionManage.Click += btnAddLesion_Click;
            btnDeletePoint.Click += btnDeletePoint_Click;
            btnUndo.Click += btnUndo_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnSaveAirwayOrLesionResult.Click += btnSaveAirwayOrLesionResult_Click;
            btnZoomOut.Click += btnZoomOut_Click;
            btnPanning.Click += btnPanning_Click;
            btnWidthAndLevel.Click += btnWidthAndLevel_Click;
            btnReturn.Click += btnReturn_Click;
            btnMeasure.Click += btnMeasure_Click;
            btnUploadRefineResult.Click += btnUploadRefineResult_Click;
            btnProductPPT.Click += btnProductPPT_Click;
            btnDrawRectangle.Click += btnDrawRectangle_Click;
            
            panel.Controls.Add(btnAddAirwayPoint);
            panel.Controls.Add(btnAddLesionPoint);
            panel.Controls.Add(btnDeletePoint);
            panel.Controls.Add(btnUndo);
            panel.Controls.Add(btnSaveAirwayOrLesionResult);

            panel2.Controls.Add(btnDrawRectangle);
            panel2.Controls.Add(btnLesionManage);
            panel2.Controls.Add(btnProductPPT);
            
            
            
            panel2.Controls.Add(btnUploadRefineResult);
            
            panel1.Controls.Add(btnZoomOut);
            panel1.Controls.Add(btnPanning);
            panel1.Controls.Add(btnMeasure);
            panel1.Controls.Add(btnWidthAndLevel);
            panel1.Controls.Add(btnRefresh);
            panel1.Controls.Add(btnReturn);

            System.Windows.Forms.ToolTip p1 = new System.Windows.Forms.ToolTip();
            p1.ShowAlways = true;
            p1.SetToolTip(this.btnAddAirwayPoint, "支气管加点");
            p1.SetToolTip(this.btnAddLesionPoint, "病灶加点");
            p1.SetToolTip(this.btnDeletePoint, "擦除");
            p1.SetToolTip(this.btnUndo, "撤销");
            p1.SetToolTip(this.btnRefresh, "刷新");
            p1.SetToolTip(this.btnSaveAirwayOrLesionResult, "保存");
            p1.SetToolTip(this.btnUploadRefineResult, "上传");

            System.Windows.Forms.ToolTip p2 = new System.Windows.Forms.ToolTip();
            p2.ShowAlways = true;
            p2.SetToolTip(this.btnZoomOut, "放缩");
            p2.SetToolTip(this.btnPanning, "移动");
            p2.SetToolTip(this.btnMeasure, "测量");
            p2.SetToolTip(this.btnWidthAndLevel, "窗宽窗位");
            p2.SetToolTip(this.btnReturn, "返回");
        }

        void btnDrawRectangle_Click(object sender, EventArgs e)
        {
            if (!btnDrawRectangle.KeepPress)
            {
                ButtonToInital();
                btnDrawRectangle.KeepPress = true;
                _CTViewControl2.StartDrawRectangle();

            }
            else
            {
                btnDrawRectangle.KeepPress = false;
                _CTViewControl2.SetAllOperationDisable();
                _CTViewControl2.StopDrawRectangle();
            }
            //throw new NotImplementedException();
        }


        private ExaminationPPTService examinationPPTService;
        void btnProductPPT_Click(object sender, EventArgs e)
        {
            //AdminWnd main = new AdminWnd();
            //main.ShowDialog();

            //return;
              System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog
                            {
                                FileName = "朗开医疗体检报告_"+_airwayPatient.Name+".ppt",
                                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                RestoreDirectory = true
                            };

              if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
              {
                  _CTViewControl2.StartHandleData();
                  string saveDirecory = _BaseResultFolder + "\\Snapshot";
                  if (!Directory.Exists(saveDirecory))
                  {
                      Directory.CreateDirectory(saveDirecory);
                  }
                  _marchingCubeWholeAirway.正面观();
                  _rendererPackage.ResetCameraAndRefreshAll();
                  _rendererPackage.ScreenShotToFile(Path.Combine(saveDirecory, "1.jpg"));
                  for (int i = 2; i < 5; i++)
                  {
                      rotateAzimuth3D(90, Path.Combine(saveDirecory, i + ".jpg"));
                  }

                  _marchingCubeWholeAirway.正面观();
                  _rendererPackage.ResetCameraAndRefreshAll();

                  rotateElevation3D(65, Path.Combine(saveDirecory, "5.jpg"));
                  rotateElevation3D(180, Path.Combine(saveDirecory, "6.jpg"));

                  try
                  {
                      examinationPPTService = new ExaminationPPTService(_BaseResultFolder);
                      examinationPPTService.SetHead(_airwayPatient);
                      examinationPPTService.AddRotate3DImages();
                      examinationPPTService.AddLesionsSnapshotToPPT();
                      examinationPPTService.SavePPT(sfd.FileName);
                  }
                  catch (Exception ex)
                  {
                      MessageBox.Show(ex.ToString());
                      _CTViewControl2.StopHandleData();
                      //MessageBox.Show("PPT创建失败，请先安装Office 2007 !");
                      return;
                  }

                  _CTViewControl2.StopHandleData();
              }
          
        }


        private void rotateElevation3D(double angle ,string saveFile)
        {
            if (_marchingCubeWholeAirway != null)
            {
               // _marchingCubeWholeAirway.正面观();
               
                _rendererPackage.Camera.Elevation(angle);
                _rendererPackage.FireModifiedEvent();
                _rendererPackage.ResetCameraAndRefreshAll();

                _rendererPackage.ScreenShotToFile(saveFile);
            }
        }

        private void rotateAzimuth3D(double angle ,string saveFile)
        {
            if (_marchingCubeWholeAirway != null)
            {
               
              
                _rendererPackage.Camera.Azimuth(90);
                _rendererPackage.FireModifiedEvent();
                _rendererPackage.ResetCameraAndRefreshAll();
                _rendererPackage.ScreenShotToFile(saveFile);
            }
        }

        void btnUploadRefineResult_Click(object sender, EventArgs e)
        {
            _CTViewControl2.StartHandleData();
            string zipfilename = zipFolder2Zip(Path.GetDirectoryName(_airwayPatient.SegmentedMhd_FileName));
            //System.Windows.MessageBox.Show("saved success");
            UploadProgressWndResultMA upw = new UploadProgressWndResultMA(_airwayPatient.OrderID);
            //upw.Owner = LungCare_Airway_PlanNav.MainWindow.Instance;
            upw.FileName = zipfilename;
            upw.InstitutionName = _airwayPatient.Institution;
            upw.PatientAge = _airwayPatient.Age;
            upw.PatientName = _airwayPatient.Name;
            upw.PatientSex = _airwayPatient.Sex;
            upw.SeriesInstanceUID = _airwayPatient.PatientId;
            //upw.StudyInstanceUID = _airwayPatient.StudyInstanceUID;
            //upw.acquisitionDate = _airwayPatient.AcquisitionDate;
            //upw.acquisitionTime = _airwayPatient.AcquisitionTime;

            upw.ShowDialog();

            _CTViewControl2.StopHandleData();
            //throw new NotImplementedException();
        }


        private string zipFolder2Zip(string folder)
        {

            string fileName = string.Format("[{0}][{3}][{1}][{2}].zip", _airwayPatient.Name, _airwayPatient.Institution, System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

            //string args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\" \"{3}\"", fileName, SelectedPatient.GetFile(""), SelectedPatient.DicomFolder, fileName);
            string args = string.Format("a -tzip \"{0}\" \"{1}\"", fileName, folder);
            var psi = new ProcessStartInfo(System.Windows.Forms.Application.StartupPath + @"\HaoZipC.exe", args);
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(psi).WaitForExit();
//            System.Windows.Forms.MessageBox.Show(string.Format(@"压缩完毕。
//文件保存到{0}。", fileName));
            //Process.Start(new FileInfo(fileName).Directory.FullName);



            return fileName;
        }

        private LesionEntities _listLesion = new LesionEntities();
        public void AddLesion(LesionEntity lesion)
        {
            _listLesion = LesionEntities.TestLoad(_BaseResultFolder);
            if (_listLesion == null)
            {
                _listLesion = new LesionEntities();
            }
            _listLesion.Add(lesion);
            LesionEntities.TestSave(_listLesion , _BaseResultFolder);
        }
        void btnAddLesion_Click(object sender, EventArgs e)
        {
            AllLesionWindow alllesions = new AllLesionWindow(_BaseResultFolder ,_airwayPatient);
            alllesions.Owner = this;
            alllesions.Topmost = true;
            alllesions.Show();
            //throw new NotImplementedException();
        }

        

        private void ButtonToInital()
        {
            btnAddAirwayPoint.KeepPress = false;
            btnAddLesionPoint.KeepPress =false;
            btnDeletePoint.KeepPress = false;
            btnPanning.KeepPress = false;
            btnRefresh.KeepPress = false;
            btnReturn.KeepPress = false;
            btnSaveAirwayOrLesionResult.KeepPress = false;
            btnUndo.KeepPress = false;
            btnWidthAndLevel.KeepPress = false;
            btnZoomOut.KeepPress = false;
            btnMeasure.KeepPress = false;
            btnDrawRectangle.KeepPress = false;
            System.Windows.Forms.Application.DoEvents();
        }

        void btnReturn_Click(object sender, EventArgs e)
        {
            if (!_CTViewControl2.HasSaved)
            {
                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("您的数据还未保存，是否需要保存？", "提示：", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    _CTViewControl2.SaveResult();
                    return;
                }
                else if (result == System.Windows.Forms.DialogResult.No){
                    this.Close();
                    return;
                }
                else if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
            }
            if (MessageBox.Show("是否退出？", "退出", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        public bool _isChangeWindowLevelAndWidth = false;
        void btnWidthAndLevel_Click(object sender, EventArgs e)
        {
            ButtonToInital();
            btnWidthAndLevel.KeepPress = true;
            _CTViewControl2.startWindowLevelAndWidth();
        }

        void btnPanning_Click(object sender, EventArgs e)
        {
            ButtonToInital();
            btnPanning.KeepPress = true;
            _CTViewControl2.StartPanning();
        }

        void btnZoomOut_Click(object sender, EventArgs e)
        {
            ButtonToInital();
            btnZoomOut.KeepPress = true;
            _CTViewControl2.StartZoom();
        }

        void btnMeasure_Click(object sender, EventArgs e)
        {
            
            ButtonToInital();
            btnMeasure.KeepPress = true;
            _CTViewControl2.StartMeasure();
            //throw new NotImplementedException();
        }
        void btnSaveAirwayOrLesionResult_Click(object sender, EventArgs e)
        {
            //ButtonToInital();
            btnSaveAirwayOrLesionResult.KeepPress = true;
            _CTViewControl2.SaveResult();
            //SaveAndLoad3DAirway("");
            btnSaveAirwayOrLesionResult.KeepPress = false;
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            _CTViewControl2.SetPictureBoxWidth();
            _CTViewControl2.RefreshWWWL();
        }

        void btnUndo_Click(object sender, EventArgs e)
        {
            _CTViewControl2.Previous();
        }

        void btnDeletePoint_Click(object sender, EventArgs e)
        {
            ButtonToInital();
            btnDeletePoint.KeepPress = true;
            _CTViewControl2.DeletePoint();
        }

        void btnAddLesionPoint_Click(object sender, EventArgs e)
        {
            if (!btnAddLesionPoint.KeepPress)
            {
                
                ButtonToInital();
                btnAddLesionPoint.KeepPress = true;
                _CTViewControl2.DrawLesion();
                
            }
            else
            {
                btnAddLesionPoint.KeepPress = false;
                _CTViewControl2.SetAllOperationDisable();
                _CTViewControl2.StopDrawLesion();
            }
        }

        void btnAddAirwayPoint_Click(object sender, EventArgs e)
        {
           if(!btnAddAirwayPoint.KeepPress)
           {
               ButtonToInital();
               btnAddAirwayPoint.KeepPress = true;
               _CTViewControl2.AddAirwayPoint();
              
           }else
           {
               btnAddAirwayPoint.KeepPress = false;
               _CTViewControl2.SetAllOperationDisable();
               _CTViewControl2.StopDrawAirway();
           }
            
        }
        //private List<NotificationItem> listMessages;
        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.MinWidth = 254 * 2 + this.Height;
            this.RefreshRoundWindowRect();

            colGrid.ColumnDefinitions[1].Width = new GridLength(this.Height, GridUnitType.Pixel);
            //colGrid.ColumnDefinitions[2].Width = new GridLength(5, GridUnitType.Pixel);
            double w = this.Width - 253 - this.Height;
            //MessageBox.Show(w.ToString());
            //grid3D.ColumnDefinitions[0].Width = new GridLength(w, GridUnitType.Pixel);
            _CTViewControl2.SetPictureBoxWidth();
            
            //gridCTView.ColumnDefinitions[0].Width
            //throw new NotImplementedException();
        }

        private void RefreshRoundWindowRect()
        {
            // 获取窗体句柄
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

            // 创建圆角窗体
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowRgn(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.CreateRoundRectRgn(0, 0, Convert.ToInt32(this.ActualWidth) + 1, Convert.ToInt32(this.ActualHeight) + 1, 5, 5), true);
        }
        
        void headBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {}
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Instance = this;
        }
        private static BitmapImage GetImageIcon(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                bitmap.Save(ms, bitmap.RawFormat);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            catch (Exception ex)
            {}
            return bitmapImage;
        }

        public Bitmap GetCTBitmap()
        {
            if (_CTViewControl2 == null)
            {
                return null;
            }
            return _CTViewControl2.ScreenShot();
        }

        public Bitmap GetAirway3DBitmap()
        {
            if (_rendererPackage == null)
            {
                return null;
            }

            return _rendererPackage.ScreenShot();
        }


        public Bitmap GetCTBitmap(string saveFile)
        {
            if (_CTViewControl2 == null)
            {
                return null;
            }
            return _CTViewControl2.ScreenShot(saveFile);
        }

        public Bitmap GetAirway3DBitmap(string saveFile)
        {
            if (_rendererPackage == null)
            {
                return null;
            }

            return _rendererPackage.ScreenShotToFile(saveFile);
        }

        private Bitmap _axialAllBitmap, _axialPartBitmap, _axial3DBitmap;
        private Bitmap _sagitalAllBitmap, _sagitalPartBitmap, _sagital3DBitmap;
        private Bitmap _coronalAllBitmap, _coronalPartBitmap, _coronal3DBitmap;


        public void SetSnapshotImages(Bitmap axialAllBitmap,Bitmap axialPartBitmap,Bitmap axial3DBitmap,
         Bitmap sagitalAllBitmap,Bitmap sagitalPartBitmap,Bitmap sagital3DBitmap,
         Bitmap coronalAllBitmap, Bitmap coronalPartBitmap, Bitmap coronal3DBitmap)
        {
            _axial3DBitmap = axial3DBitmap;
            _axialAllBitmap = axialAllBitmap;
            _axialPartBitmap = axialPartBitmap;
            _sagital3DBitmap = sagital3DBitmap;
            _sagitalAllBitmap = sagitalAllBitmap;
            _sagitalPartBitmap = sagitalPartBitmap;
            _coronal3DBitmap = coronal3DBitmap;
            _coronalAllBitmap = coronalAllBitmap;
            _coronalPartBitmap = coronalPartBitmap;

            //_axial3DBitmap.Save(@"c:\aa.jpg");
            //_axialAllBitmap.Save(@"c:\bb.jpg");
        }
        private RendererPackage _rendererPackage;
        private CrossPackage _crossPackage;
        private vtkRenderer _rendererPackage3DTopMostRenderer;
        private XmlPolyDataPackage _marchingCubeConnectiveWholeAirway;
        private XmlPolyDataPackage _marchingCubeWholeAirway;
        private XmlPolyDataPackage _marchingCubeWholeLesion;
        private XmlPolyDataPackage _marchingCubeWholeLungregion;

        public void SaveAndLoad3DAirway(string connectiveAirwayVtpFilePath, string unconnectiveAirwayVtpFilePath , bool refreshCamera)
        {
            //if (cbIsShow3D.Checked)
            if (true)
            {
                if (_rendererPackage == null)
                {
                    _rendererPackage = new RendererPackage(airwayPanel);
                    //_rendererPackage.cursor.SetColor(1, 0, 0);
                    //_rendererPackage.cursor.SetRadius(1);
                    //_rendererPackage.cursor.SetOpacity(1f);
                    _rendererPackage.ParallelProjectionOn();
                    _rendererPackage.GradientOn();

                    _rendererPackage.Picked += _rendererPackage_Picked;

                    _rendererPackage3DTopMostRenderer = vtkRenderer.New();
                    _rendererPackage3DTopMostRenderer.SetLayer(1); // top layer    
                    _rendererPackage3DTopMostRenderer.InteractiveOff();



                    _crossPackage = new CrossPackage(20, _rendererPackage3DTopMostRenderer);//内存错误
                    _crossPackage.SetColor(1, 0, 0);

                    //_rendererPackage.Renderer.RemoveActor(_rendererPackage.cursor.Actor);
                    //_rendererPackage3DTopMostRenderer.AddActor(_rendererPackage.cursor.Actor);

                    _rendererPackage.Renderer.SetLayer(0);
                    _rendererPackage.Renderer.GetRenderWindow().SetNumberOfLayers(2);
                    _rendererPackage.Renderer.GetRenderWindow().AddRenderer(_rendererPackage3DTopMostRenderer);
                    _rendererPackage3DTopMostRenderer.SetActiveCamera(_rendererPackage.Renderer.GetActiveCamera());
                    //VMTKUtil.ShowPolydata(_airwayVtpNewFilePath, _lesionVtpNewFilePath);

                    _rendererPackage.RenderWindowInteractor.ModifiedEvt += RenderWindowInteractor_ModifiedEvt;
                }

                if (File.Exists(unconnectiveAirwayVtpFilePath))
                {
                    _marchingCubeConnectiveWholeAirway = new XmlPolyDataPackage(unconnectiveAirwayVtpFilePath, _rendererPackage.Renderer);
                    //_marchingCubeConnectiveWholeAirway.正面观();
                    _marchingCubeConnectiveWholeAirway.红色();
                }
                if (File.Exists(connectiveAirwayVtpFilePath))
                {
                    _marchingCubeWholeAirway = new XmlPolyDataPackage(connectiveAirwayVtpFilePath, _rendererPackage.Renderer);
                    if (refreshCamera)
                    {
                        _marchingCubeWholeAirway.正面观();
                    }
                    _marchingCubeWholeAirway.粉色();
                }

                if (File.Exists(_airwayPatient.LungRegion_VTP))
                {
                    _marchingCubeWholeLungregion = new XmlPolyDataPackage(_airwayPatient.LungRegion_VTP , _rendererPackage.Renderer);
                    _marchingCubeWholeLungregion.SetOpacity(0.06);
                    _marchingCubeWholeLungregion.SetColor(0 , 1 , 0);
                    if (refreshCamera)
                    {
                        _marchingCubeWholeLungregion.正面观();
                    }
                }

                if (File.Exists(_airwayPatient.GetLesionVTPFileName(0)))
                {
                    if (_marchingCubeWholeLesion != null)
                    {
                        _rendererPackage.Renderer.RemoveActor(_marchingCubeWholeLesion.Actor);
                        _marchingCubeWholeLesion.Dispose();
                        _marchingCubeWholeLesion = null;
                    }
                    _marchingCubeWholeLesion = new XmlPolyDataPackage(_airwayPatient.GetLesionVTPFileName(0), _rendererPackage.Renderer);
                    //_marchingCubeWholeLesion.正面观();
                    _marchingCubeWholeLesion.黄色();
                }

                _rendererPackage.FireModifiedEvent();
                //_marchingCubeWholeAirway.粉色();
            }
            else
            {
                //_marchingCubeConnectiveWholeAirway.ReplacePolyData(_connectiveAirwayVtpNewFilePath.ReadPolyData());
            }

            _rendererPackage.FireModifiedEvent();
            //_rendererPackage.StartRefreshAll();
            // _rendererPackage.轴位();
            if (refreshCamera)
            {
                _rendererPackage.ResetCameraAndRefreshAll();
            }
            _rendererPackage.RefreshAll();



        }

        public event EventHandler<PickEventArg> Picked;
        private DateTime lastClickTimeStamp = DateTime.MinValue;
        void _rendererPackage_Picked(object sender, PickEventArg e)
        {
            if ((DateTime.Now - lastClickTimeStamp).TotalMilliseconds < 300 || System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
            {
                if (_CTViewControl2 != null)
                {
                    _CTViewControl2.UpdateCamera(e.Position , e.Position);
                    _crossPackage.SetPosition(e.Position);
                    _rendererPackage.RefreshAll();
                }
            }
            lastClickTimeStamp = DateTime.Now;
        }


        public void SwitchAxial()
        {
            if (_CTViewControl2 != null)
            {
                _CTViewControl2.SetOritation(OrientationEnum.Axial);
                if (_marchingCubeWholeAirway != null)
                {
                    _marchingCubeWholeAirway.正面观();
                    _rendererPackage.FireModifiedEvent();
                    _rendererPackage.轴位();
                    _rendererPackage.ResetCameraAndRefreshAll();
                }
                _CTViewControl2.SetPictureBoxWidth();
            }

        }


        public void SwitchSagital()
        {
            if (_CTViewControl2 != null)
            {
                _CTViewControl2.SetOritation(OrientationEnum.Sagittal);
                if (_rendererPackage != null)
                {
                    _marchingCubeWholeAirway.正面观();
                    _rendererPackage.FireModifiedEvent();
                    _rendererPackage.矢状位();
                    _rendererPackage.ResetCameraAndRefreshAll();
                }
                //btnRefresh_Click(sender, e);
                _CTViewControl2.SetPictureBoxWidth();
            }
        }


        public void SwitchCoronal()
        {
            if (_CTViewControl2 != null)
            {
                _CTViewControl2.SetOritation(OrientationEnum.Coronal);
                if (_rendererPackage != null)
                {
                    _marchingCubeWholeAirway.正面观();
                    _rendererPackage.FireModifiedEvent();
                    _rendererPackage.冠状位();
                    _rendererPackage.ResetCameraAndRefreshAll();
                }
                //btnRefresh_Click(sender,e);
                _CTViewControl2.SetPictureBoxWidth();
            }
        }



        private void rbCoronal_Checked(object sender, RoutedEventArgs e)
        {
            SwitchCoronal();
        }

        private void rbSagital_Checked(object sender, RoutedEventArgs e)
        {
            SwitchSagital();
        }

        private void rbAxial_Checked(object sender, RoutedEventArgs e)
        {
            SwitchAxial();
        }

        void RenderWindowInteractor_ModifiedEvt(vtkObject sender, vtkObjectEventArgs e)
        {
        }

        private void videoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //mediaElement.Position = TimeSpan.FromSeconds(videoSlider.Value);
        }
       
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            MedSys.PresentationCore.AdjustWindow.ChangeWindowSize changeWindowSize = new MedSys.PresentationCore.AdjustWindow.ChangeWindowSize(this);
            changeWindowSize.RegisterHook();
        }
     
        public void PauseGUI()
        {
            ((UIElement)Content).IsEnabled = false;
            Cursor = Cursors.Wait;
        }



        public void ResumeGUI()
        {
            ((UIElement)Content).IsEnabled = true;
            Cursor = Cursors.Arrow;
        }
     
        public static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent is valid.  
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child 
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree 
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.  
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search 
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name 
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found. 
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }
    }
    
   
}
using LungCare.SupportPlatform.Entities;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LungCare.SupportPlatform.UI.UserControls.Examination
{
    /// <summary>
    /// SnapshotUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class SnapshotUserControl : UserControl
    {
        public string id;
        public SnapshotUserControl()
        {
            InitializeComponent();
            btnSelectImage.MouseDown += btnSelectImage_MouseDown;

            id = Guid.NewGuid().ToString();
            //image.MouseDown += btnSelectImage_MouseDown;
            
            this.btnClose = new RibbonStyle.RibbonMenuButton();
            this.btnClose.AllowDrop = true;
            this.btnClose.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.保存;
            this.btnClose.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnClose.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnClose.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnClose.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnClose.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnClose.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnClose.FadingSpeed = 20;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnClose.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnClose.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnClose.Image = global::LungCare.SupportPlatform.Properties.Resources.close;
            this.btnClose.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnClose.ImageOffset = 5;
            this.btnClose.IsPressed = false;
            this.btnClose.KeepPress = false;
            this.btnClose.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnClose.MenuPos = new System.Drawing.Point(0, 0);
            this.btnClose.Name = "btnSaveLesionResult";
            this.btnClose.Radius = 8;
            this.btnClose.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnClose.Size = new System.Drawing.Size(50, 50);
            this.btnClose.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnClose.SplitDistance = 0;
            this.btnClose.TabIndex = 101;
            this.btnClose.Title = "";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += btnClose_Click;

        }

        void btnClose_Click(object sender, EventArgs e)
        {
            //image.Visibility = System.Windows.Visibility.Hidden;
            //mainGrid.RowDefinitions[0].Height = new GridLength(1.0, GridUnitType.Star);
            //mainGrid.RowDefinitions[1].Height = new GridLength(0 , GridUnitType.Star);
            
            //throw new NotImplementedException();
        }

        private RibbonStyle.RibbonMenuButton btnClose;
        public event EventHandler<EventArgs> MouseButtonClick;
        public event EventHandler<MouseButtonEventArgs> DeleteClick;
        public event EventHandler<DicomSnapshotEventArgs> ImageClick;
        void btnSelectImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MouseButtonClick != null)
            {
                MouseButtonClick(this , new EventArgs());
            }

            return;
            
            
            //Window parentWindow = Window.GetWindow((DependencyObject)sender);
            //if (parentWindow != null)
            //{
            //    //parentWindow.Close();//关闭了SnapshotWindow界面
            //}
            Console.WriteLine("btnSelectImage_MouseDown");
            image.Source = new BitmapImage(new Uri(@"H:\Project\net\projectE\LungcareE\professional\code\MA\LungCare.Airway.PlanNav\Resources\lungcare.ico"));
            image.Visibility = System.Windows.Visibility.Visible;
            close.Visibility = System.Windows.Visibility.Visible;
            //mainGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
            //mainGrid.RowDefinitions[1].Height = new GridLength(1.0, GridUnitType.Star);
            
            //throw new NotImplementedException();
        }


        public void SetImage(Bitmap bitmap)
        {
            //Bitmap bitmap = MainWindowMA.Instance.GetCTBitmap();
            Console.WriteLine("btnSelectImage_MouseDown");
            if (bitmap != null)
            {
                image.Source = ToBitmapSource(bitmap);
                image.MouseDown += image_MouseDown;
                image.Visibility = System.Windows.Visibility.Visible;
                //close.Visibility = System.Windows.Visibility.Visible;
            }
            
        }

        public string _studyUID, _seriesUID ;
        public int _windowWidth, _windowsLevel, _index;
        public Bitmap Bitmap;
        public System.Drawing.Drawing2D.Matrix _matrix = new System.Drawing.Drawing2D.Matrix();
        public void SetImage(Bitmap bitmap , string studyUID,string seriesUID , int windowWidth ,
            int windowsLevel, int index, System.Drawing.Drawing2D.Matrix matrix)
        {
            Bitmap = bitmap;
            _studyUID = studyUID;
            _seriesUID = seriesUID;
            _windowWidth = windowWidth;
            _windowsLevel = windowsLevel;
            _index = index;
            _matrix = new System.Drawing.Drawing2D.Matrix();
            if (matrix != null)
            {
                _matrix = matrix.Clone();
                //for (int i = 0; i < matrix.Elements.Length; i++)
                //{
                //    _matrix.Elements[i] = matrix.Elements[i];
                //}

               
            }
            //Bitmap bitmap = MainWindowMA.Instance.GetCTBitmap();
            Console.WriteLine("btnSelectImage_MouseDown");
            if (bitmap != null)
            {
                image.Source = ToBitmapSource(bitmap);
                image.MouseLeftButtonDown += image_MouseDown;
                image.Visibility = System.Windows.Visibility.Visible;
                //close.Visibility = System.Windows.Visibility.Visible;
            }

        }

        void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("image_MouseDown");

            if (ImageClick != null)
            {
                ImageClick(this, new DicomSnapshotEventArgs()
                {
                    studyUID = _studyUID,
                    seriesUID = _seriesUID,
                    WindowWidth = _windowWidth,
                    WindowLevel =_windowsLevel,
                    Index = _index,
                    Matrix = _matrix
                });
            }
            //throw new NotImplementedException();
        }

        private BitmapSource ToBitmapSource(System.Drawing.Bitmap source)
        {
            using (var handle = new SafeHBitmapHandle(source))
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle.DangerousGetHandle(),
                    IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private sealed class SafeHBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [SecurityCritical]
            public SafeHBitmapHandle(Bitmap bitmap)
                : base(true)
            {
                SetHandle(bitmap.GetHbitmap());
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            protected override bool ReleaseHandle()
            {
                return DeleteObject(handle) > 0;
            }
        }
        private void close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            image.Visibility = System.Windows.Visibility.Hidden;
            //close.Visibility = System.Windows.Visibility.Hidden;
            if (DeleteClick != null)
            {
                DeleteClick(this, e);
            }
            
        }

        private void mainGrid_MouseMove(object sender, MouseEventArgs e)
        {
            close.Visibility = System.Windows.Visibility.Visible;
        }

        private void mainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            close.Visibility = System.Windows.Visibility.Hidden;
        }


    }


}

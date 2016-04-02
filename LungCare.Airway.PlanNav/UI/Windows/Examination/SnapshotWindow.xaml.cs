using AirwayCT.Entity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.SupportPlatformDAO.Images;
namespace LungCare.SupportPlatform.UI.Windows.Examination
{
    /// <summary>
    /// MsgWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SnapshotWindow : Window
    {
        public SnapshotWindow()
        {
            InitializeComponent();

            axialAll.MouseButtonClick += axialAll_MouseButtonClick;
            axialPart.MouseButtonClick += axialPart_MouseButtonClick;
            axial3D.MouseButtonClick += axial3D_MouseButtonClick;

            coronalAll.MouseButtonClick += coronalAll_MouseButtonClick;
            coronalPart.MouseButtonClick += coronalPart_MouseButtonClick;
            coronal3D.MouseButtonClick += coronal3D_MouseButtonClick;

            sagitalAll.MouseButtonClick += sagitalAll_MouseButtonClick;
            sagitalPart.MouseButtonClick += sagitalPart_MouseButtonClick;
            sagital3D.MouseButtonClick += sagital3D_MouseButtonClick;
        }

        private AirwayPatient _SelectedPatient;
        private string _segmentDirectory;
        private LesionEntity _lesionEntity;
        public SnapshotWindow(AirwayPatient SelectedPatient)
        {
            InitializeComponent();

            _SelectedPatient = SelectedPatient;
            _segmentDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_SelectedPatient.AirwayVTP_FileName) , "Snapshot");
            if (!Directory.Exists(_segmentDirectory))
            {
                Directory.CreateDirectory(_segmentDirectory);
            }

            axialAll.MouseButtonClick += axialAll_MouseButtonClick;
            axialPart.MouseButtonClick += axialPart_MouseButtonClick;
            axial3D.MouseButtonClick += axial3D_MouseButtonClick;

            coronalAll.MouseButtonClick += coronalAll_MouseButtonClick;
            coronalPart.MouseButtonClick += coronalPart_MouseButtonClick;
            coronal3D.MouseButtonClick += coronal3D_MouseButtonClick;

            sagitalAll.MouseButtonClick += sagitalAll_MouseButtonClick;
            sagitalPart.MouseButtonClick += sagitalPart_MouseButtonClick;
            sagital3D.MouseButtonClick += sagital3D_MouseButtonClick;

            _lesionEntity = new LesionEntity();
        }

        private double[] Position;
        public SnapshotWindow(AirwayPatient SelectedPatient , int index ,LesionEntity lesion)
        {
            InitializeComponent();
            _lesionEntity = lesion;
            Position = lesion.position;
            _SelectedPatient = SelectedPatient;
            _segmentDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_SelectedPatient.AirwayVTP_FileName), "Snapshot" , lesion.Index.ToString());
            
            if (!Directory.Exists(_segmentDirectory))
            {
                Directory.CreateDirectory(_segmentDirectory);
            }

            axialAll.MouseButtonClick += axialAll_MouseButtonClick;
            axialPart.MouseButtonClick += axialPart_MouseButtonClick;
            axial3D.MouseButtonClick += axial3D_MouseButtonClick;

            coronalAll.MouseButtonClick += coronalAll_MouseButtonClick;
            coronalPart.MouseButtonClick += coronalPart_MouseButtonClick;
            coronal3D.MouseButtonClick += coronal3D_MouseButtonClick;

            sagitalAll.MouseButtonClick += sagitalAll_MouseButtonClick;
            sagitalPart.MouseButtonClick += sagitalPart_MouseButtonClick;
            sagital3D.MouseButtonClick += sagital3D_MouseButtonClick;


            LoadLesionSnapshot();

        }


        private void  LoadLesionSnapshot()
        {
            if (_lesionEntity != null)
            {
                axial3DBitmap = ImageOperation.ReadImage(_lesionEntity.Axial3DImageFile);
                if(axial3DBitmap!=null)
                {
                    axial3D.SetImage(axial3DBitmap);
                }
                axialAllBitmap = ImageOperation.ReadImage(_lesionEntity.AxialCTImageFile);
                if (axialAllBitmap != null)
                {
                    axialAll.SetImage(axialAllBitmap);
                }
                axialPartBitmap = ImageOperation.ReadImage(_lesionEntity.AxialCTDetailImageFile);
                if (axialPartBitmap != null)
                {
                    axialPart.SetImage(axialPartBitmap);
                }



                sagital3DBitmap = ImageOperation.ReadImage(_lesionEntity.Sagital3DImageFile);
                if (sagital3DBitmap != null)
                {
                    sagital3D.SetImage(sagital3DBitmap);
                }
                sagitalAllBitmap = ImageOperation.ReadImage(_lesionEntity.SagitalCTImageFile);
                if (sagitalAllBitmap != null)
                {
                    sagitalAll.SetImage(sagitalAllBitmap);
                }
                sagitalPartBitmap = ImageOperation.ReadImage(_lesionEntity.SagitalCTDetailImageFile);
                if (sagitalPartBitmap != null)
                {
                    sagitalPart.SetImage(sagitalPartBitmap);
                }



                coronal3DBitmap = ImageOperation.ReadImage(_lesionEntity.Coronal3DImageFile);
                if (coronal3DBitmap != null)
                {
                    coronal3D.SetImage(coronal3DBitmap);
                }
                coronalAllBitmap = ImageOperation.ReadImage(_lesionEntity.CoronalCTImageFile);
                if (coronalAllBitmap != null)
                {
                    coronalAll.SetImage(coronalAllBitmap);
                }
                coronalPartBitmap = ImageOperation.ReadImage(_lesionEntity.CoronalCTDetailImageFile);
                if (coronalPartBitmap != null)
                {
                    coronalPart.SetImage(coronalPartBitmap);
                }
            }
        }
        private Bitmap sagitalAllBitmap, sagitalPartBitmap, sagital3DBitmap;
        void sagital3D_MouseButtonClick(object sender, EventArgs e)
        {
            //保存到对应分割目录下
            _lesionEntity.Sagital3DImageFile = System.IO.Path.Combine(_segmentDirectory, "Sagital", "Sagital3DImage.jpg");
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(_lesionEntity.Sagital3DImageFile)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lesionEntity.Sagital3DImageFile));
            }
            sagital3DBitmap = MainWindowMA.Instance.GetAirway3DBitmap(_lesionEntity.Sagital3DImageFile);
            sagital3D.SetImage(sagital3DBitmap);
            //throw new NotImplementedException();
        }

        void sagitalPart_MouseButtonClick(object sender, EventArgs e)
        {
            _lesionEntity.SagitalCTDetailImageFile = System.IO.Path.Combine(_segmentDirectory, "Sagital", "SagitalCTDetail.jpg");
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(_lesionEntity.SagitalCTDetailImageFile)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lesionEntity.SagitalCTDetailImageFile));
            }

            sagitalPartBitmap = MainWindowMA.Instance.GetCTBitmap(_lesionEntity.SagitalCTDetailImageFile);
            sagitalPart.SetImage(sagitalPartBitmap);
            //sagitalPartBitmap.Save(_lesionEntity.SagitalCTDetailImageFile);
            //throw new NotImplementedException();
        }

        void sagitalAll_MouseButtonClick(object sender, EventArgs e)
        {
            
            _lesionEntity.SagitalCTImageFile = System.IO.Path.Combine(_segmentDirectory, "Sagital", "SagitalCTImage.jpg");
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(_lesionEntity.SagitalCTImageFile)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lesionEntity.SagitalCTImageFile));
            }
            sagitalAllBitmap = MainWindowMA.Instance.GetCTBitmap(_lesionEntity.SagitalCTImageFile);
            sagitalAll.SetImage(sagitalAllBitmap);

            //sagitalAllBitmap.Save(_lesionEntity.SagitalCTImageFile);
            //throw new NotImplementedException();
        }

        private Bitmap coronalAllBitmap, coronalPartBitmap, coronal3DBitmap;
        void coronal3D_MouseButtonClick(object sender, EventArgs e)
        {
            _lesionEntity.Coronal3DImageFile = System.IO.Path.Combine(_segmentDirectory, "Coronal", "Coronal3DImage.jpg");
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(_lesionEntity.Coronal3DImageFile)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lesionEntity.Coronal3DImageFile));
            }
            //coronal3DBitmap.Save(_lesionEntity.Coronal3DImageFile);

            coronal3DBitmap = MainWindowMA.Instance.GetAirway3DBitmap(_lesionEntity.Coronal3DImageFile);
            coronal3D.SetImage(coronal3DBitmap);


            
            //throw new NotImplementedException();
        }

        void coronalPart_MouseButtonClick(object sender, EventArgs e)
        {
            
            _lesionEntity.CoronalCTDetailImageFile = System.IO.Path.Combine(_segmentDirectory, "Coronal", "CoronalCTDetailImage.jpg");
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(_lesionEntity.CoronalCTDetailImageFile)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lesionEntity.CoronalCTDetailImageFile));
            }
            //coronalPartBitmap.Save(_lesionEntity.CoronalCTDetailImageFile);
            coronalPartBitmap = MainWindowMA.Instance.GetCTBitmap(_lesionEntity.CoronalCTDetailImageFile);
            coronalPart.SetImage(coronalPartBitmap);

            //throw new NotImplementedException();
        }

        void coronalAll_MouseButtonClick(object sender, EventArgs e)
        {
            
            _lesionEntity.CoronalCTImageFile = System.IO.Path.Combine(_segmentDirectory, "Coronal", "CoronalCTImage.jpg");
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(_lesionEntity.CoronalCTImageFile)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lesionEntity.CoronalCTImageFile));
            }
            //coronalAllBitmap.Save(_lesionEntity.CoronalCTImageFile);
            coronalAllBitmap = MainWindowMA.Instance.GetCTBitmap(_lesionEntity.CoronalCTImageFile);
            coronalAll.SetImage(coronalAllBitmap);

            //throw new NotImplementedException();
        }


        private Bitmap axialAllBitmap, axialPartBitmap, axial3DBitmap;
        void axial3D_MouseButtonClick(object sender, EventArgs e)
        {
            

            _lesionEntity.Axial3DImageFile = System.IO.Path.Combine(_segmentDirectory, "Axial", "Axial3DImage.jpg");
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(_lesionEntity.Axial3DImageFile)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lesionEntity.Axial3DImageFile));
            }
            //axial3DBitmap.Save(_lesionEntity.Axial3DImageFile);
            axial3DBitmap = MainWindowMA.Instance.GetAirway3DBitmap(_lesionEntity.Axial3DImageFile);
            axial3D.SetImage(axial3DBitmap);
            //throw new NotImplementedException();
        }

        void axialPart_MouseButtonClick(object sender, EventArgs e)
        {
            

            _lesionEntity.AxialCTDetailImageFile = System.IO.Path.Combine(_segmentDirectory, "Axial", "AxialCTDetailImage.jpg");
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(_lesionEntity.AxialCTDetailImageFile)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lesionEntity.AxialCTDetailImageFile));
            }
            //axialPartBitmap.Save(_lesionEntity.AxialCTDetailImageFile);
            axialPartBitmap = MainWindowMA.Instance.GetCTBitmap(_lesionEntity.AxialCTDetailImageFile);
            axialPart.SetImage(axialPartBitmap);

            //throw new NotImplementedException();
        }

        void axialAll_MouseButtonClick(object sender, EventArgs e)
        {
            

            _lesionEntity.AxialCTImageFile = System.IO.Path.Combine(_segmentDirectory, "Axial", "AxialCTImage.jpg");
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(_lesionEntity.AxialCTImageFile)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lesionEntity.AxialCTImageFile));
            }
            //axialAllBitmap.Save(_lesionEntity.AxialCTImageFile);
            axialAllBitmap = MainWindowMA.Instance.GetCTBitmap(_lesionEntity.AxialCTImageFile);
            axialAll.SetImage(axialAllBitmap);
            //throw new NotImplementedException();
        }
        
        private RibbonStyle.RibbonMenuButton btnClose;

        private void InitButton()
        {
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
            if (MessageBox.Show("是否退出？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MainWindowMA.Instance.SwithOld();
                this.Close();
            }
         
            //throw new NotImplementedException();
        }
        public string MsgText { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitButton();
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("是否退出？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MainWindowMA.Instance.SwithOld();
                this.Close();
            }
        
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void borderMessage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {
                
            }

        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("是否退出？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MainWindowMA.Instance.SwithOld();
                this.Close();
            }
        }

        private void rbAxial_Checked(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 0;
            MainWindowMA.Instance.SwitchAxial();
        }

        private void rbCoronal_Checked(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 1;
            MainWindowMA.Instance.SwitchCoronal();
        }

        private void rbSagital_Checked(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 2;
            MainWindowMA.Instance.SwitchSagital();
        }

        private void btnSaveResult_Click(object sender, RoutedEventArgs e)
        {
            if (_lesionEntity != null)
            {
                try
                {
                    MainWindowMA.Instance.AddLesion(_lesionEntity);
                    MessageBox.Show("保存成功！");    
                }
                catch (Exception)
                {
                    MessageBox.Show("保存失败！");    
                    
                }
               
                
            }
            //MainWindowMA.Instance.SetSnapshotImages( axialAllBitmap, axialPartBitmap, axial3DBitmap,
            // sagitalAllBitmap, sagitalPartBitmap, sagital3DBitmap,
            // coronalAllBitmap, coronalPartBitmap, coronal3DBitmap);
        }
    }
}

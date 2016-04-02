using AirwayCT.Entity;
using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.UI.UserControls.Examination;
using System;
using System.Collections.Generic;
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

namespace LungCare.SupportPlatform.UI.Windows.Examination
{
    /// <summary>
    /// MsgWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AllLesionWindow : Window
    {
        public AllLesionWindow()
        {
            InitializeComponent();
            
            
        }

        private string BaseFolder = "";
        private LesionEntities listLesion;
        public AllLesionWindow(string baseFolder, AirwayPatient airwayPatient)
        {
            InitializeComponent();
            BaseFolder = baseFolder;
            AirwayPatient = airwayPatient;
            listLesion = LesionEntities.TestLoad(BaseFolder);
            if (listLesion != null)
            {
                for (int i = 0; i < listLesion.Count; i++)
                {
                    AddNewLesion(listLesion[i]);
                }
            }
        }

        public AirwayPatient AirwayPatient;
        private RibbonStyle.RibbonMenuButton btnAddLesion;
        private RibbonStyle.RibbonMenuButton btnDeleteLesion;
        private RibbonStyle.RibbonMenuButton btnClose;

        private void InitButton()
        {
            this.btnAddLesion = new RibbonStyle.RibbonMenuButton();
            this.btnDeleteLesion = new RibbonStyle.RibbonMenuButton();
            this.btnClose = new RibbonStyle.RibbonMenuButton();

            this.btnAddLesion.AllowDrop = true;
            this.btnAddLesion.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnAddLesion.BackColor = System.Drawing.Color.Transparent;
            this.btnAddLesion.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnAddLesion.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnAddLesion.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnAddLesion.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnAddLesion.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnAddLesion.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnAddLesion.FadingSpeed = 20;
            this.btnAddLesion.FlatAppearance.BorderSize = 0;
            this.btnAddLesion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddLesion.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnAddLesion.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnAddLesion.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnAddLesion.Image = global::LungCare.SupportPlatform.Properties.Resources.加号;
            this.btnAddLesion.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnAddLesion.ImageOffset = 5;
            this.btnAddLesion.IsPressed = false;
            this.btnAddLesion.KeepPress = false;
            // this.widthAndLevel.Location = new System.Drawing.Point(29, 256);
            this.btnAddLesion.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnAddLesion.MenuPos = new System.Drawing.Point(0, 0);
            this.btnAddLesion.Name = "widthAndLevel";
            this.btnAddLesion.Radius = 8;
            this.btnAddLesion.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnAddLesion.Size = new System.Drawing.Size(50, 50);
            this.btnAddLesion.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnAddLesion.SplitDistance = 0;
            this.btnAddLesion.TabIndex = 102;
            this.btnAddLesion.Title = "";
            this.btnAddLesion.UseVisualStyleBackColor = true;
            this.btnAddLesion.Visible = true;
            // 
            // btnReturn
            // 
            this.btnDeleteLesion.AllowDrop = true;
            this.btnDeleteLesion.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnDeleteLesion.BackColor = System.Drawing.Color.Transparent;
            this.btnDeleteLesion.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnDeleteLesion.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnDeleteLesion.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnDeleteLesion.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnDeleteLesion.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnDeleteLesion.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnDeleteLesion.FadingSpeed = 20;
            this.btnDeleteLesion.FlatAppearance.BorderSize = 0;
            this.btnDeleteLesion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteLesion.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnDeleteLesion.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnDeleteLesion.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnDeleteLesion.Image = global::LungCare.SupportPlatform.Properties.Resources.未完成;
            this.btnDeleteLesion.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnDeleteLesion.ImageOffset = 5;
            this.btnDeleteLesion.IsPressed = false;
            this.btnDeleteLesion.KeepPress = false;
            this.btnDeleteLesion.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnDeleteLesion.MenuPos = new System.Drawing.Point(0, 0);
            this.btnDeleteLesion.Name = "btnReturn";
            this.btnDeleteLesion.Radius = 8;
            this.btnDeleteLesion.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnDeleteLesion.Size = new System.Drawing.Size(50, 50);
            this.btnDeleteLesion.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnDeleteLesion.SplitDistance = 0;
            this.btnDeleteLesion.TabIndex = 94;
            this.btnDeleteLesion.Title = "";
            this.btnDeleteLesion.UseVisualStyleBackColor = true;

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

            panel1.Controls.Add(btnAddLesion);
            panel1.Controls.Add(btnDeleteLesion);
            panel2.Controls.Add(btnClose);

            btnAddLesion.Click += btnAddLesion_Click;
            btnDeleteLesion.Click += btnDeleteLesion_Click;
            System.Windows.Forms.FlowLayoutPanel panel3 = new System.Windows.Forms.FlowLayoutPanel();
            wfClose.Child = panel3;
            panel3.Controls.Add(btnClose);
            panel2.BackColor = System.Drawing.Color.Red;

            //for (int i = 0; i < 5; i++)
            //{
            //    AddNewLesion();
            //}
            listLesionControls = new List<LesionButtonUserControl>();
           
        }

        private void AddNewLesion()
        {
            LesionButtonUserControl lesion1 = new LesionButtonUserControl(_maxIndex+ 1);
            lesion1.StartSnapshotEvent += lesion1_StartSnapshotEvent;
            bb.Children.Add(lesion1);
            ++_maxIndex;
        }

        //private void AddNewLesion(double[] position)
        //{
        //    LesionButtonUserControl lesion1 = new LesionButtonUserControl(_maxIndex + 1 , position);
        //    lesion1.AirwayPatient = AirwayPatient;
        //    lesion1.StartSnapshotEvent += lesion1_StartSnapshotEvent;
        //    bb.Children.Add(lesion1);
        //    ++_maxIndex;
        //}

        private void AddNewLesion(LesionEntity lesion)
        {
            LesionButtonUserControl lesion1 = new LesionButtonUserControl(_maxIndex + 1, lesion);
            lesion1.AirwayPatient = AirwayPatient;
            lesion1.StartSnapshotEvent += lesion1_StartSnapshotEvent;
            bb.Children.Add(lesion1);
            ++_maxIndex;
        }


        void lesion1_StartSnapshotEvent(object sender, EventArgs e)
        {
            //MainWindowMA.Instance.SwitchSnap();
            this.Topmost = false;
            this.Close();
            //throw new NotImplementedException();
        }
        private int _maxIndex = 0;
        private List<LesionButtonUserControl> listLesionControls;
        void btnDeleteLesion_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否删除？", "提示", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                listLesionControls = new List<LesionButtonUserControl>();
                List<LesionButtonUserControl> listDelete = new List<LesionButtonUserControl>();
                foreach (UIElement item in bb.Children)
                {
                    LesionButtonUserControl a = (LesionButtonUserControl)item;
                    if (!a.isSelect)
                    {
                        listLesionControls.Add(a);
                    }else
                    {
                        listDelete.Add(a);
                    }
                }

                bb.Children.Clear();

                foreach (var item in listLesionControls)
                {
                    bb.Children.Add(item);
                }

                foreach (var item in listDelete)
                {
                    LesionEntities.Delete(BaseFolder , item._lesion);    
                }
                

            }
            //throw new NotImplementedException();
        }

        void btnAddLesion_Click(object sender, EventArgs e)
        {
            MainWindowMA main = this.Owner as MainWindowMA;

            LesionEntity lesion = new LesionEntity();
            lesion.position = main._position;
            lesion.Index = _maxIndex+1;
            AddNewLesion(lesion);


            //throw new NotImplementedException();
        }

        void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
            //throw new NotImplementedException();
        }
        public string MsgText { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            label.Content = "病灶管理";
            InitButton();
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
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
            this.DragMove();
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            
        }
    }
}

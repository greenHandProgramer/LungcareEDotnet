using System;
using System.Windows;
using System.Windows.Forms;
using Dicom;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using Dicom.Imaging;
using LungCare.SupportPlatform.Models;
using System.Diagnostics;
using System.Threading;
using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.UI.UserControls.Examination;
using LungCare.SupportPlatform.UI.Windows.Examination;
using LungCare.SupportPlatform.SupportPlatformDAO.Utils;
using LungCare.SupportPlatform.SupportPlatformDAO.Images;
using LungCare.SupportPlatform.Entities;
using AirwayCT.Entity;
using LungCare.SupportPlatform.SupportPlatformDAO.Files;
using LungCare.SupportPlatform.SupportPlatformDAO.Logs;
using LungCare.SupportPlatform.UI.Windows.Service;

namespace LungCare.SupportPlatform.UI.Windows.Common
{
    /// <summary>
    /// ImportWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImportDicomWindow3 : Window
    {


        public ImportDicomWindow3()
        {
            InitializeComponent();
            LoadComponent();

        }

        public ImportDicomWindow3(string dicomPath)
        {
            InitializeComponent();
            LoadComponent();
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            InitParameters();
            //GetDicomFileCount(selectPath);

            lbProgress.Visibility = System.Windows.Visibility.Visible;
            LoadSelectPathDicoms(dicomPath);

          
            FilterInValidDicom();
            dataGridCT.SelectedIndex = 0;
            SelectFirstSeries();
            lbProgress.Visibility = System.Windows.Visibility.Hidden;


            LoadStudySnapshotUserControlsFromLocal();
            //string[] files = Directory.GetFiles(dicomPath);
            //foreach (var item in files)
            //{
            //    if (Path.GetExtension(item) == ".jpg")
            //    {
            //        SnapshotUserControl snapshotUserControl = new SnapshotUserControl();
            //        snapshotUserControl.Margin = new Thickness(5);
            //        snapshotUserControl.DeleteClick += snapshotUserControl_DeleteClick;
            //        snapshotUserControl.ImageClick += snapshotUserControl_ImageClick;
            //        snapshotUserControl.Width = 180;
            //        snapshotUserControl.Height = 180;
            //        snapshotUserControl.SetImage(new Bitmap(item));

            //        panelLesionThumb.Children.Add(snapshotUserControl);

            //        listSnapshotUserControls.Add(snapshotUserControl);

            //    }
            //}
        }


        #region 界面上的按钮
        private RibbonStyle.RibbonMenuButton btnImportDicom;
        private RibbonStyle.RibbonMenuButton btnSaveDicomToLocal;
        private RibbonStyle.RibbonMenuButton btnSaveDicomToNetwork;
        private RibbonStyle.RibbonMenuButton btnExit;
        private RibbonStyle.RibbonMenuButton btnWidthAndLevel;

        private RibbonStyle.RibbonMenuButton btnPanning;
        private RibbonStyle.RibbonMenuButton btnRefresh;
        private RibbonStyle.RibbonMenuButton btnZoomOut;
        private RibbonStyle.RibbonMenuButton btnAddLesion;
        private RibbonStyle.RibbonMenuButton btnSelectLesionRegion;
        private RibbonStyle.RibbonMenuButton btnMinisize;

        private void LoadComponent() 
        {
            //if (SystemParameters.WorkArea.Width < this.Width)
            {
                this.Width = SystemParameters.WorkArea.Width - 100;
            }

            //if (SystemParameters.WorkArea.Height < this.Height)
            {
                this.Height = SystemParameters.WorkArea.Height - 20;
            }

          

            //1266  738
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;// 获得窗体的 样式

            int oldstyle = MedSys.PresentationCore.AdjustWindow.NativeMethods.GetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE, oldstyle & ~MedSys.PresentationCore.AdjustWindow.NativeMethods.WS_CAPTION);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetLayeredWindowAttributes(hwnd, 1 | 2 << 8 | 3 << 16, 0, MedSys.PresentationCore.AdjustWindow.NativeMethods.LWA_ALPHA);

            this.RefreshRoundWindowRect();

            this.SizeChanged += ImportDicomWindow3_SizeChanged;
            panelDicomThumb.MouseDown += panelDicomThumb_MouseDown;


            btnImportDicom = new RibbonStyle.RibbonMenuButton();
            btnSaveDicomToLocal = new RibbonStyle.RibbonMenuButton();
            btnExit = new RibbonStyle.RibbonMenuButton();
            btnWidthAndLevel = new RibbonStyle.RibbonMenuButton();
            btnSaveDicomToNetwork = new RibbonStyle.RibbonMenuButton();
            this.btnPanning = new RibbonStyle.RibbonMenuButton();
            this.btnZoomOut = new RibbonStyle.RibbonMenuButton();
            this.btnRefresh = new RibbonStyle.RibbonMenuButton();
            this.btnAddLesion = new RibbonStyle.RibbonMenuButton();
            this.btnMinisize = new RibbonStyle.RibbonMenuButton();
            this.btnSelectLesionRegion = new RibbonStyle.RibbonMenuButton();
            // 
            // btnImportCT
            // 
            this.btnImportDicom.AllowDrop = true;
            this.btnImportDicom.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnImportDicom.BackColor = System.Drawing.Color.Transparent;
            //this.btnImportCT.BackgroundImage = global::LungCare.Airway.WinformUIControls.Properties.Resources.支气管加点;
            this.btnImportDicom.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnImportDicom.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnImportDicom.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnImportDicom.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnImportDicom.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnImportDicom.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnImportDicom.FadingSpeed = 20;
            this.btnImportDicom.FlatAppearance.BorderSize = 0;
            this.btnImportDicom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImportDicom.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnImportDicom.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnImportDicom.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnImportDicom.Image = global::LungCare.SupportPlatform.Properties.Resources.open;
            this.btnImportDicom.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnImportDicom.ImageOffset = 5;
            this.btnImportDicom.IsPressed = false;
            this.btnImportDicom.KeepPress = false;
            this.btnImportDicom.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnImportDicom.MenuPos = new System.Drawing.Point(0, 0);
            this.btnImportDicom.Name = "btnAddPoint";
            this.btnImportDicom.Radius = 8;
            this.btnImportDicom.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnImportDicom.Size = new System.Drawing.Size(50, 50);
            this.btnImportDicom.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnImportDicom.SplitDistance = 0;
            this.btnImportDicom.TabIndex = 96;
            this.btnImportDicom.Title = "";
            this.btnImportDicom.UseVisualStyleBackColor = true;
            // 
            // btnSaveDicomToLocal
            // 
            this.btnSaveDicomToLocal.AllowDrop = true;
            this.btnSaveDicomToLocal.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnSaveDicomToLocal.BackColor = System.Drawing.Color.Transparent;
            //this.btnSaveDicomToLocal.BackgroundImage = global::LungCare.Airway.WinformUIControls.Properties.Resources.确认肿瘤位置;
            this.btnSaveDicomToLocal.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnSaveDicomToLocal.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnSaveDicomToLocal.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnSaveDicomToLocal.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnSaveDicomToLocal.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnSaveDicomToLocal.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnSaveDicomToLocal.FadingSpeed = 20;
            this.btnSaveDicomToLocal.FlatAppearance.BorderSize = 0;
            this.btnSaveDicomToLocal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveDicomToLocal.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnSaveDicomToLocal.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnSaveDicomToLocal.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnSaveDicomToLocal.Image = global::LungCare.SupportPlatform.Properties.Resources.save;
            this.btnSaveDicomToLocal.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnSaveDicomToLocal.ImageOffset = 5;
            this.btnSaveDicomToLocal.IsPressed = false;
            this.btnSaveDicomToLocal.KeepPress = false;
            this.btnSaveDicomToLocal.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnSaveDicomToLocal.MenuPos = new System.Drawing.Point(0, 0);
            this.btnSaveDicomToLocal.Name = "btnDrawLesion";
            this.btnSaveDicomToLocal.Radius = 8;
            this.btnSaveDicomToLocal.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnSaveDicomToLocal.Size = new System.Drawing.Size(50, 50);
            this.btnSaveDicomToLocal.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnSaveDicomToLocal.SplitDistance = 0;
            this.btnSaveDicomToLocal.TabIndex = 99;
            this.btnSaveDicomToLocal.Title = "";
            this.btnSaveDicomToLocal.UseVisualStyleBackColor = true;

            this.btnSaveDicomToNetwork.AllowDrop = true;
            this.btnSaveDicomToNetwork.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnSaveDicomToNetwork.BackColor = System.Drawing.Color.Transparent;
            this.btnSaveDicomToNetwork.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnSaveDicomToNetwork.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnSaveDicomToNetwork.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnSaveDicomToNetwork.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnSaveDicomToNetwork.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnSaveDicomToNetwork.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnSaveDicomToNetwork.FadingSpeed = 20;
            this.btnSaveDicomToNetwork.FlatAppearance.BorderSize = 0;
            this.btnSaveDicomToNetwork.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveDicomToNetwork.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnSaveDicomToNetwork.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnSaveDicomToNetwork.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnSaveDicomToNetwork.Image = global::LungCare.SupportPlatform.Properties.Resources.save;
            this.btnSaveDicomToNetwork.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnSaveDicomToNetwork.ImageOffset = 5;
            this.btnSaveDicomToNetwork.IsPressed = false;
            this.btnSaveDicomToNetwork.KeepPress = false;
            this.btnSaveDicomToNetwork.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnSaveDicomToNetwork.MenuPos = new System.Drawing.Point(0, 0);
            this.btnSaveDicomToNetwork.Name = "btnDrawLesion";
            this.btnSaveDicomToNetwork.Radius = 8;
            this.btnSaveDicomToNetwork.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnSaveDicomToNetwork.Size = new System.Drawing.Size(50, 50);
            this.btnSaveDicomToNetwork.AllowDrop = true;
            this.btnSaveDicomToNetwork.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnSaveDicomToNetwork.BackColor = System.Drawing.Color.Transparent;
            this.btnSaveDicomToNetwork.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnSaveDicomToNetwork.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnSaveDicomToNetwork.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnSaveDicomToNetwork.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnSaveDicomToNetwork.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnSaveDicomToNetwork.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnSaveDicomToNetwork.FadingSpeed = 20;
            this.btnSaveDicomToNetwork.FlatAppearance.BorderSize = 0;
            this.btnSaveDicomToNetwork.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveDicomToNetwork.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnSaveDicomToNetwork.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnSaveDicomToNetwork.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnSaveDicomToNetwork.Image = global::LungCare.SupportPlatform.Properties.Resources.upload;
            this.btnSaveDicomToNetwork.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnSaveDicomToNetwork.ImageOffset = 5;
            this.btnSaveDicomToNetwork.IsPressed = false;
            this.btnSaveDicomToNetwork.KeepPress = false;
            this.btnSaveDicomToNetwork.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnSaveDicomToNetwork.MenuPos = new System.Drawing.Point(0, 0);
            this.btnSaveDicomToNetwork.Name = "btnDrawLesion";
            this.btnSaveDicomToNetwork.Radius = 8;
            this.btnSaveDicomToNetwork.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnSaveDicomToNetwork.Size = new System.Drawing.Size(50, 50);
            this.btnSaveDicomToNetwork.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnSaveDicomToNetwork.SplitDistance = 0;
            this.btnSaveDicomToNetwork.TabIndex = 99;
            this.btnSaveDicomToNetwork.Title = "";
            this.btnSaveDicomToNetwork.UseVisualStyleBackColor = true;
            this.btnSaveDicomToNetwork.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnSaveDicomToNetwork.SplitDistance = 0;
            this.btnSaveDicomToNetwork.TabIndex = 99;
            this.btnSaveDicomToNetwork.Title = "";
            this.btnSaveDicomToNetwork.UseVisualStyleBackColor = true;


            this.btnAddLesion.AllowDrop = true;
            this.btnAddLesion.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnAddLesion.BackColor = System.Drawing.Color.Transparent;
            this.btnAddLesion.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.确认肿瘤位置;
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
            this.btnAddLesion.Image = global::LungCare.SupportPlatform.Properties.Resources.Camera_05_3_;
            this.btnAddLesion.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnAddLesion.ImageOffset = 5;
            this.btnAddLesion.IsPressed = false;
            this.btnAddLesion.KeepPress = false;
            this.btnAddLesion.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnAddLesion.MenuPos = new System.Drawing.Point(0, 0);
            this.btnAddLesion.Name = "btnDrawLesion";
            this.btnAddLesion.Radius = 8;
            this.btnAddLesion.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnAddLesion.Size = new System.Drawing.Size(50, 50);
            this.btnAddLesion.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnAddLesion.SplitDistance = 0;
            this.btnAddLesion.TabIndex = 99;
            this.btnAddLesion.Title = "";
            this.btnAddLesion.UseVisualStyleBackColor = true;

            // 
            // btnExit
            // 
            this.btnExit.AllowDrop = true;
            this.btnExit.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnExit.BackColor = System.Drawing.Color.Transparent;
            this.btnExit.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnExit.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnExit.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnExit.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnExit.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnExit.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnExit.FadingSpeed = 20;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnExit.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnExit.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnExit.Image = global::LungCare.SupportPlatform.Properties.Resources.Quit;
            this.btnExit.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnExit.ImageOffset = 5;
            this.btnExit.IsPressed = false;
            this.btnExit.KeepPress = false;
            this.btnExit.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnExit.MenuPos = new System.Drawing.Point(0, 0);
            this.btnExit.Name = "btnExit";
            this.btnExit.Radius = 8;
            this.btnExit.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnExit.Size = new System.Drawing.Size(50, 50);
            this.btnExit.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnExit.SplitDistance = 0;
            this.btnExit.TabIndex = 95;
            this.btnExit.Title = "";
            this.btnExit.UseVisualStyleBackColor = true;

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


            this.btnMinisize.AllowDrop = true;
            this.btnMinisize.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnMinisize.BackColor = System.Drawing.Color.Transparent;
            this.btnMinisize.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.确认肿瘤位置;
            this.btnMinisize.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnMinisize.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnMinisize.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnMinisize.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnMinisize.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnMinisize.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnMinisize.FadingSpeed = 20;
            this.btnMinisize.FlatAppearance.BorderSize = 0;
            this.btnMinisize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinisize.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnMinisize.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnMinisize.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnMinisize.Image = global::LungCare.SupportPlatform.Properties.Resources.minimun;
            this.btnMinisize.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnMinisize.ImageOffset = 5;
            this.btnMinisize.IsPressed = false;
            this.btnMinisize.KeepPress = false;
            this.btnMinisize.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnMinisize.MenuPos = new System.Drawing.Point(0, 0);
            this.btnMinisize.Name = "btnSaveAirwayResult";
            this.btnMinisize.Radius = 8;
            this.btnMinisize.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnMinisize.Size = new System.Drawing.Size(50, 50);
            this.btnMinisize.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnMinisize.SplitDistance = 0;
            this.btnMinisize.TabIndex = 100;
            this.btnMinisize.Title = "";
            this.btnMinisize.UseVisualStyleBackColor = true;

            this.btnSelectLesionRegion.AllowDrop = true;
            this.btnSelectLesionRegion.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnSelectLesionRegion.BackColor = System.Drawing.Color.Transparent;
            this.btnSelectLesionRegion.BackgroundImage = global::LungCare.SupportPlatform.Properties.Resources.确认肿瘤位置;
            this.btnSelectLesionRegion.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnSelectLesionRegion.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnSelectLesionRegion.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnSelectLesionRegion.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnSelectLesionRegion.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnSelectLesionRegion.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnSelectLesionRegion.FadingSpeed = 20;
            this.btnSelectLesionRegion.FlatAppearance.BorderSize = 0;
            this.btnSelectLesionRegion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectLesionRegion.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnSelectLesionRegion.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnSelectLesionRegion.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnSelectLesionRegion.Image = global::LungCare.SupportPlatform.Properties.Resources.edit256;
            this.btnSelectLesionRegion.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnSelectLesionRegion.ImageOffset = 5;
            this.btnSelectLesionRegion.IsPressed = false;
            this.btnSelectLesionRegion.KeepPress = false;
            this.btnSelectLesionRegion.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnSelectLesionRegion.MenuPos = new System.Drawing.Point(0, 0);
            this.btnSelectLesionRegion.Name = "btnSaveAirwayResult";
            this.btnSelectLesionRegion.Radius = 8;
            this.btnSelectLesionRegion.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnSelectLesionRegion.Size = new System.Drawing.Size(50, 50);
            this.btnSelectLesionRegion.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnSelectLesionRegion.SplitDistance = 0;
            this.btnSelectLesionRegion.TabIndex = 100;
            this.btnSelectLesionRegion.Title = "";
            this.btnSelectLesionRegion.UseVisualStyleBackColor = true;

            btnImportDicom.Click += btnImportCT_Click;
            btnSaveDicomToLocal.Click += btnSaveCT_Click;
            btnSaveDicomToNetwork.Click+=btnSaveDicomToNetwork_Click;
            btnExit.Click += btnExit_Click;
            btnWidthAndLevel.Click += btnWidthAndLevel_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnZoomOut.Click += btnZoomOut_Click;
            btnPanning.Click += btnPanning_Click;
            btnAddLesion.Click += btnAddLesion_Click;
            btnMinisize.Click += btnMinisize_Click;
            btnSelectLesionRegion.Click += btnSelectLesionRegion_Click;
            panel.Controls.Add(btnImportDicom);
            panel.Controls.Add(btnAddLesion);
            panel.Controls.Add(btnSaveDicomToLocal);
            panel.Controls.Add(btnSaveDicomToNetwork);

            panel1.Controls.Add(btnSelectLesionRegion);
            panel1.Controls.Add(btnZoomOut);
            panel1.Controls.Add(btnPanning);
            panel1.Controls.Add(btnRefresh);
            panel1.Controls.Add(btnWidthAndLevel);

            panel2.Controls.Add(btnMinisize);
            panel2.Controls.Add(btnExit);

            System.Windows.Forms.ToolTip p1 = new System.Windows.Forms.ToolTip();
            p1.ShowAlways = true;
            p1.SetToolTip(this.btnImportDicom, "导入Dicom文件");
            p1.SetToolTip(this.btnSaveDicomToLocal, "保存到本地");
            p1.SetToolTip(this.btnSaveDicomToNetwork, "上传到云端");
            p1.SetToolTip(this.btnPanning, "平移");
            p1.SetToolTip(this.btnZoomOut, "放缩");
            p1.SetToolTip(this.btnWidthAndLevel, "窗宽窗位");
            p1.SetToolTip(this.btnAddLesion, "Snapshot");
            p1.SetToolTip(this.btnMinisize, "最小化");
            p1.SetToolTip(this.btnRefresh, "还原");
            p1.SetToolTip(this.btnExit, "退出");

            _dicomViewForm = new CTViewControlSingle();
            ctViewControlHost.Child = _dicomViewForm;

            _dicomViewForm.WindowWidthLevelHandler += _dicomViewForm_WindowWidthLevelHandler; 
      
            headBorder.MouseDown+=headBorder_MouseDown;
        }

        void btnSelectLesionRegion_Click(object sender, EventArgs e)
        {
            if (btnSelectLesionRegion.KeepPress)
            {
                btnSelectLesionRegion.KeepPress = false;
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                _dicomViewForm.StopDrawRectangle();
                return;
            }
            Button2Initial();
            btnSelectLesionRegion.KeepPress = true;
            _dicomViewForm.StartDrawRectangle();
            //throw new NotImplementedException();
        }

        void btnMinisize_Click(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
            //throw new NotImplementedException();
        }

        void btnAddLesion_Click(object sender, EventArgs e)
        {
            if (_dicomViewForm != null)
            {
               addLesionThumbImageRibbonBtn( _dicomViewForm.ScreenShot());
            }
            //throw new NotImplementedException();
        }

        void ImportDicomWindow3_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.RefreshRoundWindowRect();
            //throw new NotImplementedException();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            MedSys.PresentationCore.AdjustWindow.ChangeWindowSize changeWindowSize = new MedSys.PresentationCore.AdjustWindow.ChangeWindowSize(this);
            changeWindowSize.RegisterHook();
        }
        private void RefreshRoundWindowRect()
        {
            // 获取窗体句柄
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

            // 创建圆角窗体
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowRgn(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.CreateRoundRectRgn(0, 0, Convert.ToInt32(this.ActualWidth) + 1, Convert.ToInt32(this.ActualHeight) + 1, 5, 5), true);
        }

        private int _currentIndex = 0;
        private System.Drawing.Drawing2D.Matrix _matrix;
        void _dicomViewForm_WindowWidthLevelHandler(object sender, WindowWidthLevelArgs e)
        {
            if (btnCurrentSeriesButton != null)
            {
                btnCurrentSeriesButton.Tag = e.WindowWidth +" "+ e.WindowLevel+" "+e.Index;
                _currentIndex = e.Index;
                windowWidth = e.WindowWidth;
                windowLevel = e.WindowLevel;
                _matrix = e.Matrix;
            }
            //throw new NotImplementedException();
        }

        void panelDicomThumb_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                this.DragMove();
                _dicomViewForm.Refresh1();

            }
            catch (Exception)
            { }
            
            //throw new NotImplementedException();
        }

        


        void btnRefresh_Click(object sender, EventArgs e)
        {
            _dicomViewForm.SetPictureBoxWidth();
            //_dicomViewForm.RefreshWWWL();
        }

        void btnPanning_Click(object sender, EventArgs e)
        {
            if (btnPanning.KeepPress)
            {
                btnPanning.KeepPress = false;
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                _dicomViewForm.StopPanning();
                return;
            }
            Button2Initial();
            btnPanning.KeepPress = true;
            _dicomViewForm.StartPanning();
        }

        void btnZoomOut_Click(object sender, EventArgs e)
        {
            if (btnZoomOut.KeepPress)
            {
                btnZoomOut.KeepPress = false;
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                _dicomViewForm.StopZoom();
                return;
            }
            Button2Initial();
            btnZoomOut.KeepPress = true;
            _dicomViewForm.StartZoom();
        }
        void btnSaveDicomToNetwork_Click(object sender, EventArgs e)
        {
            ServiceListWnd serviceWindow = new ServiceListWnd();
            if(serviceWindow.ShowDialog() != true)
            {
                return;
            }
            if (_selectedDicomInfo != null)
                {
                    if (string.IsNullOrEmpty(_selectStudyUID))
                    {
                        if (!_allPatientDicomDataDic.ContainsKey(_selectStudyUID))
                        {
                            //if (!_allPatientDicomDataDic[_selectStudyUID].ContainsKey(_selectSeriesUID))
                            {
                                System.Windows.Forms.MessageBox.Show("请先选择一套数据！");
                                return;
                            }
                        }
                    }
            }
            else
            {
                return;
            }
            int fileCount = 0;
            //if (_selectedDicomInfo != null)
            //{
            //    if (!string.IsNullOrEmpty(_selectStudyUID))
            //    {
            //        if (_allPatientDicomDataDic.ContainsKey(_selectStudyUID))
            //        {
            //            foreach (var item in _allPatientDicomDataDic[_selectStudyUID].Keys)
            //            {
            //                foreach (var file in _allPatientDicomDataDic[_selectStudyUID][item])
            //                {
            //                    fileCount++;
            //                }
            //            }

            //        }

            //    }
            //}

            if (System.Windows.Forms.MessageBox.Show(string.Format("您将上传的病人数据是：{0} ！", _selectedDicomInfo.PatientName), @"确认", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                lbProgress.Content = "正在处理数据";
                lbProgress.Visibility = System.Windows.Visibility.Visible;
                System.Windows.Forms.Application.DoEvents();
                string dicomFolder = System.Windows.Forms.Application.StartupPath+ "\\dicomFolder";
                if (Directory.Exists(dicomFolder))
                {
                    FileDAO.DeleteFolder(dicomFolder);
                }

                if (_selectedDicomInfo != null)
                {
                    if (!string.IsNullOrEmpty(_selectStudyUID) && !string.IsNullOrEmpty(_selectSeriesUID))
                    {
                        if (_allPatientDicomDataDic.ContainsKey(_selectStudyUID))
                        {
                            foreach (var item in _allPatientDicomDataDic[_selectStudyUID].Keys)
                            {
                                if (_allPatientDicomDataDic[_selectStudyUID][item].Count > 0)
                                {
                                    string destFolder = Path.Combine(dicomFolder, _allPatientDicomDataDic[_selectStudyUID][item][0].SeriesInstanceUID);
                                    if (!Directory.Exists(destFolder))
                                    {
                                        Directory.CreateDirectory(destFolder);
                                    }
                                    foreach (var file in _allPatientDicomDataDic[_selectStudyUID][item])
                                    {
                                        File.Copy(file.FileName, Path.Combine(destFolder, Path.GetFileName(file.FileName)), true);
                                    }
                                }
                            }
                            //if (_allPatientDicomDataDic[_selectStudyUID].ContainsKey(_selectSeriesUID))
                            //{
                            //    foreach (var item in _allPatientDicomDataDic[_selectStudyUID][_selectSeriesUID])
                            //    {
                            //         File.Copy(item.FileName , Path.Combine(dicomFolder , Path.GetFileName(item.FileName)) , true);
                            //    }
                            //}
                        } 
                       
                    }
                }

                SaveSnapshotToNetwork();
                string zipfilename = zipFolder2Zip(dicomFolder);
                //string destDicomSaveFolder = "";

                ////string datetime = string.Format("{0:yyyy-MM-dd hh-mm-ss}",DateTime.Now);
                //destDicomSaveFolder = Path.Combine(AirwayPatients.BaseDicomFolder, MESPDownloadUpload.OrderId);

                //if (!Directory.Exists(destDicomSaveFolder))
                //{
                //    Directory.CreateDirectory(destDicomSaveFolder);
                //}
                //string[] files = Directory.GetFiles(dicomFolder);
                //foreach (var item in files)
                //{
                //    File.Copy(item, destDicomSaveFolder + "\\" + Path.GetFileName(item), true);
                //}


                UploadProgressWndMA upw = new UploadProgressWndMA();
                //upw.Owner = LungCare_Airway_PlanNav.MainWindow.Instance;
                upw.FileName = zipfilename;
                upw.InstitutionName = _selectedDicomInfo.InstitutionName;
                upw.PatientAge = _selectedDicomInfo.PatientAge;
                upw.PatientName = _selectedDicomInfo.PatientName;
                upw.PatientSex = _selectedDicomInfo.PatientSex;
                upw.SeriesInstanceUID = _selectedDicomInfo.SeriesInstanceUID;
                upw.StudyInstanceUID = _selectedDicomInfo.StudyInstanceUID;
                upw.acquisitionDate = _selectedDicomInfo.AcquisitionDate;
                upw.acquisitionTime = _selectedDicomInfo.AcquisitionTime;

                upw.ShowDialog();

                if (File.Exists(zipfilename))
                {
                    //File.Delete(zipfilename);
                }


                lbProgress.Visibility = System.Windows.Visibility.Hidden;
             //   File.Delete(zipfilename);
            }
            //throw new NotImplementedException();
        }
        #endregion

        private CTViewControlSingle _dicomViewForm;
        private CTDicomInfo _selectedDicomInfo;
        private string _selectStudyUID, _selectSeriesUID;
        private Dictionary<string,Dictionary<string , List<CTDicomInfo>>> _allPatientDicomDataDic;

        void btnWidthAndLevel_Click(object sender, EventArgs e)
        {
            if (btnWidthAndLevel.KeepPress)
            {
                btnWidthAndLevel.KeepPress = false;
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                _dicomViewForm.stopWindowLevelAndWidth();
                return;
            }
            Button2Initial();
            btnWidthAndLevel.KeepPress = true;
            _dicomViewForm.startWindowLevelAndWidth();
        }

        void btnExit_Click(object sender, EventArgs e) {
            if (System.Windows.MessageBox.Show("是否退出？", "退出", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        void btnSaveCT_Click(object sender,EventArgs e) {
            if (_selectedDicomInfo == null)
            {
                System.Windows.Forms.MessageBox.Show("至少选择一个病人数据！");
                return;
            }

            lbProgress.Content = "正在保存本地";
            lbProgress.Visibility = System.Windows.Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();
            if (_allPatientDicomDataDic.ContainsKey(_selectedDicomInfo.StudyInstanceUID))
            {
                foreach (var study in _allPatientDicomDataDic[_selectedDicomInfo.StudyInstanceUID])
                {
                    foreach (var series in _allPatientDicomDataDic[_selectedDicomInfo.StudyInstanceUID].Keys)
                    {
                     
                        string patientName = "patientName";
                        if (!string.IsNullOrEmpty(_selectedDicomInfo.PatientName))
                        {
                            patientName = _selectedDicomInfo.PatientName;
                        }
                        string destFolder = Path.Combine(AirwayPatients.BaseDicomFolder, patientName + "_" + _selectStudyUID, series);
                        if (!Directory.Exists(destFolder))
                        {
                            Directory.CreateDirectory(destFolder);
                        }

                        foreach (var dicomFile in _allPatientDicomDataDic[_selectedDicomInfo.StudyInstanceUID][series])
                        {
                            string sourceFile = dicomFile.FileName;
                            string destFile = Path.Combine(destFolder , Path.GetFileName(dicomFile.FileName));
                            try
                            {
                                File.Copy(sourceFile, destFile, true);  
                            }
                            catch (Exception ex)
                            {
                                LoggerEntity.Error(typeof(ImportDicomWindow3), ex.ToString());
                            }
                            
                        }
                        //string sourceFile = _a
                    }
                }
            }

            DataListItem dataItem = new DataListItem();
            dataItem.PatientName = _selectedDicomInfo.PatientName;
            dataItem.PatientSex = _selectedDicomInfo.PatientSex;
            dataItem.PatientAge = _selectedDicomInfo.PatientAge;
            dataItem.StudyInstanceUID = _selectedDicomInfo.StudyInstanceUID;
            dataItem.SeriesInstanceUID = _selectedDicomInfo.SeriesInstanceUID;
            dataItem.AcquisitionDate = _selectedDicomInfo.AcquisitionDate;
            dataItem.AcquisitionTime = _selectedDicomInfo.AcquisitionTime;
            dataItem.InstitutionName = _selectedDicomInfo.InstitutionName;
            dataItem.UploadTimestamp = string.Format("{0:yyyyMMdd HHmmss}", DateTime.Now);
            dataItem.Status = "待上传";
            dataItem.LocalDicom = "未上传";
            //为main界面显示未上传的列表获取studyUID
            dataItem.DataID = _selectedDicomInfo.StudyInstanceUID;
            DataListItemEntities.AddDataListItem(dataItem);


            SaveSnapshotToLocal();
            
            lbProgress.Visibility = System.Windows.Visibility.Hidden;

            System.Windows.Forms.MessageBox.Show("本地保存完成!");
            
            //));
            //thread.Start();
            /*
            AirwayPatient newPatient = new AirwayPatient();
            newPatient.OrderID = MESPDownloadUpload.OrderId;
            
            newPatient.DicomFolder = destDicomSaveFolder;
            
            AirwayPatients patients = AirwayPatients.TestLoad();
            patients.Insert(0, newPatient);
            AirwayPatients.TestSave(patients);
             * */
        }


        private DicomSnapshotEntities dicomSnapshotEntities = new DicomSnapshotEntities();
        private void SaveSnapshotToLocal()
        {
            string patientName = "patientName";
            if (!string.IsNullOrEmpty(_selectedDicomInfo.PatientName))
            {
                patientName = _selectedDicomInfo.PatientName;
            }
            string destFolder = Path.Combine(AirwayPatients.BaseDicomFolder, patientName + "_" + _selectStudyUID);
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            dicomSnapshotEntities = new DicomSnapshotEntities();
            foreach (var item in panelLesionThumb.Children)
            {
                SnapshotUserControl snap = item as SnapshotUserControl;
                if (snap.Bitmap != null)
                {
                    snap.Bitmap.Save(destFolder+"\\"+snap._seriesUID+"_"+snap._index+".jpg");
                }

                DicomSnapshotEntity dicomSnapshotEntity = new DicomSnapshotEntity();
                dicomSnapshotEntity.BitmapPath = destFolder + "\\" + snap._seriesUID + "_" + snap._index + ".jpg";
                dicomSnapshotEntity.id = snap.id;
                dicomSnapshotEntity.index = snap._index;
                dicomSnapshotEntity.matrix = snap._matrix;
                dicomSnapshotEntity.seriesUID = snap._seriesUID;
                dicomSnapshotEntity.studyUID = snap._studyUID;
                dicomSnapshotEntity.windowsLevel = snap._windowsLevel;
                dicomSnapshotEntity.windowWidth = snap._windowWidth;
                dicomSnapshotEntities.Add(dicomSnapshotEntity);
            }

            DicomSnapshotEntities.TestSave(dicomSnapshotEntities,destFolder);
        }

        private void SaveSnapshotToNetwork()
        {
            string destFolder = System.Windows.Forms.Application.StartupPath + "\\dicomFolder";
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            foreach (var item in panelLesionThumb.Children)
            {
                SnapshotUserControl snap = item as SnapshotUserControl;
                if (snap.Bitmap != null)
                {
                    snap.Bitmap.Save(destFolder + "\\" + snap._seriesUID + "_" + snap._index + ".jpg");
                }
            }
        }
        private string zipFolder2Zip(string folder)
        {

            string fileName = string.Format("[{0}][{3}][{1}][{2}].zip", _selectedDicomInfo.PatientName, _selectedDicomInfo.InstitutionName, System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            
            //string args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\" \"{3}\"", fileName, SelectedPatient.GetFile(""), SelectedPatient.DicomFolder, fileName);
            string args = string.Format("a -tzip \"{0}\" \"{1}\"\\*", fileName, folder);
            var psi = new ProcessStartInfo(System.Windows.Forms.Application.StartupPath + @"\HaoZipC.exe", args);
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(psi).WaitForExit();
//            System.Windows.Forms.MessageBox.Show(string.Format(@"压缩完毕。
//文件保存到{0}。", fileName));
            //Process.Start(new FileInfo(fileName).Directory.FullName);

            return fileName;
        }


        private void addThumbImageRibbonBtn(string studyUID, string seriesUID , string path)
        {

            DicomImage image;
            try
            {
                image = new DicomImage(path);
                image.WindowWidth = windowWidth;
                image.WindowCenter = windowLevel;
            }
            catch (Exception)
            {
                return;
            }
            
            RibbonStyle.RibbonMenuButton btnDicomSeries = new RibbonStyle.RibbonMenuButton();
            // 
            // btnTemp
            // 
            btnDicomSeries.AllowDrop = true;
            btnDicomSeries.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            btnDicomSeries.BackColor = System.Drawing.Color.Transparent;
            btnDicomSeries.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            btnDicomSeries.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            btnDicomSeries.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            btnDicomSeries.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            btnDicomSeries.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            btnDicomSeries.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            btnDicomSeries.FadingSpeed = 20;
            btnDicomSeries.FlatAppearance.BorderSize = 0;
            btnDicomSeries.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDicomSeries.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            btnDicomSeries.ForeColor = System.Drawing.Color.DarkBlue;
            btnDicomSeries.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            //btnTemp.Image = global::LungCare.Airway.WinformUIControls.Properties.Resources.Quit;
            btnDicomSeries.Image = (Bitmap)ImageOperation.CloneImage(image.RenderImage()); 
            btnDicomSeries.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            btnDicomSeries.ImageOffset = 5;
            btnDicomSeries.IsPressed = false;
            btnDicomSeries.KeepPress = false;
            btnDicomSeries.MaxImageSize = new System.Drawing.Point(0, 0);
            btnDicomSeries.MenuPos = new System.Drawing.Point(0, 0);
            btnDicomSeries.Name = seriesUID;
            btnDicomSeries.Title = studyUID;
            btnDicomSeries.Radius = 8;
            btnDicomSeries.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            btnDicomSeries.Size = new System.Drawing.Size(150, 150);
            btnDicomSeries.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            btnDicomSeries.SplitDistance = 0;
            btnDicomSeries.TabIndex = 95;
            
            btnDicomSeries.Tag = windowWidth + " " + windowLevel +" 0";
            btnDicomSeries.UseVisualStyleBackColor = true;
            btnDicomSeries.Click +=btnTemp_Click;
            panelDicomThumb.Controls.Add(btnDicomSeries);
        }


        private List<SnapshotUserControl> listSnapshotUserControls = new List<SnapshotUserControl>();
        private void LoadStudySnapshotUserControls()
        {
            panelLesionThumb.Children.Clear();
            foreach (var item in listSnapshotUserControls)
            {
                if (item._studyUID == _selectStudyUID)
                {
                    panelLesionThumb.Children.Add(item);
                }
            }
        }

        private void LoadStudySnapshotUserControlsFromLocal()
        {
            if (_selectedDicomInfo == null)
            {
                return;
            }
            string destFolder = Path.Combine(AirwayPatients.BaseDicomFolder, _selectedDicomInfo.PatientName + "_" + _selectStudyUID);
            dicomSnapshotEntities =   DicomSnapshotEntities.TestLoad(destFolder);
            if (dicomSnapshotEntities != null)
            {
                foreach (var item in dicomSnapshotEntities)
                {
                    SnapshotUserControl snapshotUserControl = new SnapshotUserControl();
                    snapshotUserControl.Margin = new Thickness(5);
                    snapshotUserControl.DeleteClick += snapshotUserControl_DeleteClick;
                    snapshotUserControl.ImageClick += snapshotUserControl_ImageClick;
                    snapshotUserControl.Width = 180;
                    snapshotUserControl.Height = 180;
                    panelLesionThumb.Children.Add(snapshotUserControl);
                    snapshotUserControl.SetImage(new Bitmap(item.BitmapPath), item.studyUID, item.seriesUID, item.windowWidth,
                        item.windowsLevel, item.index, item.matrix);
                    listSnapshotUserControls.Add(snapshotUserControl);
                }
            }
        }
        private void addLesionThumbImageRibbonBtn(Bitmap bitmap)
        {

            SnapshotUserControl snapshotUserControl = new SnapshotUserControl();
            snapshotUserControl.Margin = new Thickness(5);
            snapshotUserControl.DeleteClick += snapshotUserControl_DeleteClick;
            snapshotUserControl.ImageClick += snapshotUserControl_ImageClick;
            snapshotUserControl.Width = 180;
            snapshotUserControl.Height = 180;
            panelLesionThumb.Children.Add(snapshotUserControl);
            snapshotUserControl.SetImage(bitmap , _selectStudyUID ,_selectSeriesUID ,windowWidth , 
                windowLevel , _currentIndex , _matrix);
            listSnapshotUserControls.Add(snapshotUserControl);

            return;
            RibbonStyle.RibbonMenuButton btnDicomSeries = new RibbonStyle.RibbonMenuButton();
            // 
            // btnTemp
            // 
            btnDicomSeries.AllowDrop = true;
            btnDicomSeries.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            btnDicomSeries.BackColor = System.Drawing.Color.Transparent;
            btnDicomSeries.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            btnDicomSeries.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            btnDicomSeries.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            btnDicomSeries.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            btnDicomSeries.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            btnDicomSeries.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            btnDicomSeries.FadingSpeed = 20;
            btnDicomSeries.FlatAppearance.BorderSize = 0;
            btnDicomSeries.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDicomSeries.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            btnDicomSeries.ForeColor = System.Drawing.Color.DarkBlue;
            btnDicomSeries.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            //btnTemp.Image = global::LungCare.Airway.WinformUIControls.Properties.Resources.Quit;
            btnDicomSeries.Image = (Bitmap)bitmap;
            btnDicomSeries.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            btnDicomSeries.ImageOffset = 5;
            btnDicomSeries.IsPressed = false;
            btnDicomSeries.KeepPress = false;
            btnDicomSeries.MaxImageSize = new System.Drawing.Point(0, 0);
            btnDicomSeries.MenuPos = new System.Drawing.Point(0, 0);
            btnDicomSeries.Radius = 8;
            btnDicomSeries.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            btnDicomSeries.Size = new System.Drawing.Size(120, 120);
            btnDicomSeries.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            btnDicomSeries.SplitDistance = 0;
            btnDicomSeries.TabIndex = 95;

            btnDicomSeries.UseVisualStyleBackColor = true;
            //btnDicomSeries.Click += btnTemp_Click;
            //panelLesionThumb.Controls.Add(btnDicomSeries);
        }

        void snapshotUserControl_ImageClick(object sender, DicomSnapshotEventArgs e)
        {

            try
            {


                if (_allPatientDicomDataDic != null)
                {
                    if (_allPatientDicomDataDic.ContainsKey(e.studyUID))
                    {
                        if (_allPatientDicomDataDic[e.studyUID].ContainsKey(e.seriesUID))
                        {
                            if (e.Index < _allPatientDicomDataDic[e.studyUID][e.seriesUID].Count)
                            {
                                _dicomViewForm.SetPicturexBoxImage(_allPatientDicomDataDic[e.studyUID][e.seriesUID][e.Index].FileName,
                                    e.WindowWidth, e.WindowLevel, e.Matrix);
                                _selectedDicomInfo = getDicomMetaData(_allPatientDicomDataDic[e.studyUID][e.seriesUID][e.Index].FileName);
                                _dicomViewForm.SetData(_selectedDicomInfo, e.Index, _allPatientDicomDataDic[e.studyUID][e.seriesUID].Count);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            //throw new NotImplementedException();
        }

        void snapshotUserControl_DeleteClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SnapshotUserControl snap = sender as SnapshotUserControl;
            panelLesionThumb.Children.Remove(snap);

            int delete = -1;
            for (int i = 0; i < listSnapshotUserControls.Count; i++)
			{
			  if (listSnapshotUserControls[i].id == snap.id)
                {
                    delete = i; 
                }  
			}

            if (delete != -1)
            {
                listSnapshotUserControls.RemoveAt(delete);
            }
            //throw new NotImplementedException();
        }

        private RibbonStyle.RibbonMenuButton btnCurrentSeriesButton;
        private void btnTemp_Click(object sender, EventArgs e)
        {
            _dicomViewForm.Refresh1();
             btnCurrentSeriesButton = (RibbonStyle.RibbonMenuButton)sender;
            
            btnCurrentSeriesButton.KeepPress = true;
            string studyUID = btnCurrentSeriesButton.Title;
            string seriesUID = btnCurrentSeriesButton.Name;
            
            _selectStudyUID = studyUID;
            _selectSeriesUID = seriesUID;
            Console.WriteLine(_selectStudyUID);
            Console.WriteLine(_selectSeriesUID);
            if (_allPatientDicomDataDic.ContainsKey(studyUID))
            {
                if (_allPatientDicomDataDic[studyUID].ContainsKey(seriesUID))
                {
                    ArrayList files = new ArrayList();
                    foreach (var item in _allPatientDicomDataDic[studyUID][seriesUID])
                    {
                        files.Add(item.FileName);
                    }
                   
                    //Array files1 = files.ToArray();
                    //Array.Sort(files1, new FileNameComparer1());
                    if (btnCurrentSeriesButton.Tag != null)
                    {
                        string content = (string)btnCurrentSeriesButton.Tag;
                        string[] values = content.Split(' ');
                        if (values != null)
                        {
                            if (values.Length > 1)
                            {
                                windowWidth = int.Parse(values[0]);
                                windowLevel = int.Parse(values[1]);

                                //if(windowWidth>=800 && windowWidth<=1200
                                //    && windowLevel >= -700 && windowLevel <= -400)
                                //{
                                //    rbLungWWWL.IsChecked = true;
                                //}
                                //else if (windowWidth >= 250 && windowWidth <= 450
                                //   && windowLevel >= 0 && windowLevel <= 100)
                                //{
                                //    rbAbdomenWWWL.IsChecked = true;
                                //}
                                //else
                                //{
                                //    rbAbdomenWWWL.IsChecked = false;
                                //    rbLungWWWL.IsChecked = false;
                                //}
                                int index = int.Parse(values[2]);
                                Console.WriteLine(windowWidth+" "+windowLevel+" "+index);
                                if (index < _allPatientDicomDataDic[studyUID][seriesUID].Count)
                                {
                                    //_dicomViewForm.SetPicturexBoxImage0(_allPatientDicomDataDic[studyUID][seriesUID][index].FileName , windowWidth , windowLevel);
                                    _dicomViewForm.SetImageList(files ,windowWidth , windowLevel, index);
                                }
                                //_dicomViewForm.SetWindowWidthLevel(windowWidth , windowLevel);
                            }
                        }

                    }
                  
                    if (files.Count > 0)
                    {
                        _dicomViewForm.SetData(_allPatientDicomDataDic[studyUID][seriesUID][0], _allPatientDicomDataDic[studyUID][seriesUID].Count);
                        if (_allPatientDicomDataDic[studyUID][seriesUID][0].patientPosition == "HFS")
                        {
                            rbHFSHeadFirstSupine.IsChecked = true;
                        }
                        else if (_allPatientDicomDataDic[studyUID][seriesUID][0].patientPosition == "FFS")
                        {
                            rbFFSFeetFirstSupine.IsChecked = true;
                        }
                        else
                        {
                            rbFFSFeetFirstSupine.IsChecked = false;
                            rbHFSHeadFirstSupine.IsChecked = false;
                        }
                    }
                }
            }
            //int number = int.Parse(btnTemp.Name);

           
        }

       
        private void InitParameters()
        {
            _allPatientDicomDataDic = new Dictionary<string, Dictionary<string, List<CTDicomInfo>>>();
            _patients = new List<CTDicomInfo>();
            panelDicomThumb.Controls.Clear();
            _index = 0;
            _fileIndex = 0;
            _validDicomFileCount = 0;
            panelLesionThumb.Children.Clear();
        }
        void btnImportCT_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            String selectPath = @"C:\AirwayVE";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbProgress.Visibility = System.Windows.Visibility.Visible;
                selectPath = fbd.SelectedPath;

                InitParameters();
                GetDicomFileCount(selectPath);

                LoadSelectPathDicoms(selectPath);

                FilterInValidDicom();
                dataGridCT.SelectedIndex = 0;
                SelectFirstSeries();
                System.Windows.Forms.MessageBox.Show("导入成功！");
                lbProgress.Visibility = System.Windows.Visibility.Hidden;
                //dataGridCT_SelectionChanged(this, new System.Windows.Controls.SelectionChangedEventArgs());
            }

        }




        /// <summary>
        /// 将路径下的一套CT数据的病人信息读取出来
        /// </summary>
        /// <param name="dicomPath"></param>
        private CTDicomInfo getDicomMetaData(String dicomPath)
        {
            //ArrayList CTFileList = _dictionary[dicomPath];
            try
            {
                DicomFile dcm = DicomFile.Open(dicomPath);
                DicomDataset dcmDataset = dcm.Dataset;
                CTDicomInfo ctEntity = new CTDicomInfo();
                ctEntity.FileName = dicomPath;
                ctEntity.PatientName = dcmDataset.Get<String>(DicomTag.PatientName, "");
                ctEntity.PatientAge = dcmDataset.Get<String>(DicomTag.PatientAge, "");
                ctEntity.PatientSex = dcmDataset.Get<String>(DicomTag.PatientSex, "");
                ctEntity.InstitutionName = dcmDataset.Get<String>(DicomTag.InstitutionName, "");
                ctEntity.AcquisitionDate = dcmDataset.Get<String>(DicomTag.AcquisitionDate, "");
                ctEntity.AcquisitionTime = dcmDataset.Get<String>(DicomTag.AcquisitionTime, "");
                ctEntity.SliceThickness = dcmDataset.Get<double>(DicomTag.SliceThickness);
                ctEntity.SeriesInstanceUID = dcmDataset.Get<string>(DicomTag.SeriesInstanceUID);
                ctEntity.StudyInstanceUID = dcmDataset.Get<string>(DicomTag.StudyInstanceUID);
                ctEntity.InstanceNumber = dcmDataset.Get<string>(DicomTag.InstanceNumber);
                ctEntity.patientPosition = dcmDataset.Get<string>(DicomTag.PatientPosition);

                return ctEntity;
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(dicomPath + " " + ex.ToString());
            }

            return null;
        }



        /// <summary>
        /// 将路径下的一套CT数据的病人信息读取出来
        /// </summary>
        /// <param name="dicomPath"></param>
        private CTDicomInfo getDicomMetaData(DicomImage dicomImage, string dicomPath)
        {
            //ArrayList CTFileList = _dictionary[dicomPath];
            DicomDataset dcmDataset = dicomImage.Dataset;
            CTDicomInfo ctEntity = new CTDicomInfo();
            ctEntity.FileName = dicomPath;
            try
            {
                ctEntity.PatientName = dcmDataset.Get<String>(DicomTag.PatientName, "");
                ctEntity.PatientAge = dcmDataset.Get<String>(DicomTag.PatientAge, "");
                ctEntity.PatientSex = dcmDataset.Get<String>(DicomTag.PatientSex, "");
                ctEntity.InstitutionName = dcmDataset.Get<String>(DicomTag.InstitutionName, "");
                ctEntity.AcquisitionDate = dcmDataset.Get<String>(DicomTag.AcquisitionDate, "");
                ctEntity.AcquisitionTime = dcmDataset.Get<String>(DicomTag.AcquisitionTime, "");
                ctEntity.SliceThickness = dcmDataset.Get<double>(DicomTag.SliceThickness);
                ctEntity.SeriesInstanceUID = dcmDataset.Get<string>(DicomTag.SeriesInstanceUID);
                ctEntity.StudyInstanceUID = dcmDataset.Get<string>(DicomTag.StudyInstanceUID);
                ctEntity.InstanceNumber = dcmDataset.Get<string>(DicomTag.InstanceNumber);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(dicomPath +" "+ex.ToString());
            }
            
            
            return ctEntity;
        }


        /// <summary>
        /// 获取路径下面的所有文件夹并判断是否符合DICOM要求
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="CTFileList"></param>
        public void LoadSelectPathDicoms(String folderPath)
        {
           
            checkCTFolder(folderPath);
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            DirectoryInfo[] dirList = dir.GetDirectories();

            if (dirList.Length < 1)
            {
                return;
            }
            else { 
                foreach(DirectoryInfo di in dirList){
                    LoadSelectPathDicoms(di.FullName);
                }
            }
            
        }

        private int _validDicomFileCount = 0;
        private int GetDicomFileCount(string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath);
            foreach (var item in files)
            {
                _validDicomFileCount++;

            }
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            DirectoryInfo[] dirList = dir.GetDirectories();


            if (dirList.Length < 1)
            {
                return _validDicomFileCount;
            }
            else
            {
                foreach (DirectoryInfo di in dirList)
                {
                    GetDicomFileCount(di.FullName);
                }
            }

            return _validDicomFileCount;
        }

        private List<CTDicomInfo> _patients;
        private int _index = 0;
        private int _fileIndex = 0;
        private int windowWidth = 1000;
        private int windowLevel = -600;
        /// <summary>
        /// 判断一个不包含文件夹的文件夹是不是符合CT文件夹，文件夹中的CT文件数要大于20
        /// </summary>
        /// <param name="folderPath">需要判断的文件夹路径</param>
        public void checkCTFolder(String folderPath) {
            //DirectoryInfo dir = new DirectoryInfo(folderPath);
            //FileInfo[] fileList = dir.GetFiles();
            string[] files = Directory.GetFiles(folderPath);
            if (files.Length < 1)
            {
                return;
            }

            try
            {
                Array.Sort(files, new FileNameComparer1());
            }
            catch (Exception ex)
            {
            }
            
            _index = 0;
            Stopwatch time = new Stopwatch();
            foreach (string file in files)
            {
                //判断文件路径是不是DICOM
                
                time.Restart();
                
                
                if (validateDicom1(file))
                {
                    //Console.WriteLine(file);

                    if (_index % 15 == 0)
                    {
                        _dicomViewForm.SetPicturexBoxImage(file, windowWidth, windowLevel);
                    }
                    try
                    {
                        _selectedDicomInfo = getDicomMetaData(file);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (_selectedDicomInfo == null)
                    {
                        return;
                    }

                    
                 
                    if (_selectedDicomInfo!=null&& !_allPatientDicomDataDic.ContainsKey(_selectedDicomInfo.StudyInstanceUID))
                    {
                        _allPatientDicomDataDic.Add(_selectedDicomInfo.StudyInstanceUID, new Dictionary<string, List<CTDicomInfo>>());


                        _allPatientDicomDataDic[_selectedDicomInfo.StudyInstanceUID].Add(_selectedDicomInfo.SeriesInstanceUID, new List<CTDicomInfo>());
                        _allPatientDicomDataDic[_selectedDicomInfo.StudyInstanceUID][_selectedDicomInfo.SeriesInstanceUID].Add(_selectedDicomInfo);
                        addThumbImageRibbonBtn(_selectedDicomInfo.StudyInstanceUID, _selectedDicomInfo.SeriesInstanceUID, file);
                        System.Windows.Forms.Application.DoEvents();


                        _patients.Add(_selectedDicomInfo);
                        _selectStudyUID = _selectedDicomInfo.StudyInstanceUID;
                        LoadStudySnapshotUserControlsFromLocal();

                        foreach (var item in _patients)
                        {
                            if (item.PatientSex == "M")
                            {
                                item.PatientSex = "男";
                            }
                            else if (item.PatientSex == "F")
                            {
                                item.PatientSex = "女";
                            }
                        }
                        this.dataGridCT.Visibility = System.Windows.Visibility.Visible;
                        this.dataGridCT.ItemsSource = null;
                        this.dataGridCT.ItemsSource = _patients;
                        System.Windows.Forms.Application.DoEvents();


                        //addThumbImageRibbonBtn(_selectedDicomInfo.SeriesInstanceUID , file);
                    }
                    else
                    {
                        if (_selectedDicomInfo!=null && _allPatientDicomDataDic!=null&& !_allPatientDicomDataDic[_selectedDicomInfo.StudyInstanceUID].ContainsKey(_selectedDicomInfo.SeriesInstanceUID))
                        {
                            if (_selectedDicomInfo.patientPosition == "HFS")
                            {
                                rbHFSHeadFirstSupine.IsChecked = true;
                            }
                            else if (_selectedDicomInfo.patientPosition == "FFS")
                            {
                                rbFFSFeetFirstSupine.IsChecked = true;
                            }else
                            {
                                rbFFSFeetFirstSupine.IsChecked = false;
                                rbHFSHeadFirstSupine.IsChecked = false;
                            }
                            _allPatientDicomDataDic[_selectedDicomInfo.StudyInstanceUID].Add(_selectedDicomInfo.SeriesInstanceUID, new List<CTDicomInfo>());
                            _allPatientDicomDataDic[_selectedDicomInfo.StudyInstanceUID][_selectedDicomInfo.SeriesInstanceUID].Add(_selectedDicomInfo);
                            addThumbImageRibbonBtn(_selectedDicomInfo.StudyInstanceUID, _selectedDicomInfo.SeriesInstanceUID, file);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            _allPatientDicomDataDic[_selectedDicomInfo.StudyInstanceUID][_selectedDicomInfo.SeriesInstanceUID].Add(_selectedDicomInfo);
                        }
                        //_allPatientDicomDataDic[_selectedDicomInfo.StudyInstanceUID].Add(_selectedDicomInfo);
                    }
                }

                //Console.WriteLine(time.ElapsedMilliseconds + " mm");
                _index++;
                _fileIndex++;
                lbProgress.Content=_fileIndex.ToString()+" / " +_validDicomFileCount.ToString();
                System.Windows.Forms.Application.DoEvents();
            }

        }

        public void SelectFirstSeries()
        {
            
            if (panelDicomThumb.Controls.Count > 0)
            {
                try
                {
                    btnCurrentSeriesButton = (RibbonStyle.RibbonMenuButton)panelDicomThumb.Controls[0];
                    btnCurrentSeriesButton.PerformClick();

                    
                    if (_dicomViewForm != null)
                    {
                        _dicomViewForm.SetPictureBoxWidth();
                        _dicomViewForm.Refresh1();
                    }
                }
                catch (Exception)
                {
                }

            }

            if (_dicomViewForm != null)
            {
                _dicomViewForm.SetPictureBoxWidth();
            }
        }

        private static DicomImage _dicomImage;
        /// <summary>
        /// 判断文件的路径是否是dcm文件
        /// </summary>
        /// <param name="filePath">传入参数是文件的路径</param>
        /// <returns></returns>
        public static bool isDicom(String filePath)
        {
            try
            {
                _dicomImage = new DicomImage(filePath);
                return true;
            }
            catch (Exception)
            {
                _dicomImage = null;
                return false;
            }
           

            
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            byte[] data = new byte[132];
            fs.Read(data, 0, data.Length);

            int b0 = data[0] & 255, b1 = data[1] & 255, b2 = data[2] & 255, b3 = data[3] & 255;

            if (data[128] == 68 && data[129] == 73 && data[130] == 67 && data[131] == 77)
            {
                return true;
            }
            else if ((b0 == 8 || b0 == 2) && b1 == 0 && b3 == 0)
            {
                return true;
            }
            return false;
        }



        public static DicomImage validateDicom(String filePath)
        {
            _dicomImage = null;
           string extention = Path.GetExtension(filePath);
           if (extention == "" || extention == ".dcm")
           {

           }else
           {
               return null;
           }



           FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

           byte[] data = new byte[132];
           fs.Read(data, 0, data.Length);

           int b0 = data[0] & 255, b1 = data[1] & 255, b2 = data[2] & 255, b3 = data[3] & 255;

           if (data[128] == 68 && data[129] == 73 && data[130] == 67 && data[131] == 77)
           {
               try
               {
                   _dicomImage = new DicomImage(filePath);
               }
               catch (Exception)
               {
                   _dicomImage = null;
               }
           }
           else if ((b0 == 8 || b0 == 2) && b1 == 0 && b3 == 0)
           {
               try
               {
                   _dicomImage = new DicomImage(filePath);
               }
               catch (Exception)
               {
                   _dicomImage = null;
               }
           }
           return _dicomImage;


            



            
        }



        public static bool validateDicom1(String filePath)
        {
            _dicomImage = null;
            if (Path.GetFileName(filePath) == "DICOMDIR" || Path.GetFileName(filePath) == "DIRFILE")
            {
                return false;
            }
            string extention = Path.GetExtension(filePath);
            if (extention == "" || extention == ".dcm")
            {

            }
            else
            {
                return false;
            }

            try
            {
               // ClearCanvas.ImageViewer.PresentationImageFactory.Create(new ClearCanvas.ImageViewer.StudyManagement.ImageSop(filePath).Frames[1]).DrawToBitmap(512, 512);
            }
            catch (Exception)
            {
                return false;
            }
            

            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            byte[] data = new byte[132];
            fs.Read(data, 0, data.Length);

            int b0 = data[0] & 255, b1 = data[1] & 255, b2 = data[2] & 255, b3 = data[3] & 255;

            if (data[128] == 68 && data[129] == 73 && data[130] == 67 && data[131] == 77)
            {
                return true;
            }
            else if ((b0 == 8 || b0 == 2) && b1 == 0 && b3 == 0)
            {
                return  true;
            }
            return false;
        }

        /**
         * 检查dcm文件中modility属性是否是"CT"来判断文件是否可用
         * @param firstFilePath
         * @return
         */
        public static bool checkModility(String firstFilePath)
        {
            //FileInfo file = new FileInfo(firstFilePath);
            DicomFile dcm = DicomFile.Open(firstFilePath);
            //DicomFileMetaInformation dcmMetaInfo = dcm.FileMetaInfo;
            DicomDataset dcmDataset = dcm.Dataset;
            String modality = dcmDataset.Get<String>(DicomTag.Modality, "");
            if (modality.ToUpper().Equals("CT"))
            {
                return true;
            }
            return false;
        }

        public class FilesNameComparerClass : IComparer
        {
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            ///<summary>
            ///比较两个字符串，如果含用数字，则数字按数字的大小来比较。
            ///</summary>
            ///<param name="x"></param>
            ///<param name="y"></param>
            ///<returns></returns>
            int IComparer.Compare(Object x, Object y)
            {
                if (x == null || y == null)
                    throw new ArgumentException("Parameters can't be null");
                string fileA = x as string;
                string fileB = y as string;
                char[] arr1 = fileA.ToCharArray();
                char[] arr2 = fileB.ToCharArray();
                int i = 0, j = 0;
                while (i < arr1.Length && j < arr2.Length)
                {
                    if (char.IsDigit(arr1[i]) && char.IsDigit(arr2[j]))
                    {
                        string s1 = "", s2 = "";
                        while (i < arr1.Length && char.IsDigit(arr1[i]))
                        {
                            s1 += arr1[i];
                            i++;
                        }
                        while (j < arr2.Length && char.IsDigit(arr2[j]))
                        {
                            s2 += arr2[j];
                            j++;
                        }
                        if (int.Parse(s1) > int.Parse(s2))
                        {
                            return 1;
                        }
                        if (int.Parse(s1) < int.Parse(s2))
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        if (arr1[i] > arr2[j])
                        {
                            return 1;
                        }
                        if (arr1[i] < arr2[j])
                        {
                            return -1;
                        }
                        i++;
                        j++;
                    }
                }
                if (arr1.Length == arr2.Length)
                {
                    return 0;
                }
                else
                {
                    return arr1.Length > arr2.Length ? 1 : -1;
                }
            }
        }


        class CTEntity {
            public String patientName { set; get; }
            public String CTNumber { set; get; }
            public String patientSex { set; get; }
            public String patientAge { set; get; }

            public string studyUID { get; set; }
        }

        private void dataGridCT_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_dicomViewForm != null)
            {
                _dicomViewForm.Refresh1();
            }

          
            if (e.AddedItems.Count > 0)
            {
                


                 CTDicomInfo patient = (CTDicomInfo)e.AddedItems[0];

                 _selectedDicomInfo = patient;
                 _selectStudyUID = patient.StudyInstanceUID;

                 LoadStudySnapshotUserControls();
                 //_selectSeriesUID = patient.SeriesInstanceUID;
                 LoadSelectPatientSeriesDicom(patient.StudyInstanceUID);
                
                 ArrayList files = new ArrayList();
                 if (_allPatientDicomDataDic.ContainsKey(patient.StudyInstanceUID))
                 {
                     if (_allPatientDicomDataDic[patient.StudyInstanceUID].Keys.Count > 0)
                     {
                         foreach (var seriesItem in _allPatientDicomDataDic[patient.StudyInstanceUID].Keys)
                         {
                             foreach (var item in _allPatientDicomDataDic[patient.StudyInstanceUID][seriesItem])
                             {
                                 _selectSeriesUID = item.SeriesInstanceUID;
                                 files.Add(item.FileName);
                             }
                         }
                     }
                 }
                 
                 if (files.Count > 1)
                 {
                     _dicomViewForm.SetImageList(files);
                     if (_allPatientDicomDataDic.ContainsKey(patient.StudyInstanceUID))
                     {
                         if (_allPatientDicomDataDic[patient.StudyInstanceUID].ContainsKey(patient.SeriesInstanceUID))
                         {
                             if (_allPatientDicomDataDic[patient.StudyInstanceUID][patient.SeriesInstanceUID].Count > 0)
                             {
                                 _dicomViewForm.SetData(_allPatientDicomDataDic[patient.StudyInstanceUID][patient.SeriesInstanceUID][0], _allPatientDicomDataDic[patient.StudyInstanceUID][_selectSeriesUID].Count);
                             }
                         }
                     }
                     
                 }
            }
            
        }


        private void LoadSelectPatientSeriesDicom(string studyUID)
        {
            if (_allPatientDicomDataDic.ContainsKey(studyUID))
            {
                panelDicomThumb.Controls.Clear();
                foreach (var item in _allPatientDicomDataDic[studyUID].Keys)
                {
                    if (_allPatientDicomDataDic[studyUID][item].Count > 0)
                    {
                        addThumbImageRibbonBtn(studyUID , item, _allPatientDicomDataDic[studyUID][item][0].FileName);
                    }
                    
                }
            }
        }

        private void FilterInValidDicom()
        {
            List<Pair> removedList = new List<Pair>();
            foreach (var studyItem in _allPatientDicomDataDic.Keys)
            {
                foreach (var seriesItem in _allPatientDicomDataDic[studyItem].Keys)
                {
                    if (_allPatientDicomDataDic[studyItem][seriesItem].Count < 2)
                    {
                        removedList.Add(new Pair(studyItem , seriesItem));
                    }

                }    
            }


            foreach (var item in removedList)
            {
                _allPatientDicomDataDic[item.Name].Remove(item.Value);
            }
            
        }

        class Pair
        {
            public Pair(string name, string value)
            {
                Name = name;
                Value = value;
            }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        private void rbLungWWWL_Checked(object sender, RoutedEventArgs e)
        {
            windowWidth = 1000;
            windowLevel = -600;
            if (_dicomViewForm != null)
            {
                _dicomViewForm.SwithToLung();
                _dicomViewForm.Refresh1();
            }
        }

        private void rbAbdomenWWWL_Checked(object sender, RoutedEventArgs e)
        {
            windowWidth = 350;
            windowLevel = 50;
            if (_dicomViewForm != null)
            {
                _dicomViewForm.SwithToAbdomen();
                _dicomViewForm.Refresh1();
            }
        }

        private void headBorder_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
                _dicomViewForm.Refresh1();
               
            }
            catch (Exception)
            {}
        }


        private void Button2Initial()
        {
            btnPanning.KeepPress = false;
            btnWidthAndLevel.KeepPress = false;
            btnZoomOut.KeepPress = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_dicomViewForm != null)
            {
                _dicomViewForm.SetPictureBoxWidth();
            }
        }
    }
}

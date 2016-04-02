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
using LungCare.SupportPlatform.UI.UserControls.Examination;
using LungCare.SupportPlatform.SupportPlatformDAO.Utils;
using LungCare.SupportPlatform.SupportPlatformDAO.Images;

namespace LungCare.SupportPlatform.UI.Windows.Examination
{
    /// <summary>
    /// ImportWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImportWindow : Window
    {
        public ImportWindow()
        {
            InitializeComponent();
            LoadComponent();
        }


        private RibbonStyle.RibbonMenuButton btnImportCT;
        private RibbonStyle.RibbonMenuButton btnSaveCT;
        private RibbonStyle.RibbonMenuButton btnExit;
        private void LoadComponent() {
            btnImportCT = new RibbonStyle.RibbonMenuButton();
            btnSaveCT = new RibbonStyle.RibbonMenuButton();
            btnExit = new RibbonStyle.RibbonMenuButton();
            // 
            // btnImportCT
            // 
            this.btnImportCT.AllowDrop = true;
            this.btnImportCT.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnImportCT.BackColor = System.Drawing.Color.Transparent;
            //this.btnImportCT.BackgroundImage = global::LungCare.Airway.WinformUIControls.Properties.Resources.支气管加点;
            this.btnImportCT.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnImportCT.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnImportCT.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnImportCT.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnImportCT.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnImportCT.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnImportCT.FadingSpeed = 20;
            this.btnImportCT.FlatAppearance.BorderSize = 0;
            this.btnImportCT.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImportCT.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnImportCT.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnImportCT.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnImportCT.Image = global::LungCare.SupportPlatform.Properties.Resources.open;
            this.btnImportCT.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnImportCT.ImageOffset = 5;
            this.btnImportCT.IsPressed = false;
            this.btnImportCT.KeepPress = false;
            this.btnImportCT.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnImportCT.MenuPos = new System.Drawing.Point(0, 0);
            this.btnImportCT.Name = "btnAddPoint";
            this.btnImportCT.Radius = 8;
            this.btnImportCT.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnImportCT.Size = new System.Drawing.Size(50, 50);
            this.btnImportCT.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnImportCT.SplitDistance = 0;
            this.btnImportCT.TabIndex = 96;
            this.btnImportCT.Title = "";
            this.btnImportCT.UseVisualStyleBackColor = true;
            // 
            // btnSaveCT
            // 
            this.btnSaveCT.AllowDrop = true;
            this.btnSaveCT.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.btnSaveCT.BackColor = System.Drawing.Color.Transparent;
            //this.btnSaveCT.BackgroundImage = global::LungCare.Airway.WinformUIControls.Properties.Resources.确认肿瘤位置;
            this.btnSaveCT.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            this.btnSaveCT.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.btnSaveCT.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            this.btnSaveCT.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            this.btnSaveCT.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnSaveCT.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnSaveCT.FadingSpeed = 20;
            this.btnSaveCT.FlatAppearance.BorderSize = 0;
            this.btnSaveCT.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveCT.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnSaveCT.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnSaveCT.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.btnSaveCT.Image = global::LungCare.SupportPlatform.Properties.Resources.save;
            this.btnSaveCT.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.btnSaveCT.ImageOffset = 5;
            this.btnSaveCT.IsPressed = false;
            this.btnSaveCT.KeepPress = false;
            this.btnSaveCT.MaxImageSize = new System.Drawing.Point(0, 0);
            this.btnSaveCT.MenuPos = new System.Drawing.Point(0, 0);
            this.btnSaveCT.Name = "btnDrawLesion";
            this.btnSaveCT.Radius = 8;
            this.btnSaveCT.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.btnSaveCT.Size = new System.Drawing.Size(50, 50);
            this.btnSaveCT.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.btnSaveCT.SplitDistance = 0;
            this.btnSaveCT.TabIndex = 99;
            this.btnSaveCT.Title = "";
            this.btnSaveCT.UseVisualStyleBackColor = true;
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

            btnImportCT.Click += btnImportCT_Click;
            btnSaveCT.Click += btnSaveCT_Click;
            btnExit.Click += btnExit_Click;


            panel.Controls.Add(btnImportCT);
            panel.Controls.Add(btnSaveCT);
            panel.Controls.Add(btnExit);

            System.Windows.Forms.ToolTip p1 = new System.Windows.Forms.ToolTip();
            p1.ShowAlways = true;
            p1.SetToolTip(this.btnImportCT, "导入CT文件");
            p1.SetToolTip(this.btnSaveCT, "保存");
            p1.SetToolTip(this.btnExit, "退出");

            ctview = new CTViewControlSingle();
            ctViewControlHost.Child = ctview;
        }

        private CTViewControlSingle ctview;
        void btnExit_Click(object sender, EventArgs e) {
            if (System.Windows.MessageBox.Show("是否退出？", "退出", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        void btnSaveCT_Click(object sender,EventArgs e) {

            string folder = Path.GetDirectoryName((string)_dictionary[_selectPath][0]);
            string zipfilename = zipFolder2Zip(folder);
            //System.Windows.MessageBox.Show("saved success");
            UploadProgressWndMA upw = new UploadProgressWndMA();
            //upw.Owner = LungCare_Airway_PlanNav.MainWindow.Instance;
            upw.FileName = zipfilename;
            upw.InstitutionName =_selectedtCTDicomInfo.InstitutionName;
            upw.PatientAge = _selectedtCTDicomInfo.PatientAge;
            upw.PatientName = _selectedtCTDicomInfo.PatientName;
            upw.PatientSex = _selectedtCTDicomInfo.PatientSex;
            upw.SeriesInstanceUID = _selectedtCTDicomInfo.SeriesInstanceUID;
            upw.StudyInstanceUID = _selectedtCTDicomInfo.StudyInstanceUID;
            upw.acquisitionDate = _selectedtCTDicomInfo.AcquisitionDate;
            upw.acquisitionTime = _selectedtCTDicomInfo.AcquisitionTime;

            upw.ShowDialog();
        }


        private string zipFolder2Zip(string folder)
        {

            string fileName = string.Format("[{0}][{3}][{1}][{2}].zip", _selectedtCTDicomInfo.PatientName, _selectedtCTDicomInfo.InstitutionName, System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

            //string args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\" \"{3}\"", fileName, SelectedPatient.GetFile(""), SelectedPatient.DicomFolder, fileName);
            string args = string.Format("a -tzip \"{0}\" \"{1}\"", fileName, folder);
            var psi = new ProcessStartInfo(System.Windows.Forms.Application.StartupPath + @"\HaoZipC.exe", args);
            Process.Start(psi).WaitForExit();
            System.Windows.Forms.MessageBox.Show(string.Format(@"压缩完毕。
文件保存到{0}。", fileName));
            //Process.Start(new FileInfo(fileName).Directory.FullName);

            return fileName;
        }


        private RibbonStyle.RibbonMenuButton getRibbonBtn(String CTFolderPath,int numb)
        {
            ArrayList CTFileList = _dictionary[CTFolderPath];
            DicomImage image = new DicomImage(Path.Combine(CTFolderPath, CTFileList[0].ToString()));
            image.WindowWidth = 1000;
            image.WindowCenter = -600;
            RibbonStyle.RibbonMenuButton btnTemp = new RibbonStyle.RibbonMenuButton();
            // 
            // btnTemp
            // 
            btnTemp.AllowDrop = true;
            btnTemp.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            btnTemp.BackColor = System.Drawing.Color.Transparent;
            btnTemp.ColorBase = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(4)))), ((int)(((byte)(4)))));
            btnTemp.ColorBaseStroke = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(153)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            btnTemp.ColorOn = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(78)))));
            btnTemp.ColorOnStroke = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(177)))), ((int)(((byte)(118)))));
            btnTemp.ColorPress = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            btnTemp.ColorPressStroke = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            btnTemp.FadingSpeed = 20;
            btnTemp.FlatAppearance.BorderSize = 0;
            btnTemp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnTemp.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            btnTemp.ForeColor = System.Drawing.Color.DarkBlue;
            btnTemp.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            //btnTemp.Image = global::LungCare.Airway.WinformUIControls.Properties.Resources.Quit;
            btnTemp.Image = (Bitmap)ImageOperation.CloneImage(image.RenderImage()); 
            btnTemp.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            btnTemp.ImageOffset = 5;
            btnTemp.IsPressed = false;
            btnTemp.KeepPress = false;
            btnTemp.MaxImageSize = new System.Drawing.Point(0, 0);
            btnTemp.MenuPos = new System.Drawing.Point(0, 0);
            btnTemp.Name = numb.ToString();
            btnTemp.Radius = 8;
            btnTemp.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            btnTemp.Size = new System.Drawing.Size(150, 150);
            btnTemp.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            btnTemp.SplitDistance = 0;
            btnTemp.TabIndex = 95;
            btnTemp.Title = "";
            btnTemp.UseVisualStyleBackColor = true;
            btnTemp.Click +=btnTemp_Click;

            return btnTemp;
        }

        private CTDicomInfo _selectedtCTDicomInfo;
        private void btnTemp_Click(object sender, EventArgs e)
        {
            RibbonStyle.RibbonMenuButton btnTemp = (RibbonStyle.RibbonMenuButton)sender;
            int number = int.Parse(btnTemp.Name);
            if (number < _CTPathList.Count)
            {
                _selectPath = _CTPathList[number].ToString();
                _selectedtCTDicomInfo = (CTDicomInfo)_CTPatientsList[number];
                ctview.SetImageList(_dictionary[_selectPath]);
            }
            else {
                System.Windows.MessageBox.Show("数据不存在");
            }

        }

        Dictionary<String, ArrayList> _dictionary = new Dictionary<String, ArrayList>();//用来存放键值对，String是文件夹，ArrayList里面是文件夹下的文件名字
        ArrayList _CTPathList = new ArrayList();//所有符合条件的CT文件夹路径
        String _selectPath = String.Empty;
        void btnImportCT_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            String selectPath = @"C:\AirwayVE";
            if(fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK){
                selectPath = fbd.SelectedPath;
            }

            _dictionary.Clear();
            _CTPathList.Clear();
            panelCTList.Controls.Clear();

            getAllCTPath(selectPath);
            
            for (int i = 0; i < _CTPathList.Count;i++ )
            {
                String path = _CTPathList[i].ToString();
                RibbonStyle.RibbonMenuButton btnTemp = getRibbonBtn(path,i);
                panelCTList.Controls.Add(btnTemp);
                setCTData(path);
            }

            this.dataGridCT.ItemsSource = _CTPatientsList;
        }



        ArrayList _CTPatientsList = new ArrayList();

        /// <summary>
        /// 将路径下的一套CT数据的病人信息读取出来
        /// </summary>
        /// <param name="CTFolderPath"></param>
        private void setCTData(String CTFolderPath)
        {
            ArrayList CTFileList = _dictionary[CTFolderPath];

            DicomFile dcm = DicomFile.Open(Path.Combine(CTFolderPath, CTFileList[0].ToString()));
            DicomDataset dcmDataset = dcm.Dataset;
            CTDicomInfo1 ctEntity = new CTDicomInfo1();
            ctEntity.PatientName = dcmDataset.Get<String>(DicomTag.PatientName, "");
            ctEntity.PatientAge = dcmDataset.Get<String>(DicomTag.PatientAge, "");
            ctEntity.PatientSex = dcmDataset.Get<String>(DicomTag.PatientSex, "");
            ctEntity.InstitutionName = dcmDataset.Get<String>(DicomTag.InstitutionName, "");
            ctEntity.AcquisitionDate = dcmDataset.Get<String>(DicomTag.AcquisitionDate, "");
            ctEntity.AcquisitionTime = dcmDataset.Get<String>(DicomTag.AcquisitionTime, "");

            _CTPatientsList.Add(ctEntity);

        }


        /// <summary>
        /// 获取路径下面的所有文件夹并判断是否符合DICOM要求
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="CTFileList"></param>
        public void getAllCTPath(String folderPath)
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
                    getAllCTPath(di.FullName);
                }
            }
        }

        /// <summary>
        /// 判断一个不包含文件夹的文件夹是不是符合CT文件夹，文件夹中的CT文件数要大于20
        /// </summary>
        /// <param name="folderPath">需要判断的文件夹路径</param>
        public void checkCTFolder(String folderPath) {
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] fileList = dir.GetFiles();
            if(fileList.Length<1){
                return;
            }
            ArrayList CTFileList = new ArrayList();
            foreach (FileInfo fi in fileList)
            {
                //判断文件路径是不是DICOM
                if (isDicom(fi.FullName))
                {
                    Console.WriteLine(fi.Name);
                    CTFileList.Add(fi.FullName);
                }
            }
            if(CTFileList.Count>=20){
                //IComparer fileNameCompare = new FilesNameComparerClass();
                //CTFileList.Sort(fileNameCompare);
                Array temp = CTFileList.ToArray();
                Array.Sort(temp, new FileNameComparer1()); 
                CTFileList.Clear();
                foreach(String s in temp){
                    CTFileList.Add(s);
                }
                _dictionary.Add(folderPath,CTFileList);
                _CTPathList.Add(folderPath);
            }

            if (_CTPathList.Count!=0 &&_CTPathList.Count < 2)
            {
                ctview.SetImageList(_dictionary[_CTPathList[0].ToString()]);
            }

        }

        /// <summary>
        /// 判断文件的路径是否是dcm文件
        /// </summary>
        /// <param name="filePath">传入参数是文件的路径</param>
        /// <returns></returns>
        public static bool isDicom(String filePath)
        {
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
        }

        private void dataGridCT_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            String path = _CTPathList[dataGridCT.SelectedIndex].ToString();
            ctview.SetImageList(_dictionary[path]);
        }

    }
}

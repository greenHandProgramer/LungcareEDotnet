using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Dicom;
using LungCare.Airway;
using Kitware.VTK;
using System.Threading.Tasks;
using Teboscreen;
using System.Collections;
using LungCare.SupportPlatform.UI;
using ClearCanvas.ImageViewer;
using ClearCanvas.ImageViewer.StudyManagement;
using LungCare.SupportPlatform.SupportPlatformDAO.VTK;
using LungCare.SupportPlatform.UI.Windows.Common;
using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.ZLF;
using LungCare.SupportPlatform.UI.Windows.Examination;
using AirwayCT.Entity;

namespace AirwayCT
{
    public partial class PatientMgrForm : Form
    {
        public PatientMgrForm()
        {
            InitializeComponent();
           
        }

        internal AirwayPatient SelectedAirwayPatient;
        //{
        //    get { return AirwayPatients.Deserialize()[dataGridViewPatient.SelectedRows[0].Index]; }
        //}

        int? SelectedRowIndex
        {
            get
            {
                if (dataGridViewPatient.SelectedRows.Count == 0)
                {
                    return null;
                }
                else
                {
                    return dataGridViewPatient.SelectedRows[0].Index;
                }
            }
        }

        private void SearchForNext(string firstLetter)
        {
            if (!SelectedRowIndex.HasValue)
            {
                return;
            }

            AirwayPatients patients = AirwayPatients.TestLoad();
            if (patients == null) {
                MessageBox.Show("patients数据为空");
                return;
            }

            for (int i = SelectedRowIndex.Value + 1; i < patients.Count; ++i)
            {
                string patientName = patients[i].Name.ToUpper();
                if (patientName.StartsWith(firstLetter.ToUpper()))
                {
                    dataGridViewPatient.Rows[i].Selected = true;
                    if (!dataGridViewPatient.Rows[i].Displayed)
                    {
                        dataGridViewPatient.FirstDisplayedScrollingRowIndex = i;
                    }
                    return;
                }
            }

            for (int i = 0; i < SelectedRowIndex.Value - 1; ++i)
            {
                string patientName = patients[i].Name.ToUpper();
                if (patientName.StartsWith(firstLetter.ToUpper()))
                {
                    dataGridViewPatient.Rows[i].Selected = true;
                    if (!dataGridViewPatient.Rows[i].Displayed)
                    {
                        dataGridViewPatient.FirstDisplayedScrollingRowIndex = i;
                    }
                    return;
                }
            }
        }
        void GotoTop()
        {
            int? selectedRowIndex = SelectedRowIndex;
            if (!selectedRowIndex.HasValue || selectedRowIndex.Value == 0)
            {
                return;
            }

            dataGridViewPatient.Rows[0].Selected = true;
            dataGridViewPatient.FirstDisplayedScrollingRowIndex = 0;
        }

        void GotoBottom()
        {
            int? selectedRowIndex = SelectedRowIndex;

            dataGridViewPatient.Rows[dataGridViewPatient.Rows.Count - 1].Selected = true;
            dataGridViewPatient.FirstDisplayedScrollingRowIndex = dataGridViewPatient.Rows.Count - 1;
        }

        void Up()
        {
            int? selectedRowIndex = SelectedRowIndex;
            if (!selectedRowIndex.HasValue || selectedRowIndex.Value == 0)
            {
                return;
            }

            dataGridViewPatient.Rows[selectedRowIndex.Value - 1].Selected = true;
            if (!dataGridViewPatient.Rows[selectedRowIndex.Value - 1].Displayed)
            {
                dataGridViewPatient.FirstDisplayedScrollingRowIndex = selectedRowIndex.Value - 1;
            }
        }

        void Down()
        {
            int? selectedRowIndex = SelectedRowIndex;
            if (!selectedRowIndex.HasValue || selectedRowIndex.Value == dataGridViewPatient.RowCount - 1)
            {
                return;
            }

            dataGridViewPatient.Rows[selectedRowIndex.Value + 1].Selected = true;
            if (!dataGridViewPatient.Rows[selectedRowIndex.Value + 1].Displayed)
            {
                dataGridViewPatient.FirstDisplayedScrollingRowIndex = selectedRowIndex.Value + 1;
            }
        }

        private AirwayPatient SelectedPatient
        {
            get
            {
                AirwayPatients all = AirwayPatients.Deserialize();
                if (all != null&&all.Count>0)
                {
                    AirwayPatient selectedAirwayPatient = all[dataGridViewPatient.SelectedRows[0].Index];
                    return selectedAirwayPatient;
                }
                else {
                    return null;
                }

            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Down)
            {
                Down();
            }
            else if (keyData == Keys.Up)
            {
                Up();
            }
            else if (keyData == Keys.Home)
            {
                GotoTop();
            }
            else if (keyData == Keys.End)
            {
                GotoBottom();
            }
            else if (keyData == Keys.Enter)
            {
                btnNext.PerformClick();
            }
            else if (keyData == Keys.Escape)
            {
                Ask4Exit();
            }
            else if (keyData == Keys.N)
            {
                nDIReplayerToolStripMenuItem.PerformClick();
            }
            else if (com.jxdw.helper.Helper.ControlStat && keyData == Keys.A)
            {
                Process.Start(@"C:\app\MetaImageViewer\MetaImageViewer.exe", SelectedPatient.AirwayVTP_FileName);
            }
            else if (com.jxdw.helper.Helper.ControlStat && keyData == Keys.V)
            {
                Process.Start(@"C:\app\MetaImageViewer\MetaImageViewer.exe", SelectedPatient.吸气末期MhdFileName);
            }
            else if (com.jxdw.helper.Helper.ControlStat && keyData == Keys.L)
            {
                Process.Start(@"C:\app\MetaImageViewer\MetaImageViewer.exe", SelectedPatient.Labeling_VTP_FileName);
            }
            else if (com.jxdw.helper.Helper.ControlStat && keyData == Keys.C)
            {
                Process.Start(@"C:\app\MetaImageViewer\MetaImageViewer.exe", SelectedPatient.AirwayNetwork_VTP_FileName);
            }
            else if (keyData == (Keys.F4 | Keys.Alt))
            {
                Ask4Exit();
            }
            else if (keyData == (Keys.F4 | Keys.Control))
            {
                Ask4Exit();
            }
            else { SearchForNext(keyData.ToString()); }

            return true;
        }

        private XmlPolyDataPackage airwayPolyDataPackage = null;
        private XmlPolyDataPackage networkPolyDataPackage = null;
        private XmlPolyDataPackage lesionPackage = null;
        private RendererPackage _rendererPackage;

        private void 病人管理Form_Load(object sender, EventArgs e)
        {
            
            //Dicom.Imaging.DicomImage dicom1 = new Dicom.Imaging.DicomImage(@"C:\AirwayVE0\dicom\周东云\DICOM\ST00001\SE00002\IM00001");
            //if (dicom1.PixelData.Syntax.Endian.ToString().Contains("loss"))
            //{

            //}
           // FileDAO.zipFolderFiles2ZipWithoutFolder(@"C:\1.2.840.113619.2.334.3.269091143.997.1412721251.405.41" , @"test.zip");
            
            ReloadPatientDataGridView();
            UpdateButtonStatus();

            //panelDataProcess.Visible = false;
            //tableLayoutPanel1.Visible = true;
            //tableLayoutPanel1.Dock = DockStyle.Fill;

            //_rendererPackage = new RendererPackage(panelVTK);
            //_rendererPackage.StartRefreshAll();
            //_rendererPackage.GradientOn();
            //_rendererPackage.冠状位();

         //   LoadLastVisitedPatient();
            try
            {
                //SetMainform2VCAPParent();
            }
            catch
            {
                //Process p = Process.Start(LungCareConfigurationApp.Form1.导航程序Path);

                //while (true)
                //{
                //    Thread.Sleep(100);
                //    try
                //    {
                //        var client = new LungCareMainFormServiceClient();
                //        client.GetWindowHandle();
                //        break;
                //    }
                //    catch
                //    {
                //    }
                //}

                //Thread.Sleep(1000);

                //SetMainform2VCAPParent();
            }
        }

        private void ReloadPatientDataGridView()
        {
            //string lastPatientId = LungCareConfigurationApp.Form1.LastPatientId;
            string lastPatientId = "";
            dataGridViewPatient.AutoGenerateColumns = false;
            AirwayPatients airwayPatients = AirwayPatients.Deserialize();
            
            foreach (AirwayPatient airwayPatient in airwayPatients)
            {
                try
                {
                    if (string.IsNullOrEmpty(airwayPatient.Institution))
                    {
                        string firstDicomFileName = Path.Combine(airwayPatient.DicomFolder, "1");
                        if (File.Exists(firstDicomFileName))
                        {
                            //string tempFileName = Path.GetTempFileName();
                            //File.Copy(firstDicomFileName, tempFileName, true);
                            Dicom.DicomFile dicomFile = DicomFile.Open(firstDicomFileName);
                            string institutionName = dicomFile.Dataset.Get<string>(DicomTag.InstitutionName);
                            airwayPatient.Institution = institutionName;
                            //File.Delete(tempFileName);
                        }
                    }
                }
                catch
                {
                }
            }

            foreach (AirwayPatient airwayPatient in airwayPatients)
            {
                string infoFileName = airwayPatient.GetFile("Info.txt");

                if (File.Exists(infoFileName))
                {
                    string[] lines = File.ReadAllLines(infoFileName);

                    airwayPatient.入组编号 = lines[0];
                    airwayPatient.组别 = lines[1];
                    airwayPatient.中文名 = lines[2];
                }
            }

            AirwayPatients.TestSave(airwayPatients);

            dataGridViewPatient.DataSource = null;
            dataGridViewPatient.DataSource = airwayPatients;

            if (airwayPatients.Count(item =>
            {
                return item.PatientId == lastPatientId;
            }) != 0)
            {
                int index = airwayPatients.FindIndex(item => item.PatientId == lastPatientId);
                dataGridViewPatient.Rows[index].Selected = true;
            }

            dataGridViewPatient_SelectionChanged(this, new EventArgs());
            //dataGridViewPatient.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.);
        }

        private void Ask4Exit()
        {
            ///SoundPlayerController.Play提示();

            if (MessageBox.Show(@"是否退出？", @"退出程序", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void SetCursor2Wait()
        {
            Cursor = Cursors.WaitCursor;
        }

        private void SetCursor2Arrow()
        {
            Cursor = Cursors.Arrow;
        }

        private void UpdateButtonStatus()
        {
            if (dataGridViewPatient.SelectedRows.Count > 0)
            {
            }
            else
            {
            }
        }

        private void 病人管理Form_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void LoadLastVisitedPatient()
        {
            if (!File.Exists("lastVisitedPatientId.txt"))
            {
                return;
            }
            string lastVisitedPatientId = File.ReadAllText("lastVisitedPatientId.txt");

            AirwayPatients patients = AirwayPatients.TestLoad();
            if (patients.Exists(item => item.PatientId == lastVisitedPatientId))
            {
                int index = patients.FindIndex(item => item.PatientId == lastVisitedPatientId);
                dataGridViewPatient.Rows[index].Selected = true;
            }
        }

        private void PatientMgrForm_Paint(object sender, PaintEventArgs e)
        {
            //Form form = this;

            //Graphics g = e.Graphics;
            //Color FColor = Color.Black;
            //Color TColor = Color.FromArgb(50, 97, 150);
            //Brush b = new LinearGradientBrush(form.ClientRectangle, FColor, TColor, LinearGradientMode.ForwardDiagonal);
            //g.FillRectangle(b, form.ClientRectangle);
        }

        private void PatientMgrForm_Activated(object sender, EventArgs e)
        {
            UpdateButtonStatus();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Ask4Exit();
        }

        private vtkImageData _rawCTMetaImage;
        private vtkImageData _segmentedMetaImage;
        private void btnNext_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            SelectedAirwayPatient = SelectedPatient;
            if(SelectedAirwayPatient==null){
                MessageBox.Show("请选中一套数据");
                return;
            }

            //Enabled = false;
            Cursor = Cursors.WaitCursor;
            label1.BackColor = Color.FromArgb(89,39,89);
            label1.Visible = true;
            label1.BringToFront();
            Application.DoEvents();

            _rawCTMetaImage = SelectedPatient.吸气末期MhdFileName.ReadMetaImage();
            //_segmentedMetaImage = SelectedPatient.SegmentedMhd_FileName.ReadMetaImage();

            CTRefineMainForm = new MainWindowMA(SelectedPatient,
                OrientationEnum.Axial,
                _rawCTMetaImage, SelectedPatient.SegmentedMhd_FileName);
            CTRefineMainForm.Loaded += CTRefineMainForm_Loaded;
            //sCTRefineMainForm.Parent = this;
            CTRefineMainForm.Show();
        }
        MainWindowMA CTRefineMainForm;
        void CTRefineMainForm_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            label1.Visible = false;
            Enabled = true;
            Cursor = Cursors.Arrow;
            _rawCTMetaImage.Dispose();
            _rawCTMetaImage = null;
        }

        private void dataGridViewPatient_SelectionChanged(object sender, EventArgs e)
        {
            if (!SelectedRowIndex.HasValue)
            {
                return;
            }

            //LungCareConfigurationApp.Form1.LastPatientId = SelectedPatient.PatientId;
         
            Update3D();

            if (!SelectedPatient.GetFile("dcm.txt").FileExists() && File.Exists(@"C:\App\DcmInfoExtractor\DcmInfoExtractor.exe"))
            {
                Process.Start(@"C:\App\DcmInfoExtractor\DcmInfoExtractor.exe",
                    string.Format("{0} {1}", Directory.GetFiles(SelectedPatient.DicomFolder)[0], SelectedPatient.GetFile("dcm.txt"))).WaitForExit();
            }

            需要ColorToolStripMenuItem1.Checked = !SelectedPatient.NotRequireLabeling;
            不需要ColorToolStripMenuItem.Checked = SelectedPatient.NotRequireLabeling;

            if (SelectedPatient.XRay冠状位.FileExists())
            {
                Bitmap fromFile = new Bitmap(SelectedPatient.XRay冠状位);
                if (SelectedPatient.吸气末期MhdFileName.FileExists())
                {
                    double[] spacing = SelectedPatient.吸气末期MhdFileName.ReadMetaImageSpacing();
                    fromFile = Com.Jxdw.Helper.ImageHelper.ResizeImage(fromFile, fromFile.Width, (int)(fromFile.Height * spacing[2] / spacing[1]));
                }
                pbCTImage.Image = new Bitmap(fromFile);
                fromFile.Dispose();

                if (_showXRay)
                {
                    pbCTImage.Visible = true;
                }
            }
            else
            {
                pbCTImage.Visible = false;
            }
        }

        private void Update3D()
        {
            if (_rendererPackage != null && SelectedPatient.Labeling_VTP_FileName.FileExists())
            {
                if (!_showXRay)
                {
                    panelVTK.Visible = true;
                }

                if (airwayPolyDataPackage == null)
                {
                    airwayPolyDataPackage = new XmlPolyDataPackage(SelectedPatient.Labeling_VTP_FileName,
                        _rendererPackage.Renderer);
                    airwayPolyDataPackage.SetOpacity(0.6);
                    airwayPolyDataPackage.粉色();
                    airwayPolyDataPackage.ScalarVisibilityOn();
                    airwayPolyDataPackage.VisibilityOff();
                }
                else
                {
                    //airwayPolyDataPackage.ReplacePolyDataIfExists(SelectedPatient.Labeling_VTP_FileName);
                }

                if (networkPolyDataPackage == null)
                {
                    networkPolyDataPackage = new XmlPolyDataPackage(SelectedPatient.AirwayNetwork_FIXED_VTP_FileName,
                        _rendererPackage.Renderer);
                }
                else
                {
                    //networkPolyDataPackage.ReplacePolyDataIfExists(SelectedPatient.AirwayNetwork_FIXED_VTP_FileName);
                }

                if (SelectedPatient.GetLesionVTPFileName(0).FileExists())
                {
                    if (lesionPackage == null)
                    {
                        lesionPackage = new XmlPolyDataPackage(SelectedPatient.GetLesionVTPFileName(0), _rendererPackage.Renderer);
                        lesionPackage.VisibilityOff();
                    }
                    else
                    {
                        //lesionPackage.ReplacePolyDataIfExists(SelectedPatient.GetLesionVTPFileName(0));
                        lesionPackage.SetColor(1, 1, 0);
                        lesionPackage.SetOpacity(0.3);
                        lesionPackage.VisibilityOn();
                    }
                }
                else
                {
                    if (lesionPackage != null)
                    {
                        lesionPackage.VisibilityOff();
                    }
                }

                _rendererPackage.ResetCameraAndRefreshAll();
            }
        }

        private void zipPatientDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!@"C:\Program Files\2345Soft\HaoZip\HaoZipC.exe".FileExists())
            {
                MessageBox.Show(@"请安装haozip。http://haozip.2345.com/");
                return;
            }
            if (!SelectedRowIndex.HasValue)
            {
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = string.Format("{0}.zip", SelectedPatient.PatientId);
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // "C:\Program Files\2345Soft\HaoZip\HaoZipC.exe" a -tzip c:/a.zip I:\AirwayVE\Db\ZHANGYONG_M_20145693114_bfd3d7a2adbf425ba8a4497917bac0f1
                string args = string.Format("a -tzip {0} {1}", sfd.FileName, SelectedPatient.GetFile(""));
                ProcessStartInfo psi = new ProcessStartInfo(@"C:\Program Files\2345Soft\HaoZip\HaoZipC.exe", args);
                Process.Start(psi).WaitForExit();
                MessageBox.Show(string.Format(@"压缩完毕。
文件保存到{0}。", sfd.FileName));
                Process.Start(new FileInfo(sfd.FileName).Directory.FullName);
            }
        }

        private void cutPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //LungCare.Airway.NetworkCutPath.NetworkCutPathForm form = new NetworkCutPathForm
            //{
            //    AirwayPatient = SelectedPatient
            //};

            //form.ShowDialog();
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VTKUtil.ExtractLungRegionCT(this.SelectedPatient.LungRegion_MHD, SelectedPatient.吸气末期MhdFileName, SelectedPatient.GetFile("lungRegionOnly.mhd"));
            MessageBox.Show(@"DONE");
        }

        private void registrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //PickRegistrationPointsForm form = new PickRegistrationPointsForm
            //{
            //    Patient = SelectedPatient,
            //};
            //form.ShowDialog();
        }

        private void labelPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //NetworkLabelingPathForm networkLabelingPathForm = new NetworkLabelingPathForm
            //{
            //    AirwayPatient = SelectedPatient
            //};
            //networkLabelingPathForm.ShowDialog();
        }

        private void generateCMPRBmpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //vtkImageData cmpr = this.SelectedPatient.GetFile("VEBUS.mhd").ReadMetaImage();

            //int[] dims = cmpr.GetDimensions();

            //Bitmap bmp = new Bitmap(dims[2], dims[0]);

            //for (int x = 0; x < dims[0]; ++x)
            //{
            //    for (int z = 0; z < dims[2]; ++z)
            //    {
            //        int y = dims[1] / 2 + 2;

            //        double value = cmpr.GetScalarComponentAsDouble(x, y, z, 0);
            //        byte rgb = VTKUtil.Short2UChar(500, 1500, value);
            //        bmp.SetPixel(z, x, Color.FromArgb(rgb, rgb, rgb));
            //    }
            //}

            //bmp.Save(SelectedPatient.GetFile("CMPR.bmp"));
            //MessageBox.Show(@"DONE");
        }

        private void btn导出_Click(object sender, EventArgs e)
        {
            //if (!@"C:\Program Files\2345Soft\HaoZip\HaoZipC.exe".FileExists())
            //{
            //    MessageBox.Show(@"请安装haozip。http://haozip.2345.com/");
            //    return;
            //}

            if (!SelectedRowIndex.HasValue)
            {
                return;
            }

            string fileName = string.Format("[{0}][{3}][{1}][{2}].zip", SelectedPatient.Name, SelectedPatient.Institution, System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            fileName = fileName.Replace(' ', '-');

            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = fileName,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string airwayPatientFileName = Path.Combine(Path.GetTempPath(), "airwaypatient.xml");
                SelectedPatient.Save(airwayPatientFileName);

                // "C:\Program Files\2345Soft\HaoZip\HaoZipC.exe" a -tzip c:/a.zip I:\AirwayVE\Db\ZHANGYONG_M_20145693114_bfd3d7a2adbf425ba8a4497917bac0f1
                string args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\" \"{3}\"", sfd.FileName, SelectedPatient.GetFile(""), SelectedPatient.DicomFolder, airwayPatientFileName);
                var psi = new ProcessStartInfo(Application.StartupPath + @"\HaoZipC.exe", args);
                Process.Start(psi).WaitForExit();
                MessageBox.Show(string.Format(@"压缩完毕。
文件保存到{0}。", sfd.FileName));
                Process.Start(new FileInfo(sfd.FileName).Directory.FullName);
            }
        }

        private bool validateExtention(string extent)
        {
            return extent == ".rar" || extent == ".zip" || extent == ".7z" || extent == ".rar5";
        }



        private void btnImportPatient_Click(object sender, EventArgs e)
        {
            //if (false)
            //{
            //    MessageBox.Show(@"HKEY_CURRENT_USER\Software\LungCare\AirwayNav\AirwayPatientsRootPath not configured.");
            //    return;
            //}

            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "压缩包文件|*.zip;*rar;*.7z;*.rar5";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string filename = ofd.FileName;
                    if (!validateExtention(Path.GetExtension(ofd.FileName)))
                    {
                        MessageBox.Show("文件格式不合格，请选择压缩包文件", "提示：");

                        return;
                    }
                    //string folder = Path.Combine(AirPatientForm.RegistryDAO.AirwayBaseFolder, Path.GetFileNameWithoutExtension(filename).Replace(" ", ""));
                    string folder = Path.Combine("C:\\AirwayVE\\DB", Path.GetFileNameWithoutExtension(filename).Replace(" ", ""));
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    string args = string.Format("x -o\"{0}\" -y -sn \"{1}\"", folder, filename);
                    var psi = new ProcessStartInfo(Application.StartupPath + @"\HaoZipC.exe", args);
                    Process.Start(psi).WaitForExit();

                    string filenam1e = Path.Combine(folder, "airwaypatient.xml");

                    if (!File.Exists(filenam1e))
                    {
                        MessageBox.Show(@"压缩文件中未找到文件 : airwaypatient.xml");
                        return;
                    }
                    AirwayPatient patient = AirwayPatient.Load(filenam1e);
                    DirectoryInfo origDicomDirectory = new DirectoryInfo(patient.DicomFolder);
                    DirectoryInfo origLungcareDataDirectory =
                        new DirectoryInfo(Path.GetDirectoryName(patient.AirwayVTP_FileName));

                    string localDicomFolder = Path.Combine(folder, origDicomDirectory.Name);
                    string localLungCareDataDirectory = Path.Combine(folder, origLungcareDataDirectory.Name);

                    patient.AirwayVTP_FileName = patient.AirwayVTP_FileName.ReplaceFolder(localLungCareDataDirectory);
                    patient.AirwayNetwork_VTP_FileName = patient.AirwayNetwork_VTP_FileName.ReplaceFolder(localLungCareDataDirectory);
                    patient.吸气末期MhdFileName = patient.吸气末期MhdFileName.ReplaceFolder(localLungCareDataDirectory);
                    patient.SegmentedMhd_FileName = patient.SegmentedMhd_FileName.ReplaceFolder(localLungCareDataDirectory);
                    patient.LungRegion_VTP = patient.LungRegion_VTP.ReplaceFolder(localLungCareDataDirectory);
                    patient.LungRegion_MHD = patient.LungRegion_MHD.ReplaceFolder(localLungCareDataDirectory);

                    patient.DicomFolder = localDicomFolder;

                    if (AirwayPatients.FindById(patient.PatientId) != null)
                    {
                        AirwayPatients.UpdatePatient(patient);
                    }
                    else
                    {
                        AirwayPatients patients = AirwayPatients.TestLoad();
                        patients.Insert(0, patient);
                        AirwayPatients.TestSave(patients);
                    }
                    ReloadPatientDataGridView();
                    MessageBox.Show(string.Format(@"导入成功。病人姓名：{0}", patient.Name), @"导入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            return;
        }


        private void btnUpload_Click(object sender, EventArgs e)
        {
            ImportDicomWindow3 import = new ImportDicomWindow3();
            import.ShowDialog();

            return;
            string filename = @"C:\AirwayVE\[CHEN-LIANG][2015-08-25-13-49-40][BJ-ChaoYang-Hospital_GX][zlf-PC]\1.2.840.113619.2.334.3.1678396440.613.1440368814.624.4\1.2.840.113619.2.334.3.1678396440.613.1440368814.624.4.zip";

            UploadProgressWndMA upw = new UploadProgressWndMA();
            //upw.Owner = LungCare_Airway_PlanNav.MainWindow.Instance;

            string InstitutionName = "Zhongshan Hospital,Fudan Univ.";
            string PatientName = "Gan^ Fuming";
            string PatientAge = "062Y";
            string PatientSex = "M";
            string SeriesInstanceUID = "1.2.840.113619.2.55.3.269126727.31.1438774442.877.3";
            string StudyInstanceUID = "1.2.840.113619.2.55.3.269126727.31.1438774442.873";
            string acquisitionDate = "20150807";
            string acquisitionTime = "130902";
            upw.FileName = filename;
            upw.InstitutionName = InstitutionName;
            upw.PatientAge = PatientAge;
            upw.PatientName = PatientName;
            upw.PatientSex = PatientSex;
            upw.SeriesInstanceUID = SeriesInstanceUID;
            upw.StudyInstanceUID = StudyInstanceUID;
            upw.acquisitionDate = acquisitionDate;
            upw.acquisitionTime = acquisitionTime;

            upw.ShowDialog();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            DownloadListWindow1 downloadWindow = new DownloadListWindow1();
            downloadWindow.ShowDialog();
        }
        private void btnDeletePatient_Click(object sender, EventArgs e)
        {
            //SoundPlayerController.Play出错();

            AirwayPatient selectedAirwayPatient = this.SelectedPatient;
            if(selectedAirwayPatient==null){
                MessageBox.Show("删除数据不能为空");
                return;
            }
            if (MessageBox.Show(@"是否删除病人 """ + selectedAirwayPatient.Name + @"""?", @"删除病人", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                AirwayPatients all = AirwayPatients.Deserialize();
                AirwayPatient patient2BeDeleted = all[dataGridViewPatient.SelectedRows[0].Index];
                patient2BeDeleted.DeleteFiles();

                all.RemoveAt(dataGridViewPatient.SelectedRows[0].Index);
                AirwayPatients.Serialize(all);
                ReloadPatientDataGridView();

                MessageBox.Show(string.Format("病人{0}已删除。", selectedAirwayPatient.Name));
            }
        }

        private void 虚拟胸片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //虚拟胸片Form 虚拟胸片Form = new 虚拟胸片Form { AirwayPatient = SelectedPatient };
            //虚拟胸片Form.ShowDialog();
        }

        private void btnImportData_Click(object sender, EventArgs e)
        {
            Process.Start(@"C:\app\Browser\AirwayCT.exe", "ImportData 1").WaitForExit();
            ReloadPatientDataGridView();
            GotoTop();
        }

        private void pickRegistrationPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //RegistrationPathPickForm form = new RegistrationPathPickForm
            //{
            //    AirwayPatient = SelectedPatient,
            //};
            //form.ShowDialog();
        }

        private void 缩小原始MHDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            vtkImageData imageData = this.SelectedPatient.吸气末期MhdFileName.ReadMetaImage();
            //vtkImageData segmentedImageData = this.SelectedPatient.SegmentedMhd_FileName.ReadMetaImage();

            int[] dims = imageData.GetDimensions();

            int val2 = 400;

            //imageData = VTKUtil.ResizeImage(imageData, dims[0], dims[1], Math.Min(dims[2], val2));
            //segmentedImageData = VTKUtil.ResizeImage(segmentedImageData, dims[0], dims[1], Math.Min(dims[2], val2));

            dims = imageData.GetDimensions();

            //for (int x = 0; x < dims[0]; ++x)
            //{
            //    Console.WriteLine(string.Format("x = {0}", x));
            //    for (int y = 0; y < dims[1]; ++y)
            //    {
            //        for (int z = 0; z < dims[2]; ++z)
            //        {
            //            if (segmentedImageData.GetScalarComponentAsDouble(x, y, z, 0) > 0)
            //            {
            //                double componentAsDouble = imageData.GetScalarComponentAsDouble(x, y, z, 0);
            //                imageData.SetScalarComponentFromDouble(x, y, z, 0, componentAsDouble + 2000);
            //            }
            //        }
            //    }
            //}

            vtkImageData resized = VTKUtil.ResizeImage(imageData, dims[0], dims[1], Math.Min(dims[2], val2));

            //VTKUtil.WriteMhd(resized, this.SelectedPatient.GetFile("raw_small.mha"));
            VTKUtil.WriteMhd(resized, SelectedPatient.吸气末期MhdFileName);

            MessageBox.Show(@"DONE");
        }

        private void rib2D3DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var rib2D3DRegistrationForm = new Rib2D3DRegistrationForm();
            //rib2D3DRegistrationForm.ShowDialog();
        }

        private void 强制刷新所有数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(this.SelectedPatient.SegmentedMhd_FileName) { LastWriteTime = DateTime.Now };

            MessageBox.Show(@"重置完成。点击下一步后开始刷新数据。");
        }

        private void 处理数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedPatient.SegmentedMhd_FileName.FileExists())
            {
                if (MessageBox.Show(@"已有规划数据，重新处理将会删除所有当前数据。是否继续？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk,
                        MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    处理数据();
                    MessageBox.Show(@"DONE");
                }
                else
                {
                    MessageBox.Show(@"已取消数据处理");
                }
            }
        }

        private void 处理数据()
        {
            //double[] proximalPoint = SelectedPatient.ProximalPoint;

            //if (!SelectedPatient.吸气末期MhdFileName.FileExists())
            //    LungCare.SegmentationAlgorithm.AutoRegionGrowSegmentationAlgorithmAPI.Dicom2MetaImage(this.SelectedPatient);

            //if (SelectedPatient.吸气末期MhdFileName.FileExists() &&
            //    SelectedPatient.吸气末期MhdFileName.ReadMetaImageSpacing()[2] == 0)
            //{
            //    MessageBox.Show(@"Invalid data. Slice thickness = 0!");

            //    return;
            //}

            //string iRawMHA = SelectedPatient.GetFile("raw_median3D.mha").FileExists() ? SelectedPatient.GetFile("raw_median3D.mha") : SelectedPatient.吸气末期MhdFileName;
            //string oSegmentationMetaImageFName = SelectedPatient.SegmentedMhd_FileName;

            //if (DoSimpleSegmentation(iRawMHA, oSegmentationMetaImageFName, proximalPoint)) return;

            //return;
            //var autoRegionGrowSegmentationAlgorithmApi = new AutoRegionGrowSegmentationAlgorithmAPI();
            //autoRegionGrowSegmentationAlgorithmApi.Load(SelectedPatient.吸气末期MhdFileName);
            //autoRegionGrowSegmentationAlgorithmApi.Do(
            //    (int)proximalPoint[0],
            //    (int)proximalPoint[1],
            //    (int)proximalPoint[2],
            //    //SelectedPatient.GetFile("abc.mha")
            //    oSegmentationMetaImageFName);
        }

        public static bool DoSimpleSegmentation(string iRawMHA, string oSegmentationMetaImageFName, double[] proximalPoint)
        {
            GC.Collect();

            string SegmentationAlgorithmEXEPath = @"C:\App\Segmentation\SegmentationAlgorithm.exe";

            if (!SegmentationAlgorithmEXEPath.FileExists())
            {
                MessageBox.Show(SegmentationAlgorithmEXEPath + @" is missing.");
                return true;
            }


            Process.Start(SegmentationAlgorithmEXEPath,
                string.Format("simpleseg {0} \"{1}\" \"{2}\" {3} {4}", iRawMHA,
                    oSegmentationMetaImageFName, proximalPoint[0], proximalPoint[1], proximalPoint[2])).WaitForExit();
            return false;
        }

        private void 处理数据_手动()
        {
            double[] proximalPoint = SelectedPatient.ProximalPoint;

            //if (!SelectedPatient.吸气末期MhdFileName.FileExists())
            //    LungCare.SegmentationAlgorithm.AutoRegionGrowSegmentationAlgorithmAPI.Dicom2MetaImage(this.SelectedPatient);

            GC.Collect();

            string SegmentationAlgorithmEXEPath = @"C:\App\Segmentation\SegmentationAlgorithm.exe";

            if (!SegmentationAlgorithmEXEPath.FileExists())
            {
                MessageBox.Show(SegmentationAlgorithmEXEPath + @" is missing.");
                return;
            }

            Process.Start(SegmentationAlgorithmEXEPath, string.Format("manualSeg {0}", SelectedPatient.GetFile(""))).WaitForExit();

            return;
            //var autoRegionGrowSegmentationAlgorithmApi = new AutoRegionGrowSegmentationAlgorithmAPI();
            //autoRegionGrowSegmentationAlgorithmApi.Load(SelectedPatient.吸气末期MhdFileName);
            //autoRegionGrowSegmentationAlgorithmApi.Do(
            //    (int)proximalPoint[0],
            //    (int)proximalPoint[1],
            //    (int)proximalPoint[2],
            //    //SelectedPatient.GetFile("abc.mha")
            //    SelectedPatient.SegmentedMhd_FileName);
        }

        private void dataGridViewPatient_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (com.jxdw.helper.Helper.ShiftStat)
            {
                Process.Start(new FileInfo(SelectedPatient.AirwayVTP_FileName).Directory.FullName);
            }
            else if (com.jxdw.helper.Helper.ControlStat)
            {
                Process.Start(new FileInfo(SelectedPatient.DicomFolder).Directory.FullName);
            }
        }

        private void btnSimpleView_Click(object sender, EventArgs e)
        {
            Process.Start(@"C:\app\Browser\AirwayCT.exe", "View \"" + SelectedPatient.PatientId + "\"").WaitForExit();
        }

        private void btnVR_Click(object sender, EventArgs e)
        {
            Process.Start(@"C:\app\Browser\AirwayCT.exe", "VR \"" + SelectedPatient.PatientId + "\"").WaitForExit();
        }

        private void 导入segmentmhaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "MetaImage MHA files (*.mha)|*.mha";
            ofd.FileName = Clipboard.GetText();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    vtkImageData imageData = ofd.FileName.ReadMetaImage();

                    if (imageData.GetDimensions()[0] == 1)
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    MessageBox.Show(@"选中文件不是合法MetaImage。" + ofd.FileName);
                    return;
                }

                if (SelectedPatient.SegmentedMhd_FileName.FileExists())
                {
                    File.Move(SelectedPatient.SegmentedMhd_FileName, SelectedPatient.SegmentedMhd_FileName + DateTime.Now.Ticks + ".bak");
                    File.Copy(ofd.FileName, SelectedPatient.SegmentedMhd_FileName);
                    MessageBox.Show(@"导入完毕。");
                }
            }
        }

        private void 导入lesionvtpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "XML PolyData files (*.vtp)|*.vtp";
            ofd.FileName = Clipboard.GetText();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    vtkPolyData imageData = ofd.FileName.ReadPolyData();

                    if (imageData.GetNumberOfPoints() == 0)
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    MessageBox.Show(@"选中文件不是合法XML PolyData。" + ofd.FileName);
                    return;
                }

                string lesion0fixedVTPFileName = SelectedPatient.GetFile("lesion.0.fixed.vtp");

                if (lesion0fixedVTPFileName.FileExists())
                {
                    File.Move(lesion0fixedVTPFileName, lesion0fixedVTPFileName + "_" + DateTime.Now.Ticks + ".bak");
                }

                File.Copy(ofd.FileName, lesion0fixedVTPFileName);
                MessageBox.Show(@"导入完毕。");
            }
        }

        private void btn导出_除去Dicom_Click(object sender, EventArgs e)
        {
            if (!@"C:\Program Files\2345Soft\HaoZip\HaoZipC.exe".FileExists())
            {
                MessageBox.Show(@"请安装haozip。http://haozip.2345.com/");
                return;
            }

            if (!SelectedRowIndex.HasValue)
            {
                return;
            }

            string fileName = string.Format("[{0}][{1}][{2}][{3}]_NoDICOM.zip", SelectedPatient.Name, SelectedPatient.Institution, System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            fileName = fileName.Replace(' ', '-');

            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = fileName,
                //FileName = string.Format("{0}_No_DICOM.zip", SelectedPatient.PatientId),
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string airwayPatientFileName = Path.Combine(Path.GetTempPath(), "airwaypatient.xml");
                SelectedPatient.Save(airwayPatientFileName);

                // "C:\Program Files\2345Soft\HaoZip\HaoZipC.exe" a -tzip c:/a.zip I:\AirwayVE\Db\ZHANGYONG_M_20145693114_bfd3d7a2adbf425ba8a4497917bac0f1
                string args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\"", sfd.FileName, SelectedPatient.GetFile(""), airwayPatientFileName);
                var psi = new ProcessStartInfo(@"C:\Program Files\2345Soft\HaoZip\HaoZipC.exe", args);
                Process.Start(psi).WaitForExit();
                MessageBox.Show(string.Format(@"压缩完毕。
文件保存到{0}。", sfd.FileName));
                Process.Start(new FileInfo(sfd.FileName).Directory.FullName);
            }
        }

        private void 系统配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //new LungCareConfigurationApp.Form1().ShowDialog();
        }

        //private void tbFrame_Scroll(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        string dicomFile = Path.Combine(SelectedPatient.DicomFolder, (tbFrame.Value + 1).ToString());

        //        Dicom.Imaging.DicomImage dicomImage = new Dicom.Imaging.DicomImage(dicomFile)
        //        {
        //            WindowCenter = -500,
        //            WindowWidth = 1400
        //        };

        //        Image renderImage = dicomImage.RenderImage();
        //        Graphics g = Graphics.FromImage(renderImage);

        //        StringBuilder sb = new StringBuilder();

        //        DicomDataset ds = dicomImage.Dataset;

        //        sb.AppendLine(ds.Get<string>(DicomTag.PatientName) + " ( " + ds.Get<string>(DicomTag.PatientAge) + " )");
        //        sb.AppendLine(ds.Get<string>(DicomTag.SeriesDescription) + " -- " +
        //                      ds.Get<string>(DicomTag.StudyDescription));
        //        sb.AppendLine("Thickness: " + ds.Get<string>(DicomTag.SliceThickness) + "mm");
        //        //sb.AppendLine("Location : " + ds[DicomTags.SliceLocation] + "mm");
        //        sb.AppendLine("Sex: " + ds.Get<string>(DicomTag.PatientSex));
        //        sb.AppendLine(ds.Get<string>(DicomTag.AcquisitionDate) + " " + ds.Get<string>(DicomTag.AcquisitionTime));
        //        //sb.AppendLine(string.Format("Im: {0}/{1}", ds[DicomTags.InstanceNumber], trackBarIdx.Maximum + 1));

        //        //pictureBox1.Tag = sb.ToString();
        //        g.DrawString(sb.ToString(), new Font("Arial", 12), Brushes.White, 10, 10);

        //        if (SelectedPatient.Airway3DScreenshotFileName.FileExists())
        //        {
        //            Bitmap bmp = new Bitmap(SelectedPatient.Airway3DScreenshotFileName);
        //            bmp.MakeTransparent(Color.Black);

        //            g.DrawImage(bmp, -50, renderImage.Height - 200, 250, 250);
        //        }

        //        pbCTImage.Image = renderImage;
        //        pbCTImage.Refresh();
        //    }
        //    catch
        //    {
        //    }
        //}

        private void 高级数据处理慢ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.BackColor = Color.FromArgb(200, Color.SkyBlue);
            label1.Visible = true;
            label1.BringToFront();
            Application.DoEvents();

            //AirwayDataProcessMainServiceClient airwayDataProcessMainServiceClient = new AirwayDataProcessMainServiceClient();
            //airwayDataProcessMainServiceClient.DoAll(SelectedPatient.PatientId);

            Enabled = true;
            Cursor = Cursors.Arrow;
            Close();
        }

        private void smooth1次ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (!SelectedPatient.吸气末期MhdFileName.FileExists())
            //    LungCare.SegmentationAlgorithm.AutoRegionGrowSegmentationAlgorithmAPI.Dicom2MetaImage(this.SelectedPatient);

            string args = string.Format("smooth \"{0}\" \"{1}\" {2}", SelectedPatient.吸气末期MhdFileName,
                SelectedPatient.GetFile("raw_median3D.mha"), 5);
            Console.WriteLine(args);
            Process.Start(@"C:\App\Segmentation\SegmentationAlgorithm.exe", args).WaitForExit();

            MessageBox.Show(@"平滑完毕");
            return;
            //vtkImageData imageData = SelectedPatient.吸气末期MhdFileName.ReadMetaImage();

            //for (int i = 0; i < 6; ++i)
            //{
            //    Console.WriteLine(i);

            //    Kitware.VTK.vtkImageMedian3D median3D = vtkImageMedian3D.New();

            //    median3D.SetInput(imageData);
            //    median3D.SetKernelSize(3, 3, 1);
            //    median3D.Update();

            //    imageData.Dispose();

            //    imageData = median3D.GetOutput();
            //}

            //VTKUtil.WriteMhd(imageData, SelectedPatient.GetFile("raw_median3D.mha"));
        }

        private void 不SmoothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedPatient.GetFile("raw_median3D.mha").FileExists())
            {
                File.Delete(SelectedPatient.GetFile("raw_median3D.mha"));
            }

            MessageBox.Show(@"DONE");
        }

        private void smoothX10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (!SelectedPatient.吸气末期MhdFileName.FileExists())
            //    LungCare.SegmentationAlgorithm.AutoRegionGrowSegmentationAlgorithmAPI.Dicom2MetaImage(this.SelectedPatient);

            string args = string.Format("smooth \"{0}\" \"{1}\" {2}", SelectedPatient.吸气末期MhdFileName,
                SelectedPatient.GetFile("raw_median3D.mha"), 10);
            Console.WriteLine(args);
            Process.Start(@"C:\App\Segmentation\SegmentationAlgorithm.exe", args).WaitForExit();

            MessageBox.Show(@"平滑完毕");
            return;
        }

        private void 处理数据手动ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            处理数据_手动();
            MessageBox.Show(@"DONE");
        }

        private void hessianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            {
                string ifile = SelectedPatient.吸气末期MhdFileName;
                string ofile = SelectedPatient.GetFile("Invert.mha");
            }
            //    vtkImageData hessianF = ifile.ReadMetaImage();
            //    int[] dims = hessianF.GetDimensions();

            //    ParallelOptions pOption = new ParallelOptions { MaxDegreeOfParallelism = 8 };
            //    // 设置最大线程数

            //    Parallel.For(0, dims[0], pOption, x =>
            //    //for (int x = 0; x < dims[0]; ++x)
            //    {
            //        Console.WriteLine(x);

            //        for (int y = 0; y < dims[1]; ++y)
            //        {
            //            for (int z = 0; z < dims[2]; ++z)
            //            {
            //                double scalarComponentAsDouble = hessianF.GetScalarComponentAsDouble(x, y, z, 0);
            //                if (scalarComponentAsDouble >= -300)
            //                {
            //                    scalarComponentAsDouble = -300;
            //                }

            //                hessianF.SetScalarComponentFromDouble(x, y, z, 0, -scalarComponentAsDouble);
            //            }
            //        }
            //    });

            //    VTKUtil.WriteMhd(hessianF, ofile);

            //    hessianF.Dispose();

            //    VMTKUtil.DoHessian(ofile, SelectedPatient.GetFile("hessian.mha"));
            //}

            //{
            //    string ifile = SelectedPatient.GetFile("hessian.mha");
            //    string ofile = SelectedPatient.GetFile("hessian_c.mha");

            //    vtkImageData hessianF = ifile.ReadMetaImage();
            //    int[] dims = hessianF.GetDimensions();
            //    double[] spacing = hessianF.GetSpacing();

            //    vtkImageData hessianC = vtkImageData.New();
            //    hessianC.SetDimensions(dims[0], dims[1], dims[2]);
            //    hessianC.SetSpacing(spacing[0], spacing[1], spacing[2]);
            //    hessianC.SetScalarTypeToUnsignedChar();

            //    ParallelOptions pOption = new ParallelOptions { MaxDegreeOfParallelism = 8 };
            //    // 设置最大线程数
            //    //for (int x = 0; x < dims[0]; ++x)

            //    Parallel.For(0, dims[0], pOption, x =>
            //    {
            //        Console.WriteLine(x);

            //        for (int y = 0; y < dims[1]; ++y)
            //        {
            //            for (int z = 0; z < dims[2]; ++z)
            //            {
            //                hessianC.SetScalarComponentFromDouble(x, y, z, 0,
            //                    hessianF.GetScalarComponentAsDouble(x, y, z, 0) > 0 ? 255 : 0);
            //            }
            //        }
            //    });

            //    VTKUtil.WriteMhd(hessianC, ofile);

            //    hessianF.Dispose();
            //    hessianC.Dispose();
            //}

            //Console.Beep();
            //MessageBox.Show(@"DONE");
        }

        private void vCAPReplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
        //    WindowState = FormWindowState.Minimized;

        //    VCAPReplayForm VCAPReplayForm = new VCAPReplayForm();
        //    VCAPReplayForm.ShowDialog();

        //    WindowState = FormWindowState.Normal;
        }

        private void nDIReplayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Cursor = Cursors.WaitCursor;
            //new LungCare.Airway.Replayer.NDIText2NDIPoints().ShowDialog();
            //Cursor = Cursors.Arrow;
            //return;
        }

        private void 导出segmentMHAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!SelectedRowIndex.HasValue)
            {
                return;
            }

            string fileName = string.Format("[{0}][{1}][{2}]_refine_airway_segment.mha", SelectedPatient.Name, System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            fileName = fileName.Replace(' ', '-');

            //SaveFileDialog sfd = new SaveFileDialog
            //{
            //    FileName = fileName,
            //    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            //};

            //if (sfd.ShowDialog() == DialogResult.OK)
            //{
            //    File.Copy(SelectedPatient.SegmentedMhd_FileName, sfd.FileName, true);

            //    MessageBox.Show(string.Format(@"分割文件保存到{0}。", sfd.FileName));
            //    Process.Start(new FileInfo(sfd.FileName).Directory.FullName);
            //}
        }

        private void 导出术中数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!@"C:\Program Files\2345Soft\HaoZip\HaoZipC.exe".FileExists())
            {
                MessageBox.Show(@"请安装haozip。http://haozip.2345.com/");
                return;
            }

            if (!SelectedRowIndex.HasValue)
            {
                return;
            }

            string fileName = string.Format("[{0}][{1}][{2}][{3}]_Surgery.zip", SelectedPatient.Name, SelectedPatient.Institution, System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            fileName = fileName.Replace(' ', '-');

            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = fileName,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string airwayPatientFileName = Path.Combine(Path.GetTempPath(), "airwaypatient.xml");
                SelectedPatient.Save(airwayPatientFileName);

                // "C:\Program Files\2345Soft\HaoZip\HaoZipC.exe" a -tzip c:/a.zip I:\AirwayVE\Db\ZHANGYONG_M_20145693114_bfd3d7a2adbf425ba8a4497917bac0f1
//                string args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\" \"{5}\" \"{6}\" \"{7}\" \"{8}\"", sfd.FileName,
//                    SelectedPatient.GetFile(""),
//                    airwayPatientFileName,
//                    "C:/ndi",
//                    "C:/NDI_Endoscopy",
//                    "C:/NDI_XML",
//                    "C:/VCAP",
//                    @"C:\Program Files\tlxsoft\屏幕录像专家 共享版\ls",
//                    SelectedPatient.InSurgeryDataFolder);
//                args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\" \"{5}\" \"{6}\"", sfd.FileName,
//                                SelectedPatient.GetFile(""),
//                                airwayPatientFileName,
//                                "C:/ndi",
//                                "C:/NDI_Endoscopy",
//                                "C:/NDI_XML",
//                                //"C:/VCAP",
//                                //@"C:\Program Files\tlxsoft\屏幕录像专家 共享版\ls",
//                                SelectedPatient.InSurgeryDataFolder);

//                var psi = new ProcessStartInfo(@"C:\Program Files\2345Soft\HaoZip\HaoZipC.exe", args);
//                Process.Start(psi).WaitForExit();
//                MessageBox.Show(string.Format(@"压缩完毕。
//文件保存到{0}。", sfd.FileName));
//                Process.Start(new FileInfo(sfd.FileName).Directory.FullName);
            }
        }

        private void vCAPReplayDualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;

            //DualVCAPReplayForm VCAPReplayForm = new DualVCAPReplayForm();
            //VCAPReplayForm.ShowDialog();

            WindowState = FormWindowState.Normal;
        }

        private void btn选择配准范围_Click(object sender, EventArgs e)
        {
            //NavRangePickForm form = new NavRangePickForm
            //{
            //    AirwayPatient = SelectedPatient,
            //};
            //form.ShowDialog();
        }

        private bool _showXRay = true;
        private void virtualXRayViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Switch2XRayView();
        }

        private void Switch2XRayView()
        {
            _showXRay = true;
            pbCTImage.Visible = true;
            panelVTK.Visible = false;
        }

        private void airway3DViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Switch2Airway3DView();
        }

        private void Switch2Airway3DView()
        {
            _showXRay = false;
            pbCTImage.Visible = false;
            panelVTK.Visible = true;
        }

        private void pbCTImage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Switch2Airway3DView();
            Update3D();
        }

        private void pbCTImage_Paint(object sender, PaintEventArgs e)
        {

            if (SelectedPatient!=null && !string.IsNullOrEmpty(SelectedPatient.Name) && SelectedPatient.GetFile("dcm.txt").FileExists())
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                FontFamily fontFamily = new FontFamily("Arial");
                StringFormat strformat = new StringFormat();
                string szbuf = File.ReadAllText(SelectedPatient.GetFile("dcm.txt"));

                if (SelectedPatient.GetFile("vcap.txt").FileExists())
                {
                    szbuf += Environment.NewLine + File.ReadAllText(SelectedPatient.GetFile("vcap.txt"));
                }

                GraphicsPath path = new GraphicsPath();
                path.AddString(szbuf, fontFamily, (int)FontStyle.Regular, 18.0f, new Point(10, 10), strformat);
                Pen pen = new Pen(Color.FromArgb(255, 255, 255), 4);
                pen.LineJoin = LineJoin.Round;
                e.Graphics.DrawPath(pen, path);
                SolidBrush brush = new SolidBrush(Color.FromArgb(0, 0, 0));
                e.Graphics.FillPath(brush, path);

            }
        }

        private void screenshot虚拟胸片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bmp = ScreenShot.CaptureImage(
                pbCTImage.PointToScreen(new Point(0, 0)),
                pbCTImage.PointToScreen(new Point(pbCTImage.Width, pbCTImage.Height)),
                new Rectangle(0, 0, pbCTImage.Width, pbCTImage.Height));
            Clipboard.SetImage(bmp);
            MessageBox.Show(@"DONE");
        }

        private void btn虚拟胸片2_Click(object sender, EventArgs e)
        {
            
        }

        private void btn中山_Click(object sender, EventArgs e)
        {
            btn中山.Checked = true;
            btn胸科.Checked = false;
            btn全部.Checked = false;

            AirwayPatients all = AirwayPatients.Deserialize();
            for (int index = 0; index < all.Count; index++)
            {
                var patient = all[index];
                if (patient.Institution != null && patient.Institution.ToUpper().Contains("CHEST"))
                {
                    dataGridViewPatient.Rows[index].Height = 0;
                }
            }

            Refresh();
        }

        private void btn胸科_Click(object sender, EventArgs e)
        {

            btn中山.Checked = false;
            btn胸科.Checked = true;
            btn全部.Checked = false;
        }

        private void btn全部_Click(object sender, EventArgs e)
        {
            btn中山.Checked = false;
            btn胸科.Checked = false;
            btn全部.Checked = true;
        }

        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(@"Upload");
        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(@"Download");
        }

        private void 改名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles(SelectedPatient.DicomFolder);

            foreach (string file in files)
            {
                Console.WriteLine(file);
                DicomFile df = DicomFile.Open(file);
                df.Dataset.Remove(DicomTag.PatientName);
                df.Dataset.Add(DicomTag.PatientName, "XZK");
                df.Save(@"I:\zlfLabData\Dicom\Ma 2014-09-15\XZK_Rename\" + new FileInfo(file).Name);

                return;
            }
        }

        private void 需要ColorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            需要ColorToolStripMenuItem1.Checked = true;
            不需要ColorToolStripMenuItem.Checked = false;

            AirwayPatient patient = SelectedPatient;
            patient.NotRequireLabeling = false;
            patient.Save();
        }

        private void 不需要ColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            需要ColorToolStripMenuItem1.Checked = false;
            不需要ColorToolStripMenuItem.Checked = true;

            AirwayPatient patient = SelectedPatient;
            patient.NotRequireLabeling = true;
            patient.Save();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void vR2JPGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void dcm2JPGToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void dcm2JPGAllPatientsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AirwayPatients all = AirwayPatients.Deserialize();

            foreach (AirwayPatient patient in all)
            {
                if (patient.Institution != null && patient.Institution.ToLower().Contains("chest"))
                {
                    continue;
                }

              //  LungCare.Algorithm.Patient2JPG.Export(patient);
            }
        }

        private void webGLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            vtkPolyData polyData = SelectedPatient.AirwayVTP_FileName.ReadPolyData();


            vtkTriangleFilter triangleFilter = vtkTriangleFilter.New();
            triangleFilter.SetInput(polyData);
            triangleFilter.Update();

            vtkPolyData triangled = triangleFilter.GetOutput();

            vtkDecimatePro decimate = vtkDecimatePro.New();
            decimate.SetInput(triangled);
            decimate.Update();

            triangled = decimate.GetOutput();

            //int numberOfCells = triangled.GetNumberOfCells();
            ////numberOfCells = 2376/(3*3);

            //System.Text.StringBuilder vertexPositionsSB = new System.Text.StringBuilder();
            //System.Text.StringBuilder vertexNormalsSB = new System.Text.StringBuilder();
            //System.Text.StringBuilder vertexTextureCoordsSB = new System.Text.StringBuilder();
            //System.Text.StringBuilder indicesSB = new System.Text.StringBuilder();

            //Random rnd = new Random();

            //vtkDataArray normals = triangled.GetPointData().GetNormals();
            //vtkPoints allPoints = triangled.GetPoints();

            //for (int i = 0; i < numberOfCells; ++i)
            //{
            //    vtkCell cell = triangled.GetCell(i);
            //    vtkPoints points = cell.GetPoints();

            //    int numberOfPoints = cell.GetNumberOfPoints();

            //    for (int n = 0; n < numberOfPoints; ++n)
            //    {
            //        double[] point = points.GetPoint(n);

            //        for (int x = 0; x < allPoints.GetNumberOfPoints(); ++x)
            //        {
            //            double[] point2Compare = allPoints.GetPoint(x);

            //            if (point[0] == point2Compare[0] && point[1] == point2Compare[1] && point[2] == point2Compare[2])
            //            {
            //                double[] normal = normals.GetTuple3(x);
            //                string normalLine = string.Format("{0:F1}, {1:F1}, {2:F1},", normal[0], normal[1], normal[2]);
            //                vertexNormalsSB.Append(normalLine);
            //                break;
            //            }
            //        }

            //        point[0] -= triangled.GetCenter()[0];
            //        point[1] -= triangled.GetCenter()[1];
            //        point[2] -= triangled.GetCenter()[2];

            //        string line = string.Format("{0:F1}, {1:F1}, {2:F1},", point[0], point[1], point[2]);
            //        vertexPositionsSB.Append(line);
            //        vertexTextureCoordsSB.Append(rnd.NextDouble().ToString("F1") + ","+ rnd.NextDouble().ToString("F1") + ",");
            //    }

            //    indicesSB.Append(string.Format("{0},{1},{2},", i * 3, i * 3 + 1, i * 3 + 2));
            //}

            //vertexPositionsSB.Remove(vertexPositionsSB.Length - 1, 1);
            //vertexNormalsSB.Remove(vertexNormalsSB.Length - 1, 1);
            //vertexTextureCoordsSB.Remove(vertexTextureCoordsSB.Length - 1, 1);
            //indicesSB.Remove(indicesSB.Length - 1, 1);

            //System.Text.StringBuilder resultSB = new System.Text.StringBuilder();

            //resultSB.AppendLine("{");

            //resultSB.AppendLine(string.Format(@"""vertexPositions"" : [{0}],", vertexPositionsSB.ToString()));
            //resultSB.AppendLine(string.Format(@"""vertexNormals"" : [{0}],", vertexNormalsSB.ToString()));
            //resultSB.AppendLine(string.Format(@"""vertexTextureCoords"" : [{0}],", vertexTextureCoordsSB.ToString()));
            //resultSB.AppendLine(string.Format(@"""indices"" : [{0}]", indicesSB.ToString()));

            //resultSB.AppendLine("}");

            //File.WriteAllText(@"I:\zlfLab\zsenb\zsenbWeb\WebApplication9\lesson14\airway.json", resultSB.ToString());

            //MessageBox.Show("DONE");

            //return;

            //System.Text.StringBuilder sb = new System.Text.StringBuilder();

            //for (int i = 0; i < numberOfCells; ++i)
            //{
            //    vtkCell cell = triangled.GetCell(i);
            //    int numberOfPoints = cell.GetNumberOfPoints();
            //    vtkPoints points = cell.GetPoints();

            //    for (int n = 0; n < numberOfPoints; ++n)
            //    {
            //        double[] point = points.GetPoint(n);
            //        string line = string.Format("{0:F1}, {1:F1}, {2:F1},", point[0], point[1], point[2]);
            //        sb.AppendLine(line);
            //        Console.WriteLine(line);
            //    }

            //    //Console.WriteLine(string.Format("{0}/{1} : {2}", i, numberOfCells, numberOfPoints));
            //}
            //Console.WriteLine("numberOfCells = " + numberOfCells);

            //Clipboard.SetText(sb.ToString());
        }

        private void 查看帮助ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //HelpWindow hw = new HelpWindow();
            //hw.Show();
            System.Diagnostics.Process.Start("朗开医疗手动分割软件V1.0.0.0.CHM");
        }

        private void 关于记事本ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutWindow aw = new AboutWindow();
            aw.Show();
        }

        private void btnImportCTFile_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "压缩包文件|*.zip;*rar;*.7z;*.rar5";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filename = ofd.FileName;
                if (!validateExtention(Path.GetExtension(ofd.FileName)))
                {
                    MessageBox.Show("文件格式不合格，请选择压缩包文件", "提示：");
                    return;
                }

                String guid = Guid.NewGuid().ToString();//保存上传文件的唯一文件夹

                string uploadFolder = Path.Combine("C:\\AirwayVE\\UploadFile", guid);
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string args = string.Format("x -o\"{0}\" -y -sn \"{1}\"", uploadFolder, filename);
                var psi = new ProcessStartInfo(Application.StartupPath + @"\HaoZipC.exe", args);
                Process.Start(psi).WaitForExit();

                DirectoryInfo dir = new DirectoryInfo(uploadFolder);

                string CTSaveFolder = Path.Combine("C:\\AirwayVE\\CT",guid);
                ArrayList CTFileList = new ArrayList();
                CTFileList = getAllCTPath(dir);
                for (int i = 0; i < CTFileList.Count;i++ ) {
                    //String targetPath = Path.Combine(CTSaveFolder,(i+1).ToString());
                    //File.Copy(CTFileList[i],targetPath,true);
                    Console.WriteLine(CTFileList[i]);
                }


            }
        }

        //获取所有的CT文件的路径
        public ArrayList getAllCTPath(DirectoryInfo dir) {
            ArrayList CTFileList = new ArrayList();

            FileInfo[] fileList = dir.GetFiles();
            foreach(FileInfo fi in fileList){
                if(isDicom(fi.FullName)){
                    CTFileList.Add(fi.FullName);
                }
            }
            DirectoryInfo[] dirList = dir.GetDirectories();
            foreach(DirectoryInfo di in dirList){
                getAllCTPath(di);
            }

            return CTFileList;
        }

        //判断是否是dcm文件
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


    }
}

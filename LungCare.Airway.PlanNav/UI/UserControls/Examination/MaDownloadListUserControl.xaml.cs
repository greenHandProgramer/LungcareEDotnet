using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kitware.VTK;
using LungCare.SupportPlatform.SupportPlatformDAO.VTK;
using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.UI.Windows.Common;
using LungCare.SupportPlatform.UI.Windows.Examination;
using AirwayCT.Entity;
namespace LungCare.SupportPlatform.UI.UserControls.Examination
{
    /// <summary>
    /// UploadListUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class MaDownloadListUserControl : UserControl
    {
        Models.DataListItem[] _datalist;

        public void StartLoading()
        {
            lbLoadingPrompt.Visibility = Visibility.Visible;
        }
        public void FinishLoading()
        {
            lbLoadingPrompt.Visibility = Visibility.Hidden;
        }

        public void SetDataSource(Models.DataListItem[] dataSource)
        {
            _datalist = dataSource;
            _datalist = _datalist.OrderBy(item => string.IsNullOrEmpty(item.UploadTimestamp) ? DateTime.MinValue : DateTime.ParseExact(item.UploadTimestamp, "yyyyMMdd HHmmss", null)).Reverse().ToArray();

            string text2Search = tbSearch.Text.ToLower();

            listView.DataContext =
                new System.ComponentModel.BindingList<Models.DataListItem>(
                    new List<Models.DataListItem>(
                        _datalist.Where(item => item.Status == "处理完成" && item.PatientName != null && (item.PatientName.ToLower().Contains(text2Search)|| item.DataID.ToLower().Contains(text2Search)))));
            //listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
            listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
        }

        public MaDownloadListUserControl()
        {
            InitializeComponent();

            btnCancelDownload.Visibility = lbWaiting.Visibility = Visibility.Hidden;
        }


        private List<FileListItem> _adminFileList;
        private void GetAdminFiles()
        {

            MESPDownloadUpload.UserId = _userID;
            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = _dataID;
            try
            {

                ThreadPool.QueueUserWorkItem(delegate
                {
                    new MESPDownloadUpload().FetchFileListAsync(
                        云数据类型Enum.处理结果,
                        new EventHandler<ExceptionArgs>(
                            delegate(Object senderInner, ExceptionArgs eInner)
                            {
                                MessageBox.Show("未找到文件!");
                            }),
                        new EventHandler<FileListFinishedArgs>(delegate(Object senderInner, FileListFinishedArgs eInner)
                        {
                           Dispatcher.BeginInvoke(new Action(delegate()
                            {
                                _adminFileList = eInner.Result;
                            }
                            ));
                        }));
                }
                );
            }catch(Exception ex){
                MessageBox.Show(ex.ToString());
            }
        }


        private String _userID;
        private String _dataID;

        private void linkLabel_Click(object sender, RoutedEventArgs e)
        {
            // 判断是否上传
            ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
            string DataID = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<Models.DataListItem> list = listView.Items.SourceCollection.Cast<Models.DataListItem>();
            var items = list.First(item => ((Models.DataListItem)item).DataID == DataID);

            _userID = "15261595318";
            _dataID = items.DataID;


            MESPDownloadUpload.UserId = _userID;
            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = _dataID;
            try
            {

                ThreadPool.QueueUserWorkItem(delegate
                {
                    new MESPDownloadUpload().FetchFileListAsync(
                        云数据类型Enum.处理结果,
                        new EventHandler<ExceptionArgs>(
                            delegate(Object senderInner, ExceptionArgs eInner)
                            {
                                MessageBox.Show("未找到文件!");
                            }),
                        new EventHandler<FileListFinishedArgs>(delegate(Object senderInner, FileListFinishedArgs eInner)
                        {
                            Dispatcher.BeginInvoke(new Action(delegate()
                            {
                                _adminFileList = eInner.Result;
                                FileListItem selectedFile = _adminFileList[0];

                                lbWaiting.Content = "正在下载，请稍候。";
                                lbWaiting.Visibility = Visibility.Visible;

                                MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = items.DataID;
                                string filename = @"C:\AirwayVE\CT\" + selectedFile.FileName;


                                string selectedDicomPackageLocalPath = "";
                                try
                                {
                                    selectedDicomPackageLocalPath =
                                    new FileInfo(filename).FullName;
                                }
                                catch (Exception)
                                {
                                    filename = @"C:\AirwayVE\CT\" + items.PatientName + ".zip";
                                    selectedDicomPackageLocalPath =
                                   new FileInfo(filename).FullName;
                                }

                                PauseGUI();
                                ThreadPool.QueueUserWorkItem(delegate
                                {
                                    Download(selectedFile, selectedDicomPackageLocalPath, 云数据类型Enum.处理结果);
                                });
                            }
                             ));
                        }));
                }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        private void PauseGUI()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                ((UIElement)Content).IsEnabled = false;
                Cursor = Cursors.Wait;
            }));
        }
        private void ResumeGUI()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                ((UIElement)Content).IsEnabled = true;
                Cursor = Cursors.Arrow;
            }));
        }
        private void Download(FileListItem selectedFileItem, string selectedDicomPackageLocalPath, 云数据类型Enum 云数据类型Enum)
        {

            DownloadFileWorker.Download(
                selectedFileItem,
                selectedDicomPackageLocalPath,
                delegate(string filename)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        try
                        {
                            lbWaiting.Visibility = Visibility.Hidden;
                            ResumeGUI();
                            while (!File.Exists(selectedDicomPackageLocalPath)) { 
                            }
                            //Thread.Sleep(1000);
                            openPatientsMsgWindow(selectedDicomPackageLocalPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    });
                },
                delegate(string errMsg)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        MessageBox.Show(errMsg);
                        ResumeGUI();
                    });
                },
                delegate(Exception ex)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        Util.ShowExceptionMessage(ex);
                        //Download(selectedFile, selectedDicomPackageLocalPath, 云数据类型Enum.处理结果);
                        ResumeGUI();
                    });
                },
                delegate(ProgressArgs progressArgs)
                {
                    UIUtil.Invoke(this, delegate
                    {

                        lbWaiting.Content = "正在下载，请稍候。" + progressArgs.ToString();
                        lbDownloadProgress.Content = progressArgs.ToString();

                        Console.WriteLine(progressArgs);
                    });
                }, 云数据类型Enum);
        }

        private void openPatientsMsgWindow(String fileNamePath) {
            String fileName = Path.GetFileName(fileNamePath);
            if (!validateExtention(Path.GetExtension(fileName)))
            {
                MessageBox.Show("文件格式不合格，请选择压缩包文件", "提示：");

                return;
            }
            //string folder = Path.Combine(AirPatientForm.RegistryDAO.AirwayBaseFolder, Path.GetFileNameWithoutExtension(filename).Replace(" ", ""));
            string folder = Path.Combine("C:\\AirwayVE\\DB", Path.GetFileNameWithoutExtension(fileName).Replace(" ", ""));
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string filenam1e = Path.Combine(folder, "airwaypatient.xml");
            if (!File.Exists(filenam1e))
            {

                string args = string.Format("x -o\"{0}\" -y -sn \"{1}\"", folder, fileNamePath);
                var psi = new ProcessStartInfo(System.Windows.Forms.Application.StartupPath + @"\HaoZipC.exe", args);
                Process.Start(psi).WaitForExit();
            }

            if (!File.Exists(filenam1e))
            {
                MessageBox.Show(@"压缩文件中未找到文件 : airwaypatient.xml");
                return;
            }
            
            AirwayPatient patient = AirwayPatient.Load(filenam1e);
            patient.OrderID = _dataID; 
            patient.Save(filenam1e);

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

            vtkImageData _rawCTMetaImage = patient.吸气末期MhdFileName.ReadMetaImage();

            MainWindowMA CTRefineMainForm = new MainWindowMA(patient,
                OrientationEnum.Axial,
                _rawCTMetaImage, patient.SegmentedMhd_FileName);

            CTRefineMainForm.ShowDialog();

        }



        private bool validateExtention(string extent)
        {
            return extent == ".rar" || extent == ".zip" || extent == ".7z" || extent == ".rar5";
        }


        private void btnPlayVideo_Click(object sender, RoutedEventArgs e)
        {
            new VideoPlayer(@"C:\Users\Public\Videos\Sample Videos\Wildlife.wmv").ShowDialog();
            //VideoPlayer VideoPlayer = new VideoPlayer();
            //VideoPlayer.ShowDialog();
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text2Search = tbSearch.Text.ToLower();

            if (_datalist == null || _datalist.Length == 0)
            {
                return;
            }

            listView.DataContext =
                new System.ComponentModel.BindingList<Models.DataListItem>(
                    new List<Models.DataListItem>(
                        _datalist.Where(item => item.Status == "处理完成" && item.PatientName != null && (item.PatientName.ToLower().Contains(text2Search)|| item.DataID.ToLower().Contains(text2Search)))));
            //listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
            listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
        }

        private void btnCancelDownload_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("是否取消下载？取消后支持续传您可以随时重新启动下载。", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                btnCancelDownload.Visibility = lbWaiting.Visibility = Visibility.Hidden;
                tbSearch.IsEnabled = this.listView.IsEnabled = true;
                this.Cursor = Cursors.Arrow;
                MESPDownloadUpload.CancelDownload();
            }
        }

    }
}

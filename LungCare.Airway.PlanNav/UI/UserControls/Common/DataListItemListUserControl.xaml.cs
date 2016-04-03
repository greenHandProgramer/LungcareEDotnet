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
using LungCare.SupportPlatform.Models;
using LungCare.SupportPlatform.SupportPlatformDAO.Files;
using LungCare.SupportPlatform.SupportPlatformDAO.VTK;
using LungCare.SupportPlatform.UI.Windows.Common;
using LungCare.SupportPlatform.UI.Windows.Examination;
using LungCare.SupportPlatform.Entities;
using AirwayCT.Entity;
using LungCare.SupportPlatform.SupportPlatformDAO.UI;
using LungCare.SupportPlatform.SupportPlatformDAO.LocalDicom;
using System.Text;
using LungCare.SupportPlatform.SupportPlatformDAO.Airway;
namespace LungCare.SupportPlatform.UI.UserControls.Common
{
    /// <summary>
    /// UploadListUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class DataListItemListUserControl : UserControl
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

        public event EventHandler<EventArgs> UpdateData;
        
        public void SetDataSource(Models.DataListItem[] dataSource)
        {

            DataListItemEntities localList = DataListItemEntities.TestLoad();
            if (localList != null)
            {
                DataListItem[] list = new DataListItem[dataSource.Length + localList.Count];
                for (int i = 0; i < dataSource.Length; i++)
                {
                    list[i] = dataSource[i];
                }

                for (int i = dataSource.Length; i < list.Length ; i++)
                {
                    list[i] = localList[i - dataSource.Length];
                }
                _datalist = list;
            }
            else
            {
                _datalist = dataSource;
            }


            _datalist = _datalist.OrderBy(item => string.IsNullOrEmpty(item.UploadTimestamp) ? DateTime.MinValue : DateTime.ParseExact(item.UploadTimestamp, "yyyyMMdd HHmmss", null)).Reverse().ToArray();

            
            string text2Search = tbSearch.Text.ToLower();

            listView.DataContext =
                new System.ComponentModel.BindingList<Models.DataListItem>(
                    new List<Models.DataListItem>(
                        _datalist.Where(item => item.PatientName != null && (item.PatientName.ToLower().Contains(text2Search) || item.DataID.ToLower().Contains(text2Search)))));
            //listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
            listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());

            return;
            _CurrentDataList = _datalist;
            //_CurrentPage = 0;
            if (_CurrentDataList.Count() % _PageSize == 0)
            {
                _MaxPage = _CurrentDataList.Count() / _PageSize - 1;
            }
            else
            {
                _MaxPage = _CurrentDataList.Count() / _PageSize;
            }
            checkPage(_CurrentPage, _MaxPage, _MinPage);
            LoadPage(_CurrentPage, _PageSize, _CurrentDataList, _MaxPage);
        }

        public DataListItemListUserControl()
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

        private void linkLabelDicom_Click(object sender, RoutedEventArgs e)
        {
            
            ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
            if (LinkLabel.Tag != null)
            {
                string tag = LinkLabel.Tag.ToString();
                if (tag.Contains(".")) //此条件判断绑定的是UID还是DataID
                {
                    string studyUID = tag;

                    string savedPath = LocalDicomDAO.GetLocalDicomPath(studyUID);
                    if (savedPath != null)
                    {
                        MainDataWindow.Instance.PauseGUI();
                        ImportDicomWindow3 dicomView = new ImportDicomWindow3(savedPath);
                        dicomView.ShowDialog();
                        MainDataWindow.Instance.ResumeGUI();
                        return;
                    }

                    return;
                }
            }
            string DataID = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<Models.DataListItem> list = listView.Items.SourceCollection.Cast<Models.DataListItem>();
            var dataListItem = list.FirstOrDefault(item => ((Models.DataListItem)item).DataID == DataID);
            int index = list.ToList().IndexOf(dataListItem);
            if (index >= 0)
            {
                ListViewDAO.SetCheckStatusInListView(listView, "cbSelect", false);
                ListViewDAO.SetCheckStatusInListViewItem(listView, index, "cbSelect", false);
                System.Windows.Forms.Application.DoEvents();
            }
            if (dataListItem == null)
            {
                MessageBox.Show("订单数据为空");
                return;
            }
            _userID = Security.SessionManager.UserName;
            _dataID = dataListItem.DataID;

           
            /*
             * 先在LocalDicom目录下找dicom文件，然后再在处理好的包里找
             * */
            string dicomFolder = AirwayPatients.BaseDicomFolder + "\\" + dataListItem.PatientName+"_"+dataListItem.StudyInstanceUID;
            AirwayPatient patient = AirwayPatients.FindByOrderId(_dataID);
            if (Directory.Exists(dicomFolder))
            {
                string[] dirs = Directory.GetDirectories(dicomFolder);
                bool hasLocalDicom = false;
                if (dirs.Length > 0)
                {
                    foreach (var item in dirs)
                    {
                        string[] files1 = Directory.GetFiles(item);
                        if (files1.Length > 10)
                        {
                            hasLocalDicom = true;
                        }
                    }
                }

                if (hasLocalDicom)
                {
                    this.Cursor = Cursors.Wait;
                    ImportDicomWindow3 dicomViewer = new ImportDicomWindow3(dicomFolder);
                    dicomViewer.ShowDialog();
                    this.Cursor = Cursors.Arrow;
                    return;
                }
            }
            else if (patient != null)
            {
                if (Directory.Exists(patient.DicomFolder))
                {
                    if (Directory.GetFiles(patient.DicomFolder).Length > 10)
                    {
                        this.Cursor = Cursors.Wait;
                        ImportDicomWindow3 dicomViewer = new ImportDicomWindow3(patient.DicomFolder);
                        dicomViewer.ShowDialog();
                        this.Cursor = Cursors.Arrow;
                        return;
                    }
                }
            }
            MESPDownloadUpload.UserId = _userID;
            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = _dataID;
            try
            {

                ThreadPool.QueueUserWorkItem(delegate
                {
                    new MESPDownloadUpload().FetchFileListAsync(
                        云数据类型Enum.Dicom数据压缩包,
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

                               

                                MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = dataListItem.DataID;
                                string filename = Path.Combine( AirwayPatients.BaseDicomFolder , selectedFile.FileName);

                                if (File.Exists(filename))
                                {
                                    string destFolder = AirwayPatients.BaseDicomFolder + "\\" + dataListItem.PatientName + "_" + dataListItem.StudyInstanceUID;
                                    FileDAO.upZip(filename, destFolder);
                                    this.Cursor = Cursors.Wait;
                                    ImportDicomWindow3 dicomViewer = new ImportDicomWindow3(destFolder);
                                    dicomViewer.ShowDialog();
                                    this.Cursor = Cursors.Arrow;

                                    return;
                                }

                                lbWaiting.Content = "正在下载，请稍候。";
                                lbWaiting.Visibility = Visibility.Visible;
                                btnCancelDownload.Visibility = System.Windows.Visibility.Visible;
                                string selectedDicomPackageLocalPath = "";
                                try
                                {
                                    filename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filename), System.IO.Path.GetFileNameWithoutExtension(filename) + ".temp");
                                    selectedDicomPackageLocalPath = new FileInfo(filename).FullName;
                                }
                                catch (Exception)
                                {
                                    //filename = @"C:\AirwayVE\CT\" + items.PatientName + ".zip";
                                    filename =  Path.Combine(AirwayPatients.BaseDicomFolder, dataListItem.PatientName + ".temp");
                                    selectedDicomPackageLocalPath = new FileInfo(filename).FullName;
                                }

               
                                ThreadPool.QueueUserWorkItem(delegate
                                {
                                   DownloadSingleDicom(selectedFile,dataListItem, selectedDicomPackageLocalPath, 云数据类型Enum.Dicom数据压缩包);
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

      
        private void linkLabelResult_Click(object sender, RoutedEventArgs e)
        {
            MainDataWindow.Instance.PauseGUI();
            System.Windows.Forms.Application.DoEvents();
            // 判断是否上传
            ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
            string DataID = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<Models.DataListItem> list = listView.Items.SourceCollection.Cast<Models.DataListItem>();
            var items = list.First(item => ((Models.DataListItem)item).DataID == DataID);
            int index = list.ToList().IndexOf(items);
            if (index >= 0)
            {
                ListViewDAO.SetCheckStatusInListView(listView, "cbSelect", false);
                ListViewDAO.SetCheckStatusInListViewItem(listView, index, "cbSelect", false);
                System.Windows.Forms.Application.DoEvents();
            }
            _userID = Security.SessionManager.UserName;
            _dataID = items.DataID;

            AirwayPatient patient =AirwayDAO.ExistedAirwayData(_dataID);
            if (patient!=null)
            {
                
                vtkImageData _rawCTMetaImage = patient.吸气末期MhdFileName.ReadMetaImage();
                
                MainWindowMA CTRefineMainForm = new MainWindowMA(patient,
                    LungCare.SupportPlatform.Entities.OrientationEnum.Axial,
                    _rawCTMetaImage, patient.SegmentedMhd_FileName);
                
                CTRefineMainForm.ShowDialog();
                MainDataWindow.Instance.ResumeGUI();
                return;
            }
            MainDataWindow.Instance.ResumeGUI();
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
                                //MessageBox.Show("数据还未处理完成,请等待数据处理完成!");
                                return;
                            }),
                        new EventHandler<FileListFinishedArgs>(delegate(Object senderInner, FileListFinishedArgs eInner)
                        {
                            Dispatcher.BeginInvoke(new Action(delegate()
                            {
                                _adminFileList = eInner.Result;
                                FileListItem selectedFileItem = _adminFileList[0];
                                ////先判断本地是否已经解压缩的文件包
                                //bool isExistAirwayFolder = false;
                                //string airwayFolder = Path.Combine(AirwayPatients.BaseFolder , Path.GetFileNameWithoutExtension(selectedFileItem.FileName));
                                //if (Directory.Exists(airwayFolder))
                                //{
                                //    string[] dirs = Directory.GetDirectories(airwayFolder);
                                //    foreach (var item in dirs)
                                //    {
                                //        if (item.Contains(items.PatientName.Replace(" " , "")))
                                //        {
                                //            string[] files = Directory.GetFiles(item);
                                //            if (files.Length > 2)
                                //            {
                                //                if (files.Contains(Path.Combine(item, "airway.vtp"))
                                //                    && files.Contains(Path.Combine(item, "i.mhd"))
                                //                    && files.Contains(Path.Combine(item, "i.raw")))
                                //                {
                                //                    isExistAirwayFolder = true;
                                //                }
                                //            }
                                //        }
                                //    }
                                  
                                //}

                                //if (isExistAirwayFolder)
                                //{
                                //    openPatientAirwayWindow(airwayFolder);
                                //    return;
                                //}
                                lbWaiting.Content = "正在下载，请稍候。";
                                lbWaiting.Visibility = Visibility.Visible;
                                btnCancelDownload.Visibility = System.Windows.Visibility.Visible;
                                MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = items.DataID;
                                //string filename = @"C:\AirwayVE\CT\" + selectedFile.FileName;
                                string filename = Path.Combine(AirwayPatients.BaseDicomFolder, selectedFileItem.FileName);

                                string selectedAirwayPackageLocalPath = "";  //airway压缩包文件路径
                                try
                                {

                                    selectedAirwayPackageLocalPath =
                                    new FileInfo(filename).FullName;
                                }
                                catch (Exception)
                                {
                                    //filename = @"C:\AirwayVE\CT\" + items.PatientName + ".zip";
                                    filename = Path.Combine(AirwayPatients.BaseDicomFolder, items.PatientName + ".zip");
                                    selectedAirwayPackageLocalPath =
                                   new FileInfo(filename).FullName;
                                }

                                
                                ThreadPool.QueueUserWorkItem(delegate
                                {
                                    DownloadAirwayResultFile(selectedFileItem, selectedAirwayPackageLocalPath, 云数据类型Enum.处理结果);
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

        private void linkLabelExportResult_Click(object sender, RoutedEventArgs e)
        {
            // 判断是否上传
            ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
            string DataID = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<Models.DataListItem> list = listView.Items.SourceCollection.Cast<Models.DataListItem>();
            var items = list.First(item => ((Models.DataListItem)item).DataID == DataID);
            int index = list.ToList().IndexOf(items);
            if (index >= 0)
            {
                ListViewDAO.SetCheckStatusInListView(listView, "cbSelect", false);
                ListViewDAO.SetCheckStatusInListViewItem(listView, index, "cbSelect", false);
            }
            _userID = Security.SessionManager.UserName;
            _dataID = items.DataID;

            AirwayPatient patient = AirwayDAO.ExistedAirwayData(_dataID);
            if (patient != null)
            {
                if (!string.IsNullOrEmpty(patient.吸气末期MhdFileName) && Directory.Exists(Path.GetDirectoryName(patient.吸气末期MhdFileName)))
                {
                    if (File.Exists(patient.吸气末期MhdFileName))
                    {
                        if (Directory.GetFiles(Path.GetDirectoryName(patient.吸气末期MhdFileName)).Length > 0)
                        {
                            string fileName = string.Format("[{0}][{3}][{1}][{2}].zip", patient.Name, patient.Institution, System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                            fileName = fileName.Replace(' ', '-');

                            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog
                            {
                                FileName = fileName,
                                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                            };

                            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                string airwayPatientFileName = Path.Combine(Path.GetTempPath(), "airwaypatient.xml");
                                patient.Save(airwayPatientFileName);

                                // "C:\Program Files\2345Soft\HaoZip\HaoZipC.exe" a -tzip c:/a.zip I:\AirwayVE\Db\ZHANGYONG_M_20145693114_bfd3d7a2adbf425ba8a4497917bac0f1
                                string args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\" \"{3}\"", sfd.FileName, patient.GetFile(""), patient.DicomFolder, airwayPatientFileName);
                                var psi = new ProcessStartInfo(System.Windows.Forms.Application.StartupPath + @"\HaoZipC.exe", args);
                                Process.Start(psi).WaitForExit();
                                MessageBox.Show(string.Format(@"压缩完毕。
文件保存到{0}。", sfd.FileName));
                                Process.Start(new FileInfo(sfd.FileName).Directory.FullName);
                            }
                        }
                        else
                        {
                            MessageBox.Show("本地分割数据不完整，请重新下载！");
                        }
                    }
                    else
                    {
                        MessageBox.Show("本地分割数据不存在，请先下载！");
                    }
                }
                else
                {
                    MessageBox.Show("本地分割数据不完整，请重新下载！");
                }
                
            }
            else
            {
                MessageBox.Show("本地不存在分割数据，请先下载！");
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
        private void DownloadAirwayResultFile(FileListItem selectedFileItem, string selectedAirwayPackageLocalPath, 云数据类型Enum 云数据类型Enum)
        {
            DownloadFileWorker.Download(
                selectedFileItem,
                selectedAirwayPackageLocalPath,
                delegate(string filename)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        try
                        {
                            lbWaiting.Visibility = Visibility.Hidden;
                            //ResumeGUI();
                            while (!File.Exists(selectedAirwayPackageLocalPath)) { 
                            }

                            if (UpdateData != null)
                            {
                                UpdateData(this, new EventArgs() { });
                            }
                            //Thread.Sleep(1000);
                            importAndOpenPatientAirwayWindow(selectedAirwayPackageLocalPath);
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
                delegate(ProgressArgs eProgress)
                {
                    UIUtil.Invoke(this, delegate
                    {

                        StringBuilder stringUploadinfo = new StringBuilder();
                        //stringUploadinfo.AppendLine(string.Format("{0} {1} {2}",PatientName,PatientSex, PatientAge));
                        stringUploadinfo.AppendFormat("【预计剩余时间：{0}，当前速度：{3}，已下载：{1}/{2}】",
                            eProgress.RemainTimeHumanReadable,
                            eProgress.Finished.HasValue ? HumanReadableFilesize(eProgress.Finished.Value) : HumanReadableFilesize(0),
                            HumanReadableFilesize(eProgress.Total),
                            eProgress.Speed.HasValue ? HumanReadableFilesize((long)eProgress.Speed.Value) + "/秒" : "未知");

                        string speed = eProgress.Speed.HasValue ? HumanReadableFilesize((long)eProgress.Speed.Value) + "/秒" : "未知";
                        //progressArgs.ToString();


                        lbWaiting.Visibility = System.Windows.Visibility.Visible;
                        lbWaiting.Content = stringUploadinfo.ToString();
                        System.Windows.Forms.Application.DoEvents();
                    });
                }, 云数据类型Enum);
        }



        private void DownloadAirwayResultFileWithoutOpen(FileListItem selectedFileItem, string selectedAirwayPackageLocalPath, 云数据类型Enum 云数据类型Enum)
        {
            DownloadFileWorker.Download(
                selectedFileItem,
                selectedAirwayPackageLocalPath,
                delegate(string filename)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        try
                        {
                            lbWaiting.Visibility = Visibility.Hidden;
                            ResumeGUI();
                            while (!File.Exists(selectedAirwayPackageLocalPath))
                            {
                            }


                            if (UpdateData != null)
                            {
                                UpdateData(this, new EventArgs() { });
                            }
                            //Thread.Sleep(1000);
                            Process.Start(Path.GetDirectoryName(selectedAirwayPackageLocalPath));
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
                delegate(ProgressArgs eProgress)
                {
                    UIUtil.Invoke(this, delegate
                    {

                        StringBuilder stringUploadinfo = new StringBuilder();
                        //stringUploadinfo.AppendLine(string.Format("{0} {1} {2}",PatientName,PatientSex, PatientAge));
                        stringUploadinfo.AppendFormat("【预计剩余时间：{0}，当前速度：{3}，已下载：{1}/{2}】",
                            eProgress.RemainTimeHumanReadable,
                            eProgress.Finished.HasValue ? HumanReadableFilesize(eProgress.Finished.Value) : HumanReadableFilesize(0),
                            HumanReadableFilesize(eProgress.Total),
                            eProgress.Speed.HasValue ? HumanReadableFilesize((long)eProgress.Speed.Value) + "/秒" : "未知");

                        string speed = eProgress.Speed.HasValue ? HumanReadableFilesize((long)eProgress.Speed.Value) + "/秒" : "未知";
                        //progressArgs.ToString();


                        lbWaiting.Visibility = System.Windows.Visibility.Visible;
                        lbWaiting.Content = stringUploadinfo.ToString();
                        System.Windows.Forms.Application.DoEvents();
                    });
                }, 云数据类型Enum);
        }


        private void DownloadSingleDicom(FileListItem selectedFile, DataListItem dataListItem, string selectedDicomPackageLocalPath, 云数据类型Enum 云数据类型Enum)
        {

            DownloadFileWorker.Download(
                selectedFile,
                selectedDicomPackageLocalPath,
                delegate(string filename)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        try
                        {
                            //string tempPath = selectedDicomPackageLocalPath.Substring(0, selectedDicomPackageLocalPath.LastIndexOf(Path.DirectorySeparatorChar));
                            //System.Diagnostics.Process.Start(tempPath);
                            //ResumeGUI();
                            lbWaiting.Visibility = Visibility.Hidden;
                            btnCancelDownload.Visibility = System.Windows.Visibility.Hidden;
                            ResumeGUI();
                            while (!File.Exists(selectedDicomPackageLocalPath))
                            {

                            }

                            string destFileRAR = Path.Combine(Path.GetDirectoryName(selectedDicomPackageLocalPath) , Path.GetFileNameWithoutExtension(selectedDicomPackageLocalPath)+".zip");
                            File.Copy(selectedDicomPackageLocalPath , destFileRAR);
                            File.Delete(selectedDicomPackageLocalPath);
                            selectedDicomPackageLocalPath = destFileRAR;
                            //System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(selectedDicomPackageLocalPath));
                            string destFolder = AirwayPatients.BaseDicomFolder + "\\" +dataListItem.PatientName +"_"+dataListItem.StudyInstanceUID;
                            //解压缩并用dicomviewer打开
                            FileDAO.upZip(selectedDicomPackageLocalPath, destFolder);
                            if (File.Exists(selectedDicomPackageLocalPath))
                            {
                                File.Delete(selectedDicomPackageLocalPath);
                            }

                            if (UpdateData != null)
                            {
                                UpdateData(this, new EventArgs() { });
                            }
                            this.Cursor = Cursors.Wait;
                            ImportDicomWindow3 dicomViewer = new ImportDicomWindow3(destFolder);
                            dicomViewer.ShowDialog();
                            this.Cursor = Cursors.Arrow;
                            //Thread.Sleep(1000);
                            //openPatientsMsgWindow(selectedDicomPackageLocalPath);
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
                        lbWaiting.Visibility = Visibility.Hidden;
                        btnCancelDownload.Visibility = System.Windows.Visibility.Hidden;
                        MessageBox.Show("下载发生异常！", "提示");
                        lbWaiting.Visibility = Visibility.Hidden;
                        ResumeGUI();
                    });
                },
                delegate(Exception ex)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        lbWaiting.Visibility = Visibility.Hidden;
                        btnCancelDownload.Visibility = System.Windows.Visibility.Hidden;
                        Util.ShowExceptionMessage(ex);
                        lbWaiting.Visibility = Visibility.Hidden;
                        //Download(selectedFile, selectedDicomPackageLocalPath, 云数据类型Enum.处理结果);
                        ResumeGUI();
                    });
                },
                delegate(ProgressArgs eProgress)
                {
                    UIUtil.Invoke(this, delegate
                    {

                        StringBuilder stringUploadinfo = new StringBuilder();
                        //stringUploadinfo.AppendLine(string.Format("{0} {1} {2}",PatientName,PatientSex, PatientAge));
                        stringUploadinfo.AppendFormat("【预计剩余时间：{0}，当前速度：{3}，已上传：{1}/{2}】",
                            eProgress.RemainTimeHumanReadable,
                            eProgress.Finished.HasValue ? HumanReadableFilesize(eProgress.Finished.Value) : HumanReadableFilesize(0),
                            HumanReadableFilesize(eProgress.Total),
                            eProgress.Speed.HasValue ? HumanReadableFilesize((long)eProgress.Speed.Value) + "/秒" : "未知");

                        string speed = eProgress.Speed.HasValue ? HumanReadableFilesize((long)eProgress.Speed.Value) + "/秒" : "未知";
                        //progressArgs.ToString();
                        
                        
                        //lbWaiting.Visibility = System.Windows.Visibility.Visible;
                        lbWaiting.Content = stringUploadinfo.ToString();
                        System.Windows.Forms.Application.DoEvents();
                    });
                }, 云数据类型Enum);
        }

        private void importAndOpenPatientAirwayWindow(String airwayRarFileNamePath) {
            String fileName = Path.GetFileName(airwayRarFileNamePath);
            if (!validateExtention(Path.GetExtension(fileName)))
            {
                MessageBox.Show("文件格式不合格，请选择压缩包文件", "提示：");

                return;
            }
            //string folder = Path.Combine(AirPatientForm.RegistryDAO.AirwayBaseFolder, Path.GetFileNameWithoutExtension(filename).Replace(" ", ""));
            string folder = Path.Combine(AirwayPatients.BaseFolder, Path.GetFileNameWithoutExtension(fileName).Replace(" ", ""));

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string filenam1e = Path.Combine(folder, "airwaypatient.xml");
            if (!File.Exists(filenam1e))
            {

                string args = string.Format("x -o\"{0}\" -y -sn \"{1}\"", folder, airwayRarFileNamePath);
                var psi = new ProcessStartInfo(System.Windows.Forms.Application.StartupPath + @"\HaoZipC.exe", args);
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(psi).WaitForExit();
            }


            File.Delete(airwayRarFileNamePath);
            if (!File.Exists(filenam1e))
            {
                MessageBox.Show(@"压缩文件中未找到文件 : airwaypatient.xml");
                return;
            }
            
            //更新DataID
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
            MainDataWindow.Instance.PauseGUI();
            MainWindowMA CTRefineMainForm = new MainWindowMA(patient,
                LungCare.SupportPlatform.Entities.OrientationEnum.Axial,
                _rawCTMetaImage, patient.SegmentedMhd_FileName);

            CTRefineMainForm.ShowDialog();

            MainDataWindow.Instance.ResumeGUI();
        }

        private void importPatientAirway(String airwayRarFileNamePathRar)
        {
            String fileName = Path.GetFileName(airwayRarFileNamePathRar);
            if (!validateExtention(Path.GetExtension(fileName)))
            {
                MessageBox.Show("文件格式不合格，请选择压缩包文件", "提示：");

                return;
            }
            //string folder = Path.Combine(AirPatientForm.RegistryDAO.AirwayBaseFolder, Path.GetFileNameWithoutExtension(filename).Replace(" ", ""));
            string folder = Path.Combine(AirwayPatients.BaseFolder, Path.GetFileNameWithoutExtension(fileName).Replace(" ", ""));

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string filenam1e = Path.Combine(folder, "airwaypatient.xml");
            if (!File.Exists(filenam1e))
            {

                string args = string.Format("x -o\"{0}\" -y -sn \"{1}\"", folder, airwayRarFileNamePathRar);
                var psi = new ProcessStartInfo(System.Windows.Forms.Application.StartupPath + @"\HaoZipC.exe", args);
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(psi).WaitForExit();
            }

            File.Delete(airwayRarFileNamePathRar);
            if (!File.Exists(filenam1e))
            {
                MessageBox.Show(@"压缩文件中未找到文件 : airwaypatient.xml");
                return;
            }

            //更新DataID
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

            //if (_datalist == null || _datalist.Length == 0)
            //{
            //    return;
            //}

            //listView.DataContext =
            //    new System.ComponentModel.BindingList<Models.DataListItem>(
            //        new List<Models.DataListItem>(
            //            _datalist.Where(item => item.Status == "处理完成" && item.PatientName != null && (item.PatientName.ToLower().Contains(text2Search)|| item.DataID.ToLower().Contains(text2Search)))));
            ////listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
            //listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());

            if (_datalist == null || _datalist.Length == 0)
            {
                return;
            }

            _CurrentDataList = _datalist.Where(item => ((item.ChineseName != null && item.ChineseName.ToLower().Contains(text2Search)) || (item.UserId != null && item.UserId.ToLower().Contains(text2Search)) || (item.PatientName != null && item.PatientName.ToLower().Contains(text2Search)) || (item.DataID != null && item.DataID.ToLower().Contains(text2Search)))).ToArray();
            _CurrentPage = 0;


            if (_CurrentDataList.Count() % _PageSize == 0)
            {
                _MaxPage = _CurrentDataList.Count() / _PageSize - 1;
            }
            else
            {
                _MaxPage = _CurrentDataList.Count() / _PageSize;
            }

            checkPage(_CurrentPage, _MaxPage, _MinPage);
            LoadPage(_CurrentPage, _PageSize, _CurrentDataList, _MaxPage);
        }

        private void btnCancelDownload_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("是否取消下载？取消后支持续传您可以随时重新启动下载。", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                btnCancelDownload.Visibility = lbWaiting.Visibility = Visibility.Hidden;
                tbSearch.IsEnabled = this.listView.IsEnabled = true;
                this.Cursor = Cursors.Arrow;
                MESPDownloadUpload.CancelDownload();
                (sender as Image).Visibility = System.Windows.Visibility.Hidden;
                lbWaiting.Visibility = System.Windows.Visibility.Hidden;
            }
        }


        Models.DataListItem[] _CurrentDataList;
        private int _MinPage = 0;
        private int _MaxPage = 0;
        private int _CurrentPage = 0;
        private int _PageSize = 12;

        private void setCurrentDataList() { 
            int restNumb = _datalist.Length-((_CurrentPage+1)*_PageSize);
             
            if(restNumb<=12&&restNumb>0){
                _CurrentDataList = new Models.DataListItem[] { };

                for (int i = 0; i < restNumb;i++ )
                {
                    _CurrentDataList[i] = _datalist[(_CurrentPage + 1 )*_PageSize];
                    string text2Search = tbSearch.Text.ToLower();
                    listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(
                    new List<Models.DataListItem>(
                        _CurrentDataList.Where(item => item.PatientName != null && (item.PatientName.ToLower().Contains(text2Search) || item.DataID.ToLower().Contains(text2Search)))));
                    //listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
                    listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
                }

            }
        }


        private void btn_PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            _CurrentPage -= 1;
            checkPage(_CurrentPage, _MaxPage, _MinPage);
            LoadPage(_CurrentPage, _PageSize, _CurrentDataList, _MaxPage);
        }

        private void btn_NextPage_Click(object sender, RoutedEventArgs e)
        {
            _CurrentPage += 1;
            checkPage(_CurrentPage, _MaxPage, _MinPage);
            LoadPage(_CurrentPage, _PageSize, _CurrentDataList, _MaxPage);
        }

        public void LoadPage(int currentPage, int pageSize, DataListItem[] _allOrder, int maxPage)
        {
            label_totalNumber.Content = "订单总数：" + _allOrder.Length;
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //label_lastRefreshTime.Content = "最后刷新的时间是：" + now;
            label_Pages.Content = "第" + (currentPage + 1) + "页  共" + (maxPage + 1) + "页";
            if (currentPage < maxPage)
            {
                DataListItem[] _CurrentPageOrderList = new DataListItem[pageSize];
                for (int i = 0; i < pageSize; i++)
                {
                    _CurrentPageOrderList[i] = _allOrder[currentPage * pageSize + i];
                }
                listView.DataContext = new System.ComponentModel.BindingList<DataListItem>(_CurrentPageOrderList);
                listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
            }
            if (currentPage == maxPage)
            {
                if (_allOrder.Count() % pageSize != 0)
                {

                    DataListItem[] _CurrentPageOrderList = new DataListItem[_allOrder.Count() % pageSize];
                    for (int i = 0; i < _allOrder.Count() % pageSize; i++)
                    {
                        _CurrentPageOrderList[i] = _allOrder[currentPage * pageSize + i];
                    }
                    listView.DataContext = null;
                    listView.DataContext = new System.ComponentModel.BindingList<DataListItem>(_CurrentPageOrderList);
                    listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
                }

                if (_allOrder.Count() % pageSize == 0)
                {
                    DataListItem[] _CurrentPageOrderList = new DataListItem[pageSize];
                    for (int i = 0; i < pageSize; i++)
                    {
                        _CurrentPageOrderList[i] = _allOrder[currentPage * pageSize + i];
                    }
                    listView.DataContext = null;
                    listView.DataContext = new System.ComponentModel.BindingList<DataListItem>(_CurrentPageOrderList);
                    listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
                    for (int i = 0; i < listView.Items.Count; i++) {
                        

                    }
                }

            
            }

            if (cbSelectAll.IsChecked.Value)
            {
                ListViewDAO.SetCheckStatusInListView(listView, "cbSelect", true);

            }
            else
            {
                ListViewDAO.SetCheckStatusInListView(listView, "cbSelect", false);
            }
        }


        /// <summary>
        /// 根据当前page与minPage和maxPage的比较设定上下页按钮的状态
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="maxPage"></param>
        public void checkPage(int currentPage, int maxPage, int minPage)
        {
            if (currentPage <= minPage)
            {
                btn_PreviousPage.IsEnabled = false;
            }
            else
            {
                btn_PreviousPage.IsEnabled = true;
            }
            if (currentPage >= (maxPage))
            {
                btn_NextPage.IsEnabled = false;
            }
            else
            {
                btn_NextPage.IsEnabled = true;
            }
        }

        private void linkLabelDownLoadAirwayPackage_Click(object sender, RoutedEventArgs e)
        {

            // 判断是否上传
            ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
            string DataID = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<Models.DataListItem> list = listView.Items.SourceCollection.Cast<Models.DataListItem>();
            var items = list.First(item => ((Models.DataListItem)item).DataID == DataID);
            int index = list.ToList().IndexOf(items);
            if (index >= 0)
            {
                ListViewDAO.SetCheckStatusInListView(listView, "cbSelect", false);
                ListViewDAO.SetCheckStatusInListViewItem(listView, index, "cbSelect", false);
            }
            _userID = Security.SessionManager.UserName;
            _dataID = items.DataID;

            AirwayPatient patient = AirwayDAO.ExistedAirwayData(_dataID);
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
                                //MessageBox.Show("数据还未处理完成,请等待数据处理完成!");
                                return;
                            }),
                        new EventHandler<FileListFinishedArgs>(delegate(Object senderInner, FileListFinishedArgs eInner)
                        {
                            Dispatcher.BeginInvoke(new Action(delegate()
                            {
                                _adminFileList = eInner.Result;
                                FileListItem selectedFileItem = _adminFileList[0];
                                
                                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog
                                 {
                                     FileName = selectedFileItem.FileName,
                                     InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                                 };

                                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    lbWaiting.Content = "正在下载，请稍候。";
                                    lbWaiting.Visibility = Visibility.Visible;
                                    MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = items.DataID;
                                    //string filename = @"C:\AirwayVE\CT\" + selectedFile.FileName;

                                    string filename = sfd.FileName;
                                    string selectedAirwayPackageLocalPath = "";  //airway压缩包文件路径
                                    try
                                    {

                                        selectedAirwayPackageLocalPath =
                                        new FileInfo(filename).FullName;
                                    }
                                    catch (Exception)
                                    {
                                        //filename = @"C:\AirwayVE\CT\" + items.PatientName + ".zip";
                                        filename = Path.Combine(AirwayPatients.BaseDicomFolder, items.PatientName + ".zip");
                                        selectedAirwayPackageLocalPath =
                                       new FileInfo(filename).FullName;
                                    }


                                    ThreadPool.QueueUserWorkItem(delegate
                                    {
                                        DownloadAirwayResultFileWithoutOpen(selectedFileItem, selectedAirwayPackageLocalPath, 云数据类型Enum.处理结果);
                                    });
                                }
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

        private void cbSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            ListViewDAO.SetCheckStatusInListView(listView, "cbSelect" , true);
        }

        private void cbSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            ListViewDAO.SetCheckStatusInListView(listView, "cbSelect", false);
        }

        private void btnDownloadAllAirwayPackage_Click(object sender, RoutedEventArgs e)
        {
            DownloadSelectBatchAirway();
        }

        private void DownloadSelectBatchAirway()
        {
            batchAirwayFilesDownload = new List<DataItemFilePair>();
            _airwayBatchDownloadIndex = 0;
            totalBatchSize = 0;
            hasDownloadSize = 0;
            increaseSize = 0;
            List<DataListItem> list = ListViewDAO.SelectAirwayDataListItemsFromListView(listView, "cbSelect", true);

            //foreach (var item in list)
            //{
            //    if (DownloadBatchAirwayEvevt != null)
            //    {
            //        DownloadBatchAirwayEvevt(this, new DataListItemArgs()
            //        {
            //            DataListItem = item
            //        });
            //    }
            //}


            batchAirwayFilesDownload = AirwayDAO.GetFileListItemPair(list);
            if (batchAirwayFilesDownload == null || batchAirwayFilesDownload.Count <= 0)
            {
                return;
            }

            btnCancelDownload.Visibility = System.Windows.Visibility.Visible;
            lbWaiting.Visibility = System.Windows.Visibility.Visible;
            foreach (var item in batchAirwayFilesDownload)
            {
                totalBatchSize += item.FileListItem.FileSize;
            }

            DownloadBatchAirway(batchAirwayFilesDownload[_dicomBatchDownloadIndex].FileListItem,
                batchAirwayFilesDownload[_dicomBatchDownloadIndex].DataListItem);
            
           
        }

        public event EventHandler<DataListItemArgs> DownloadBatchDicomEvevt;
        public event EventHandler<DataListItemArgs> DownloadBatchAirwayEvevt;
        private int _dicomBatchDownloadIndex = 0, _airwayBatchDownloadIndex = 0;
        private List<DataItemFilePair> batchDicomFilesDownload;
        private List<DataItemFilePair> batchAirwayFilesDownload;
        private long totalBatchSize = 0;
        private long hasDownloadSize = 0;
        private long increaseSize = 0;
        private void btnDownloadAllDicom_Click(object sender, RoutedEventArgs e)
        {
            DownloadSelectBatchDicoms();
        }


        private void DownloadSelectBatchDicoms()
        {
            batchDicomFilesDownload = new List<DataItemFilePair>();
            _dicomBatchDownloadIndex = 0;
            totalBatchSize = 0;
            hasDownloadSize = 0;
            increaseSize = 0;
            List<DataListItem> list = ListViewDAO.SelectDicomDataListItemsFromListView(listView, "cbSelect", true);
            //foreach (var item in list)
            //{
            //    if (DownloadBatchDicomEvevt != null)
            //    {
            //        DownloadBatchDicomEvevt(this, new DataListItemArgs()
            //        {
            //            DataListItem = item
            //        });
            //    }
            //}

            //return;
            batchDicomFilesDownload = LocalDicomDAO.GetFileListItemPair(list);
            if (batchDicomFilesDownload == null || batchDicomFilesDownload.Count <= 0)
            {
                return;
            }

            btnCancelDownload.Visibility = System.Windows.Visibility.Visible;
            lbWaiting.Visibility = System.Windows.Visibility.Visible;
            foreach (var item in batchDicomFilesDownload)
            {
                totalBatchSize += item.FileListItem.FileSize;
            }

            DownloadBatchDICOM(batchDicomFilesDownload[_dicomBatchDownloadIndex].FileListItem,
                batchDicomFilesDownload[_dicomBatchDownloadIndex].DataListItem);
            
            
        }

        private void DownloadBatchDICOM(FileListItem file , DataListItem dataListItem)
        {
            
            string destFilename = Path.Combine(AirwayPatients.BaseDicomFolder, file.FileName);
            destFilename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(destFilename), System.IO.Path.GetFileNameWithoutExtension(destFilename) + ".temp");
            DownloadFileWorker.Download(
               file,
               destFilename,
               delegate(string filename)
               {
                   while (!File.Exists(destFilename))
                   {

                   }

                   Thread.Sleep(1000);
                   Dispatcher.BeginInvoke(new Action(delegate()
                            {
                   lbWaiting.Content = "【正在解压缩. . . . . .】";
                            }));
                   string destFileRAR = Path.Combine(Path.GetDirectoryName(destFilename), Path.GetFileNameWithoutExtension(destFilename) + ".zip");
                   File.Copy(destFilename, destFileRAR);
                   File.Delete(destFilename);
                   destFilename = destFileRAR;
                   //System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(selectedDicomPackageLocalPath));
                   string destFolder = AirwayPatients.BaseDicomFolder + "\\" + dataListItem.PatientName + "_" + dataListItem.StudyInstanceUID;
                   //解压缩并用dicomviewer打开
                   FileDAO.upZip(destFilename, destFolder);

                   if (UpdateData != null)
                   {
                       UpdateData(this, new EventArgs() { });
                   }
                   if (File.Exists(destFilename))
                   {
                       File.Delete(destFilename);
                   }
                   hasDownloadSize += file.FileSize;
                   _dicomBatchDownloadIndex++;
                   if (_dicomBatchDownloadIndex < batchDicomFilesDownload.Count)
                   {
                       DownloadBatchDICOM(batchDicomFilesDownload[_dicomBatchDownloadIndex].FileListItem ,
                           batchDicomFilesDownload[_dicomBatchDownloadIndex].DataListItem);
                   }
                   else
                   {
                       Dispatcher.BeginInvoke(new Action(delegate()
                            {
                                lbWaiting.Visibility = System.Windows.Visibility.Hidden;
                                btnCancelDownload.Visibility = System.Windows.Visibility.Hidden;
                            }));
                   }
               },
               delegate(string errMsg)
               {
                   Dispatcher.BeginInvoke(new Action(delegate()
                   {
                       lbWaiting.Visibility = System.Windows.Visibility.Hidden;
                       btnCancelDownload.Visibility = System.Windows.Visibility.Hidden;
                   }));
                   MessageBox.Show("下载发生异常！", "提示");
               },
               delegate(Exception ex)
               {
                   Dispatcher.BeginInvoke(new Action(delegate()
                   {
                       lbWaiting.Visibility = System.Windows.Visibility.Hidden;
                       btnCancelDownload.Visibility = System.Windows.Visibility.Hidden;
                   }));
                   Util.ShowExceptionMessage(ex);
               },
               delegate(ProgressArgs eProgress)
               {
                   this.Dispatcher.Invoke(new Action(delegate()
                   {
                       StringBuilder stringUploadinfo = new StringBuilder();
                       //stringUploadinfo.AppendLine(string.Format("{0} {1} {2}",PatientName,PatientSex, PatientAge));
                       increaseSize = eProgress.Finished.HasValue ? eProgress.Finished.Value : 0;
                       //string speed = eProgress.Speed.HasValue ? HumanReadableFilesize((long)eProgress.Speed.Value) + "/秒" : "未知";
                       double speedValue = eProgress.Speed.HasValue ? eProgress.Speed.Value : 1;
                       double leftMilliontime = ((totalBatchSize - (hasDownloadSize+increaseSize)) / speedValue);
                       string leftTime = "";
                       if (leftMilliontime > 60 && leftMilliontime<3600)
                       {
                           leftTime =  (int)(leftMilliontime / 60) + "分钟";
                       }
                       else if (leftMilliontime > 3600)
                       {
                           leftTime = (int)(leftMilliontime / 3600) + "小时";
                       }
                       else
                       {
                           leftTime = (int)leftMilliontime + "秒";
                       }
                       stringUploadinfo.AppendFormat("【预计剩余时间：{0}，当前速度：{3}，已下载：{1}/{2}】",
                           leftTime,
                           HumanReadableFilesize((hasDownloadSize +increaseSize)),
                           HumanReadableFilesize(totalBatchSize),
                           eProgress.Speed.HasValue ? HumanReadableFilesize((long)eProgress.Speed.Value) + "/秒" : "未知");
                      
                       
                       //progressArgs.ToString();
                       //lbWaiting.Visibility = System.Windows.Visibility.Visible;
                       lbWaiting.Content = stringUploadinfo.ToString();
                       System.Windows.Forms.Application.DoEvents();
                   }));
                   //Console.WriteLine(progressArgs);
               }, 云数据类型Enum.Dicom数据压缩包);
        }



        private void DownloadBatchAirway(FileListItem file, DataListItem dataListItem)
        {

            string destFilename = Path.Combine(AirwayPatients.BaseDicomFolder, file.FileName);
            destFilename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(destFilename), System.IO.Path.GetFileNameWithoutExtension(destFilename) + ".temp");
            DownloadFileWorker.Download(
               file,
               destFilename,
               delegate(string filename)
               {
                   while (!File.Exists(destFilename))
                   {

                   }
                   string destFileRAR = Path.Combine(Path.GetDirectoryName(destFilename), Path.GetFileNameWithoutExtension(destFilename) + ".zip");
                   File.Copy(destFilename, destFileRAR);
                   File.Delete(destFilename);
                   destFilename = destFileRAR;

                   importPatientAirway(destFilename);
                   //System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(selectedDicomPackageLocalPath));
                   //string destFolder = AirwayPatients.BaseDicomFolder + "\\" + dataListItem.PatientName + "_" + dataListItem.StudyInstanceUID;
                   ////解压缩并用dicomviewer打开
                   //FileDAO.upZip(destFilename, destFolder);

                   if (UpdateData != null)
                   {
                       UpdateData(this, new EventArgs() { });
                   }
                   if (File.Exists(destFilename))
                   {
                       File.Delete(destFilename);
                   }
                   hasDownloadSize += file.FileSize;
                   _airwayBatchDownloadIndex++;
                   if (_airwayBatchDownloadIndex < batchAirwayFilesDownload.Count)
                   {
                       DownloadBatchAirway(batchAirwayFilesDownload[_dicomBatchDownloadIndex].FileListItem,
                           batchAirwayFilesDownload[_dicomBatchDownloadIndex].DataListItem);
                   }
                   else
                   {
                       lbWaiting.Visibility = System.Windows.Visibility.Hidden;
                       btnCancelDownload.Visibility = System.Windows.Visibility.Hidden;
                   }
               },
               delegate(string errMsg)
               {
                   MessageBox.Show("下载发生异常！", "提示");
               },
               delegate(Exception ex)
               {
                   Util.ShowExceptionMessage(ex);
               },
               delegate(ProgressArgs eProgress)
               {
                   this.Dispatcher.Invoke(new Action(delegate()
                   {
                       StringBuilder stringUploadinfo = new StringBuilder();
                       //stringUploadinfo.AppendLine(string.Format("{0} {1} {2}",PatientName,PatientSex, PatientAge));
                       increaseSize = eProgress.Finished.HasValue ? eProgress.Finished.Value : 0;
                       //string speed = eProgress.Speed.HasValue ? HumanReadableFilesize((long)eProgress.Speed.Value) + "/秒" : "未知";
                       double speedValue = eProgress.Speed.HasValue ? eProgress.Speed.Value : 1;
                       double leftMilliontime = ((totalBatchSize - (hasDownloadSize + increaseSize)) / speedValue);
                       string leftTime = "";
                       if (leftMilliontime > 60 && leftMilliontime < 3600)
                       {
                           leftTime = (int)(leftMilliontime / 60) + "分钟";
                       }
                       else if (leftMilliontime > 3600)
                       {
                           leftTime = (int)(leftMilliontime / 3600) + "小时";
                       }
                       else
                       {
                           leftTime = (int)leftMilliontime + "秒";
                       }
                       stringUploadinfo.AppendFormat("【预计剩余时间：{0}，当前速度：{3}，已下载：{1}/{2}】",
                           leftTime,
                           HumanReadableFilesize((hasDownloadSize + increaseSize)),
                           HumanReadableFilesize(totalBatchSize),
                           eProgress.Speed.HasValue ? HumanReadableFilesize((long)eProgress.Speed.Value) + "/秒" : "未知");


                       //progressArgs.ToString();
                       //lbWaiting.Visibility = System.Windows.Visibility.Visible;
                       lbWaiting.Content = stringUploadinfo.ToString();
                       System.Windows.Forms.Application.DoEvents();
                   }));
                   //Console.WriteLine(progressArgs);
               }, 云数据类型Enum.处理结果);
        }

        private String HumanReadableFilesize(long? size)
        {
            if (!size.HasValue)
            {
                return "";
            }

            double size1 = size.Value;
            String[] units = new String[] { "B", "KB", "MB", "GB", "TB", "PB" };
            double mod = 1024.0;
            int i = 0;
            while (size1 >= mod)
            {
                size1 /= mod;
                i++;
            }

            return size1.ToString("F1") + units[i];
            return Math.Round(size1) + units[i];
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateData != null)
            {
                UpdateData(this , new EventArgs());
            }
        }

        private bool isSelectDicom = false, isSelectAirway = false;
        private void cbSelectDicom_Checked(object sender, RoutedEventArgs e)
        {
            isSelectDicom = true;
        }

        private void cbSelectDicom_Unchecked(object sender, RoutedEventArgs e)
        {
            isSelectDicom = false;
        }

        private void cbSelectAirway_Checked(object sender, RoutedEventArgs e)
        {
            isSelectAirway = true;
        }

        private void cbSelectAirway_Unchecked(object sender, RoutedEventArgs e)
        {
            isSelectAirway = false;
        }

        private void btnDownloadAll_Click(object sender, MouseButtonEventArgs e)
        {
            if (isSelectDicom)
            {
                DownloadSelectBatchDicoms();
            }
            if (isSelectAirway)
            {
                DownloadSelectBatchAirway();
            }
        }

    }
}

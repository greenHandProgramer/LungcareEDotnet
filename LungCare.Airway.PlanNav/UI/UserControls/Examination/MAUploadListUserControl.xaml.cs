using AirwayCT.Entity;
using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.SupportPlatformDAO.Files;
using LungCare.SupportPlatform.UI.Windows.Common;
using LungCare.SupportPlatform.UI.Windows.Examination;
using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LungCare.SupportPlatform.UI.UserControls.Examination
{
    /// <summary>
    /// UploadListUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class MAUploadListUserControl : UserControl
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
            _datalist = _datalist.OrderBy(item => string.IsNullOrEmpty(item.UploadTimestamp)? DateTime.MinValue : DateTime.ParseExact(item.UploadTimestamp, "yyyyMMdd HHmmss", null)).Reverse().ToArray();

            string text2Search = tbSearch.Text.ToLower();

            listView.DataContext =
                new System.ComponentModel.BindingList<Models.DataListItem>(
                    new List<Models.DataListItem>(
                        _datalist.Where(item => item.Status != "待上传" && (item.PatientName.ToLower().Contains(text2Search)|| item.DataID.ToLower().Contains(text2Search)))));
            //listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
            listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
        }

        public MAUploadListUserControl()
        {
            InitializeComponent();
        }

        private String _userID;
        private String _dataID;
        private List<FileListItem> _adminFileList;
        private void linkLabel_Click(object sender, RoutedEventArgs e)
        {
            // 判断是否上传
            ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
            string DataID = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<Models.DataListItem> list = listView.Items.SourceCollection.Cast<Models.DataListItem>();
            var items = list.FirstOrDefault(item => ((Models.DataListItem)item).DataID == DataID);
            if (items == null)
            {
                MessageBox.Show("订单数据为空");
                return;
            }
            _userID = "15261595318";
            _dataID = items.DataID;
            string dicomFolder = AirwayPatients.BaseDicomFolder + "\\" + _dataID;
            if (Directory.Exists(dicomFolder))
            {
                string[] files = Directory.GetFiles(dicomFolder);
                if (files.Length > 0)
                {
                    ImportDicomWindow3 dicomViewer = new ImportDicomWindow3(dicomFolder);
                    dicomViewer.ShowDialog();

                    return;
                }
            }
            //AirwayPatient patient = AirwayPatients.FindByOrderId(_dataID);
            //if (patient != null)
            //{
            //    if (!string.IsNullOrEmpty(patient.DicomFolder))
            //    {
            //        if (Directory.Exists(patient.DicomFolder))
            //        {
            //            string[] files = Directory.GetFiles(patient.DicomFolder);
            //            if (files.Length > 0)
            //            {
            //                ///DicomViewer打开目录
            //                ///
            //                ImportDicomWindow1 dicomViewer = new ImportDicomWindow1(patient.DicomFolder);
            //                dicomViewer.ShowDialog();

            //                return;
            //            }
            //        }
            //    }
            //}
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
                                    Download(selectedFile, selectedDicomPackageLocalPath, 云数据类型Enum.Dicom数据压缩包);
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

        private void ResumeGUI()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                ((UIElement)Content).IsEnabled = true;
                Cursor = Cursors.Arrow;
            }));
        }
        private void Download(FileListItem selectedFile, string selectedDicomPackageLocalPath, 云数据类型Enum 云数据类型Enum)
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
                            ResumeGUI();
                            while (!File.Exists(selectedDicomPackageLocalPath))
                            {
                            }


                            //System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(selectedDicomPackageLocalPath));
                            string destFolder = AirwayPatients.BaseDicomFolder+"\\"+_dataID;
                            //解压缩并用dicomviewer打开
                            FileDAO.upZip(selectedDicomPackageLocalPath , destFolder);
                            ImportDicomWindow3 dicomViewer = new ImportDicomWindow3(destFolder);
                            dicomViewer.ShowDialog();

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

        private void PauseGUI()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                ((UIElement)Content).IsEnabled = false;
                Cursor = Cursors.Wait;
            }));
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
                        _datalist.Where(item => item.Status != "待上传" && (item.PatientName.ToLower().Contains(text2Search)|| item.DataID.ToLower().Contains(text2Search)))));
            //listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
            listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
        }

        //private void ShowOrderDetail_Click(object sender, RoutedEventArgs e)
        //{
        //    ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
        //    string PatientName = LinkLabel.Tag.ToString();
        //    Console.WriteLine(LinkLabel.Tag.ToString());

        //    IEnumerable<LungCare.SupportPlatform.Models.CTDicomInfo> list =
        //        datagrid.Items.SourceCollection.Cast<LungCare.SupportPlatform.Models.CTDicomInfo>();

        //    var items = list.First(item => ((LungCare.SupportPlatform.Models.CTDicomInfo)item).PatientName == PatientName);

        //    LungCare.SupportPlatform.UI.MsgWindow MsgWindow = new LungCare.SupportPlatform.UI.MsgWindow();
        //    MsgWindow.MsgText = items.AdditionalData;

        //    MsgWindow.AllowsTransparency = true;
        //    MsgWindow.Background = System.Windows.Media.Brushes.Transparent;
        //    MsgWindow.OpacityMask = System.Windows.Media.Brushes.White;

        //    MsgWindow.ShowDialog();
        //}
    }

    public class 上传列表Entity
    {
        public string 时间 { get; set; }
        public string 单号 { get; set; }
        public string 患者姓名 { get; set; }
        public string 性别 { get; set; }
        public string 年龄 { get; set; }
        public string 状态 { get; set; }
    }
}

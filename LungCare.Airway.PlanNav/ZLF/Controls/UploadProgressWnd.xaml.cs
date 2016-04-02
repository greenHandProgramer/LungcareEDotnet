using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.UI.Windows.Common;
using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace LungCare.SupportPlatform.UI
{
    /// <summary>
    /// MsgWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UploadProgressWnd : Window
    {
        ProgressArgs _lastProgress;
        UploadFileWorker UploadFileWorker = new UploadFileWorker();

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

        public UploadProgressWnd()
        {
            InitializeComponent();
            borderMessage.MouseDown+=borderMessage_MouseDown;
        }

        private void StartParseDicom()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                lbDicomInfo.Content = string.Empty;

                labelDownloadInfo.Visibility = Visibility.Visible;
                labelDownloadInfo.Content = "正在解析dicom信息";
                lbUploadError.Visibility = System.Windows.Visibility.Hidden;
                uploadProgress.Visibility = System.Windows.Visibility.Hidden;
                uploadProgress.Value = 0;
                btnUploadFile.Visibility = System.Windows.Visibility.Hidden;
            }));
        }

        private void DicomOnError()
        {
            // labelDownloadInfo.Visibility =System.Windows.Visibility.Visible;
            // lbUploadError.Visibility =System.Windows.Visibility.Visible;
            // uploadProgress.Visibility =System.Windows.Visibility.Visible;
            // btnUploadFile.Visibility =System.Windows.Visibility.Visible;
            // btn续传.Visibility =System.Windows.Visibility.Visible;
        }

        private void StartUpload()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                label.Visibility = Visibility.Visible;
                labelDownloadInfo.Visibility = Visibility.Visible;
                labelDownloadInfo.Content = "正在上传";
                lbUploadError.Visibility = System.Windows.Visibility.Hidden;
                uploadProgress.Visibility = System.Windows.Visibility.Visible;
                //uploadProgress.Value = 0;
                btnUploadFile.Visibility = System.Windows.Visibility.Hidden;
            }));
        }

        private void FinishUpload()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                labelDownloadInfo.Visibility = Visibility.Hidden;
                labelDownloadInfo.Content = "上传完毕";
                lbUploadError.Visibility = System.Windows.Visibility.Hidden;
                uploadProgress.Visibility = System.Windows.Visibility.Hidden;
                //btnUploadFile.Visibility = System.Windows.Visibility.Visible;
            }));
        }

        private void UploadErrorOccurred()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                label.Visibility = Visibility.Hidden;
                labelDownloadInfo.Visibility = Visibility.Visible;
                labelDownloadInfo.Content = "上传出错。正在自动重试。" + (_lastProgress==null?string.Empty : string.Format("【已上传：{0}/{1}】",
                    _lastProgress.Finished.HasValue ? HumanReadableFilesize(_lastProgress.Finished.Value) : HumanReadableFilesize(0),
                    HumanReadableFilesize(_lastProgress.Total)));

                lbUploadError.Visibility = System.Windows.Visibility.Visible;
                uploadProgress.Visibility = System.Windows.Visibility.Visible;
                //btnUploadFile.Visibility = System.Windows.Visibility.Visible;
            }));
        }

        public string FileName;

        public string acquisitionDate;
        public string acquisitionTime;
        public string InstitutionName;
        public string PatientAge;
        public string PatientName;
        public string PatientSex;
        public string SeriesInstanceUID;
        public string StudyInstanceUID;
        private long alreadySent;
        public bool IsWin7System
        {
            get { return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major >= 6); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsWin7System)
            {
                btnMin.Visibility = Visibility.Hidden;
            }

            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = null;
            Upload();
        }

        private void Upload()
        {
            StartUpload();

            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                StringBuilder stringUploadinfo = new StringBuilder();
                labelPatientInfo.Content = string.Format("{0}, {1}, {2}, {3}, {4}", PatientName, PatientSex, PatientAge, InstitutionName, acquisitionDate);
                stringUploadinfo.AppendFormat("正在连接服务器");

                if (_lastProgress != null) {
                    stringUploadinfo.Append(string.Format(Environment.NewLine + "【已上传：{0}/{1}】",
                        _lastProgress.Finished.HasValue ? HumanReadableFilesize(_lastProgress.Finished.Value) : HumanReadableFilesize(0),
                        HumanReadableFilesize(_lastProgress.Total)));
                }

                labelDownloadInfo.Content = stringUploadinfo.ToString();
            }));

            UploadFileWorker.CreateOrderThenUploadFileThenUploadDicomInfo(
                 filename: FileName,
                 InstitutionName: InstitutionName,
                 PatientAge: PatientAge,
                 PatientName: PatientName,
                 PatientSex: PatientSex,
                 SeriesInstanceUID: SeriesInstanceUID,
                 StudyInstanceUID: StudyInstanceUID,
                 acquisitionDate: acquisitionDate,
                 acquisitionTime: acquisitionTime,
                 UploadTimestamp: DateTime.Now,
                 successCallback: delegate ()
                 {
                     this.Dispatcher.Invoke(new Action(delegate ()
                     {
                         FinishUpload();
                         //MessageBox.Show("上传成功！");
                         Dicom成功上传 Dicom成功上传 = new Dicom成功上传();
                         Dicom成功上传.Owner = MainWindow.Instance;
                         Dicom成功上传.Evt完成 += delegate
                         {
                             Close();
                             MainWindow.Instance.Switch2UploadList();
                         };
                         Dicom成功上传.Evt继续上传 += delegate
                         {
                             Close();
                             MainWindow.Instance.Switch2UploadWindowThenPopupFileSelectDialog();
                         };
                         Dicom成功上传.ShowDialog();
                         //this.dataGridViewFileList.DataSource = eInner.Result;
                     }));
                 },
                 failureCallback: delegate (string errMsg)
                 {
                     UploadErrorOccurred();
                     //MessageBox.Show(errMsg);

                     if (!cancelUploadManually)
                     {
                         System.Threading.Thread.Sleep(5000);
                         Upload();
                     }
                 },
                 errorCallback: delegate (Exception ex)
                 {
                     UploadErrorOccurred();
                     //Util.ShowExceptionMessage(ex);

                     if (!cancelUploadManually)
                     {
                         System.Threading.Thread.Sleep(5000);
                         Upload();
                     }
                 },
                 uploadProgressCallback: delegate (ProgressArgs eProgress)
                 {
                     this.Dispatcher.Invoke(new Action(delegate ()
                     {
                         _lastProgress = eProgress;

                         alreadySent = eProgress.Finished.Value;

                         //StringBuilder stringUploadinfo = new StringBuilder();
                         //stringUploadinfo.AppendLine("已上传 ：" + HumanReadableFilesize(eProgress.Finished.Value));
                         //if (eProgress.RemainTimeInMillisecond.HasValue)
                         //{
                         //    stringUploadinfo.AppendLine("剩余时间：" + eProgress.RemainTimeInMillisecond.Value.ToString("0.0") + " s");
                         //}
                         //if (eProgress.Percentage.HasValue)
                         //{
                         //    stringUploadinfo.AppendLine("上传进度：" + (eProgress.Percentage.Value * 100).ToString("0.00") + " %");
                         //}
                         //stringUploadinfo.AppendLine("文件大小：" + HumanReadableFilesize(eProgress.Total));
                         //labelDownloadInfo.Content = stringUploadinfo.ToString();

                         StringBuilder stringUploadinfo = new StringBuilder();
                         //stringUploadinfo.AppendLine(string.Format("{0} {1} {2}",PatientName,PatientSex, PatientAge));
                         stringUploadinfo.AppendFormat("【预计剩余时间：{0}，当前速度：{3}，已上传：{1}/{2}】",
                             eProgress.RemainTimeHumanReadable,
                             eProgress.Finished.HasValue ? HumanReadableFilesize(eProgress.Finished.Value) : HumanReadableFilesize(0),
                             HumanReadableFilesize(eProgress.Total),
                             eProgress.Speed.HasValue ? HumanReadableFilesize((long)eProgress.Speed.Value) + "/秒" : "未知");
                         labelDownloadInfo.Content = stringUploadinfo.ToString();

                         uploadProgress.Value = eProgress.Percentage.Value;
                         System.Windows.Forms.Application.DoEvents();

                         Console.ResetColor();
                         stringUploadinfo.Clear();
                     }));
                 }
             );
        }
        
        bool cancelUploadManually = false;

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("是否取消上传文件？取消后不支持续传。需要重新上传。", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                cancelUploadManually = true;
                UploadFileWorker.ManuallyCancelUpload();
                Close();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //if(e.Key == Key.Escape)
            //{
            //    Close();
            //}
        }

        private void borderMessage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //this.DragMove();
        }

        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            Upload();
            //ResumeUpload();
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.BringIntoView();
        }

        private void btnMin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
            MainWindow.Instance.WindowState = System.Windows.WindowState.Minimized;
        }
    }
}

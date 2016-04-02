using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.UI.Windows.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LungCare.SupportPlatform.UI.UserControls.Examination
{
    /// <summary>
    /// UploadListUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadListUserControlMA : UserControl
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

        public DownloadListUserControlMA()
        {
            InitializeComponent();

            btnCancelDownload.Visibility = lbWaiting.Visibility = Visibility.Hidden;
        }

        private void linkLabel_Click(object sender, RoutedEventArgs e)
        {
            

            ThemedControlsLibrary.LinkLabel LinkLabel = (ThemedControlsLibrary.LinkLabel)sender;
            string DataID = LinkLabel.Tag.ToString();
            Console.WriteLine(LinkLabel.Tag.ToString());

            IEnumerable<Models.DataListItem> list = listView.Items.SourceCollection.Cast<Models.DataListItem>();
            var items = list.First(item => ((Models.DataListItem)item).DataID == DataID);
            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = items.DataID;

            this.Cursor = Cursors.Wait;
            lbWaiting.Content = "正在下载，请稍候。";
            btnCancelDownload.Visibility = lbWaiting.Visibility = Visibility.Visible;
            tbSearch.IsEnabled = this.listView.IsEnabled = false;

            //ThreadPool.QueueUserWorkItem(delegate
            //{
            //    Thread.Sleep(1000);
            //    UIUtil.Invoke(this, delegate
            //    {
            //        new VideoPlayer(new FileInfo(@"Jiang^Guoping_M_2015119182046_ccecdc24cebe42818cb7cc768579eea5_vr.m4v").FullName).ShowDialog();
            //        lbWaiting.Visibility = Visibility.Hidden;
            //        IsEnabled = true;
            //    });
            //});

            //return;

            string orderDataFolder = new DirectoryInfo(Path.Combine(Security.SessionManager.UserName, items.DataID)).FullName;
            Console.WriteLine("orderDataFolder  = " + orderDataFolder);

            if (!Directory.Exists(orderDataFolder))
            {
                Console.WriteLine("Creating orderDataFolder = " + orderDataFolder);
                Directory.CreateDirectory(orderDataFolder);
            }
            //string vrAVIPath = Path.Combine(orderDataFolder, "vr.avi");

            //if (File.Exists(vrAVIPath))
            //{
            //    lbWaiting.Visibility = Visibility.Hidden;
            //    IsEnabled = true;
            //    this.Cursor = Cursors.Arrow;

            //    new VideoPlayer(vrAVIPath).ShowDialog();
            //}
            //else
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    WebAPIWorkers.DownloadFileWorker.DownloadFile(
                        云数据类型Enum.处理结果,
                        items.DataID,
                        delegate (string filename)
                        {
                            UIUtil.Invoke(this, delegate
                            {
                                btnCancelDownload.Visibility = lbWaiting.Visibility = Visibility.Hidden;
                                tbSearch.IsEnabled = this.listView.IsEnabled = true;
                                this.Cursor = Cursors.Arrow;
                                lbWaiting.Content = "正在下载，请稍候。";
                                lbWaiting.Visibility = Visibility.Visible;
                                string lowerext = new FileInfo(filename).Extension.ToString();

                            });
                        },
                        delegate (string errMsg)
                        {
                            UIUtil.Invoke(this, delegate
                            {
                                btnCancelDownload.Visibility = lbWaiting.Visibility = Visibility.Hidden;
                                tbSearch.IsEnabled = this.listView.IsEnabled = true;
                                this.Cursor = Cursors.Arrow;
                                lbDownloadProgress.Content = "下载失败，请重试";

                                MessageBox.Show(errMsg);
                            });
                        },
                        delegate (Exception ex)
                        {
                            UIUtil.Invoke(this, delegate
                            {
                                btnCancelDownload.Visibility = lbWaiting.Visibility = Visibility.Hidden;
                                tbSearch.IsEnabled = this.listView.IsEnabled = true;
                                this.Cursor = Cursors.Arrow;
                                lbDownloadProgress.Content = "下载失败，请重试";

                                WebAPIWorkers.Util.ShowExceptionMessage(ex);
                            });
                        },
                        delegate (ProgressArgs progressArgs)
                        {
                            UIUtil.Invoke(this, delegate
                            {
                                lbWaiting.Content = "正在下载，请稍候。" + progressArgs.ToString();
                                lbDownloadProgress.Content = progressArgs.ToString();

                                Console.WriteLine(progressArgs);
                            });
                        }
                    );
                });
            }

            //new MESPDownloadUpload().DownloadFileAsync(
            //    );

            //DownloadFileAsync
            return;

            UI.MsgWindow MsgWindow = new UI.MsgWindow();
            MsgWindow.MsgText = string.Format(@"患者姓名：{0} 年龄：{1}
20150920 09:10 上传CT
20150920 10:10 后台核对图像
.......


如果图像无法处理，这里显示拒绝原因！", items.PatientSex, items.PatientAge);
            MsgWindow.MsgText = items.AdditionalData;

            MsgWindow.AllowsTransparency = true;
            MsgWindow.Background = System.Windows.Media.Brushes.Transparent;
            MsgWindow.OpacityMask = System.Windows.Media.Brushes.White;

            MsgWindow.ShowDialog();
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

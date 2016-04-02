using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.ServiceModel;
using System.Threading;
using System.IO;
using LungCare.SupportPlatform.Network;
using LK_DW_NK_ZModelCRFChart;
using System.ComponentModel;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;
using LungCare.SupportPlatform.Models;
using LungCare.SupportPlatform.WebAPIWorkers;
using LungCare.SupportPlatform.Security;

namespace LungCare_Airway_PlanNav
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        public void Switch2PersonalCenter()
        {
            tabControl.SelectedIndex = 4;
        }

        public void Switch2UploadWindow()
        {
            tabControl.SelectedIndex = 1;
        }
        public void Switch2UploadList()
        {
            tabControl.SelectedIndex = 2;
        }

        public void DisableAll()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                this.Cursor = Cursors.Wait;
                btnShowMessage.IsEnabled = btnExit.IsEnabled = false;
                tabItemClientService.IsEnabled = tabItemDownloadList.IsEnabled = tabItemMyCenter.IsEnabled =
                    tabItemUploadCT.IsEnabled = tabItemUploadList.IsEnabled = false;
            }));
        }

        public void EnableAll()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                this.Cursor = Cursors.Arrow;
                btnShowMessage.IsEnabled = btnExit.IsEnabled = true;
                tabItemClientService.IsEnabled = tabItemDownloadList.IsEnabled = tabItemMyCenter.IsEnabled =
                    tabItemUploadCT.IsEnabled = tabItemUploadList.IsEnabled = true;
            }));
        }

        private DispatcherTimer timer;
        public MainWindow()
        {
            this.InitializeComponent();
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;// 获得窗体的 样式

            int oldstyle = MedSys.PresentationCore.AdjustWindow.NativeMethods.GetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE, oldstyle & ~MedSys.PresentationCore.AdjustWindow.NativeMethods.WS_CAPTION);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetLayeredWindowAttributes(hwnd, 1 | 2 << 8 | 3 << 16, 0, MedSys.PresentationCore.AdjustWindow.NativeMethods.LWA_ALPHA);

            this.RefreshRoundWindowRect();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.SizeChanged += MainWindow_SizeChanged;
            //this.Width = SystemParameters.WorkArea.Width;
            //this.Height = SystemParameters.WorkArea.Height;
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = TimeSpan.FromMilliseconds(1000);

            headBorder.MouseDown += headBorder_MouseDown;
            loadAllData();

            Uri uri = new Uri("../Images/1.jpg", UriKind.Relative);
            BitmapImage bitmap = new BitmapImage(uri);

            //listMessages = new List<MessageEntity>();
            //for (int i = 0; i < 10; i++)
            //{
            //    MessageEntity mess = new MessageEntity();
            //    mess.SentBy = "系统管理员";
            //    mess.Content = "恭喜您，医生资格证书已上传成功";
            //    listMessages.Add(mess);
            //}

            //dataGridMessage.DataContext = listMessages;

            //loadCertifyImage();
            //loadUserIconImage();
            //LoadUserInfo();

            LoadNotification();


            btnSavePassword.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF59275A"));
            btnSaveForgetPassword.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF59275A"));

            //personImage.Source = bitmap;
            //return;
        }

        private GetNotificationsResponse _notification;
        public MainWindow(GetUserInfoResponse userInfo, DownloadUserIconRespnse userIcon, DownloadCertificateRespnse certificate,
            GetCertificateApproveStatusResponse certificateStatus, GetNotificationsResponse notification)
        {
            this.InitializeComponent();

            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;// 获得窗体的 样式

            int oldstyle = MedSys.PresentationCore.AdjustWindow.NativeMethods.GetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE, oldstyle & ~MedSys.PresentationCore.AdjustWindow.NativeMethods.WS_CAPTION);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetLayeredWindowAttributes(hwnd, 1 | 2 << 8 | 3 << 16, 0, MedSys.PresentationCore.AdjustWindow.NativeMethods.LWA_ALPHA);

            this.RefreshRoundWindowRect();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.SizeChanged += MainWindow_SizeChanged;
            //this.Width = SystemParameters.WorkArea.Width;
            //this.Height = SystemParameters.WorkArea.Height;
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = TimeSpan.FromMilliseconds(1000);

            headBorder.MouseDown += headBorder_MouseDown;
            loadAllData();

            Uri uri = new Uri("../Images/1.jpg", UriKind.Relative);
            BitmapImage bitmap = new BitmapImage(uri);

            //listMessages = new List<MessageEntity>();
            //for (int i = 0; i < 10; i++)
            //{
            //    MessageEntity mess = new MessageEntity();
            //    mess.SentBy = "系统管理员";
            //    mess.Content = "恭喜您，医生资格证书已上传成功";
            //    listMessages.Add(mess);
            //}

            //dataGridMessage.DataContext = listMessages;

            this._downloadUserIcon = userIcon;
            this._userInfo = userInfo;
            this._certificateRespnse = certificate;
            this._certificateApproveStatus = certificateStatus;
            this._notification = notification;
            if (_userInfo != null)
            {
                tbUserName.Text = _userInfo.ChineseName + "  医生";
                usernameHome.Text = _userInfo.ChineseName;
                tbHospitalName.Text = _userInfo.Institution + "  " + _userInfo.Department;
                userNameHead.Text = "Hi , " + _userInfo.ChineseName;
            }

            if (_certificateApproveStatus != null)
            {
                string status = "未知";
                switch (_certificateApproveStatus.Result)
                {
                    case "YES":
                        status = "通过审核";
                        break;
                    case "NO":
                        status = "没有通过审查:" + _certificateApproveStatus.RejectReason;
                        break;
                    case "NotUploadedYet":
                        status = "未上传行医执照";
                        break;
                    case "WaitingApprove":
                        status = "等待工作人员审核";
                        break;
                    default:
                        break;
                }
                lbCertificateApproveStatus.Content = status;
            }

            if (_downloadUserIcon != null)
            {
                if (_downloadUserIcon.UserIconImage != null)
                {
                    BitmapSource bitmapHead = BitmapSourceFromBase64(_downloadUserIcon.UserIconImage);
                    personHeadImage.Source = bitmapHead;
                    personHeadImageHead.Source = bitmapHead;
                }
            }

            if (_certificateRespnse != null)
            {
                if (_certificateRespnse.CertificateImage != null)
                {
                    imagePersonCertify.Source = BitmapSourceFromBase64(_certificateRespnse.CertificateImage);

                }
                else
                {
                    lbCertificateApproveStatus.Content = "未上传行医执照";
                }
            }



            //loadCertifyImage();
            //loadUserIconImage();
            //LoadUserInfo();

            if (this._notification != null)
            {
                if (this._notification.NotificationList != null)
                {
                    this.listMessages = this._notification.NotificationList;
                    this.dataGridMessage.DataContext = this._notification.NotificationList;
                    tbMessageNum.Text = this._notification.NotificationList.Count.ToString();
                }
            }


            btnSavePassword.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF59275A"));
            btnSaveForgetPassword.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF59275A"));
            //personImage.Source = bitmap;
            //return;
        }

        private List<NotificationItem> listMessages;
        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.RefreshRoundWindowRect();
            //throw new NotImplementedException();
        }

        private void RefreshRoundWindowRect()
        {
            // 获取窗体句柄
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

            // 创建圆角窗体
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowRgn(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.CreateRoundRectRgn(0, 0, Convert.ToInt32(this.ActualWidth) + 1, Convert.ToInt32(this.ActualHeight) + 1, 5, 5), true);
        }

        private Loading loading;
        public void CreateCounterWindowThread()
        {
            //this.Invoke(new EventHandler(delegate
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                loading = new LK_DW_NK_ZModelCRFChart.Loading();
                loading.Show();
                //loading.Hide();
                //loading.Close();    
            }));

            //System.Windows.Threading.Dispatcher.Run();
            //loading.Closed += (s, e) =>
            //loading.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Background);
        }

        private Thread newWindowThread;
        private void loadAllData()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                RetrieveDataListWorker.SendRetrieveDataListRequest(
                    successCallback:
                    delegate (RetrieveDataListResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            UploadListUserControl.SetDataSource(response.DataList);
                            DownloadListUserControl.SetDataSource(response.DataList);
                        }));
                    },
                    failureCallback:
                    delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("获取数据列表失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback:
                    delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "获取数据列表出错。");
                        }));
                    });
            });
        }

        private void loadAllDataMESP()
        {
            newWindowThread = new Thread(new ThreadStart(CreateCounterWindowThread));
            newWindowThread.Start();

            new MESPDownloadUpload().FetchFileListAsync(
                new EventHandler<ExceptionArgs>(delegate (Object senderInner, ExceptionArgs eInner)
                {
                    MessageBox.Show(eInner.Exception.Message);
                }),
                new EventHandler<FileListFinishedArgs>(delegate (Object senderInner, FileListFinishedArgs eInner)
                {
                    Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        int counter = 0;
                        List<CTDicomInfo> ctDicomInfoList = new List<CTDicomInfo>();
                        foreach (var item in eInner.Result)
                        {
                            if (item.FileName.EndsWith(".info"))
                            {
                                counter++;
                                new MESPDownloadUpload().DownloadFileAsync(
                                    item.FileName,
                                    (int)item.FileSize,
                                    new EventHandler<ExceptionArgs>(delegate (Object senderInnerDownload, ExceptionArgs eInnerDownload)
                                    {
                                        MessageBox.Show(eInnerDownload.Exception.Message);
                                    }),
                                    new EventHandler<FileDownloadFinishedArgs>(delegate (Object senderInnerDownload, FileDownloadFinishedArgs eInnerDownload)
                                    {
                                        Thread.Sleep(100);
                                        string text = File.ReadAllText(item.FileName);
                                        ctDicomInfoList.Add(JsonConvert.DeserializeObject<LungCare.SupportPlatform.Models.CTDicomInfo>(text));
                                        counter--;
                                    }),
                                    new EventHandler<ProgressArgs>(delegate (Object senderInnerDownload, ProgressArgs eInnerDownload)
                                    {
                                        Console.ResetColor();
                                        Console.WriteLine(eInner.ToString());
                                    })
                                );
                            }
                        }

                        while (counter != 0)
                        {
                            Thread.Sleep(100);
                        }

                        //UploadListUserControl.SetDataSource(ctDicomInfoList);
                        //this.datagrid.DataContext = ctDicomInfoList;
                        //this.datagrid_Copy.DataContext = ctDicomInfoList;
                        loading.Hide();
                        loading.Close();
                    }));
                }));
        }

        void headBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //this.DragMove();
            //throw new NotImplementedException();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Instance = this;
        }


        private void timer_Tick(object sender, EventArgs e)
        {
            //if (mediaElement.Source != null)
            //{
            //    if (mediaElement.NaturalDuration.HasTimeSpan)
            //    {
            //        lbVideoTime.Content = String.Format("{0} / {1}", mediaElement.Position.ToString(@"hh\:mm\:ss"), mediaElement.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss"));
            //    }
            //}
        }
        private void BtnChoosePatient_Click(object sender, RoutedEventArgs e)
        {
            (sender as ToggleButton).IsChecked = true;
        }

        private void gd_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void bdDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("退出导航吗？", "", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Question, System.Windows.Forms.MessageBoxDefaultButton.Button2, System.Windows.Forms.MessageBoxOptions.DefaultDesktopOnly);
            if (result.ToString() == "OK")
            {
                this.Close();
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                return;
            }
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            //resultGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
            //resultGrid.RowDefinitions[1].Height = new GridLength(1.0, GridUnitType.Star);
            //System.Windows.Forms.Application.DoEvents();

            return;
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

            return Math.Round(size1) + units[i];
        }


        private void LinkLabel_Click(object sender, RoutedEventArgs e)
        {

            //var a = this.datagrid.SelectedItem;
            //var b = a as FileListItem;

            //new MESPDownloadUpload().DownloadFileAsync(b.FileName, (int)b.FileSize,
            //    new EventHandler<ExceptionArgs>(delegate(Object senderInner, ExceptionArgs eInner)
            //    {
            //        MessageBox.Show(eInner.Exception.Message);
            //    }),
            //    new EventHandler<FileDownloadFinishedArgs>(delegate(Object senderInner, FileDownloadFinishedArgs eInner)
            //    {
            //        //this.Invoke(new EventHandler(delegate
            //        this.Dispatcher.BeginInvoke(new Action(delegate()
            //        {
            //            //this.dataGridViewFileList.DataSource = eInner.Result;
            //            MessageBox.Show("DONE");
            //        }));
            //    }),
            //    new EventHandler<ProgressArgs>(delegate(Object senderInner, ProgressArgs eInner)
            //    {
            //        //this.Invoke(new EventHandler(delegate
            //        //{
            //        Console.ResetColor();
            //        Console.WriteLine(eInner.ToString());
            //        //}));
            //    }));
            ////MessageBox.Show("mlx : " + b);
        }

        private void LinkLabelPlay_Click(object sender, RoutedEventArgs e)
        {
            ////return;
            //var a = this.datagrid_Copy.SelectedItem;
            //var b = a as FileListItem;

            //string filename =  System.Windows.Forms.Application.StartupPath+"\\"+b.FileName;
            ////filename = @"C:\Users\john\Desktop\download1.avi";
            //if (File.Exists(filename))
            //{
            //    if (System.IO.Path.GetExtension(filename) == ".avi")
            //    {
            //        StartPlay(filename);
            //    }
            //}
            //else
            //{
            //    downloadProgress.Visibility = System.Windows.Visibility.Visible;

            //    tbDownloading.Visibility = System.Windows.Visibility.Visible;
            //    lbdownloading.Visibility = System.Windows.Visibility.Visible;
            //    StringBuilder stringDownloadInfo = new StringBuilder();
            //    new MESPDownloadUpload().DownloadFileAsync(b.FileName, (int)b.FileSize,
            //        new EventHandler<ExceptionArgs>(delegate(Object senderInner, ExceptionArgs eInner)
            //        {
            //            MessageBox.Show(eInner.Exception.Message);
            //        }),
            //        new EventHandler<FileDownloadFinishedArgs>(delegate(Object senderInner, FileDownloadFinishedArgs eInner)
            //        {
            //            //this.Invoke(new EventHandler(delegate
            //            this.Dispatcher.BeginInvoke(new Action(delegate()
            //            {
            //                //this.dataGridViewFileList.DataSource = eInner.Result;
            //                //MessageBox.Show("DONE");
            //                downloadProgress.Visibility = System.Windows.Visibility.Hidden;
            //                tbDownloading.Visibility = System.Windows.Visibility.Hidden;
            //                lbdownloading.Visibility = System.Windows.Visibility.Hidden;
            //                if (File.Exists(filename))
            //                {
            //                    if (System.IO.Path.GetExtension(filename) == ".avi")
            //                    {
            //                        StartPlay(filename);
            //                    }
            //                }
            //            }));
            //        }),
            //        new EventHandler<ProgressArgs>(delegate(Object senderInner, ProgressArgs eInner)
            //        {
            //            this.Dispatcher.BeginInvoke(new Action(delegate()
            //            {
            //                stringDownloadInfo.AppendLine("已下载 ：" + HumanReadableFilesize(eInner.Finished.Value));
            //                if (eInner.RemainTimeInMillisecond.HasValue)
            //                {
            //                    stringDownloadInfo.AppendLine("剩余时间：" + eInner.RemainTimeInMillisecond.Value.ToString("0.0") + " s");
            //                }
            //                if (eInner.Percentage.HasValue)
            //                {
            //                    stringDownloadInfo.AppendLine("下载进度：" + (eInner.Percentage.Value * 100).ToString("0.00") + " %");
            //                }
            //                stringDownloadInfo.AppendLine("文件大小：" + HumanReadableFilesize(eInner.Total));


            //                lbdownloading.Content = stringDownloadInfo.ToString();

            //                uploadProgress.Value = eInner.Percentage.Value;
            //                System.Windows.Forms.Application.DoEvents();

            //                downloadProgress.Value = eInner.Percentage.Value;
            //                System.Windows.Forms.Application.DoEvents();
            //                Console.ResetColor();
            //                Console.WriteLine(eInner.ToString());

            //                stringDownloadInfo.Clear();
            //            }));
            //        }));

            //    //MessageBox.Show("文件不存在，无法播放！");
            //}
        }


        private void StartPlay(string filename)
        {
            //resultGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
            //resultGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            //System.Windows.Forms.Application.DoEvents();

            //mediaElement.Source = new Uri(filename);
            //mediaElement.Play();
            //if (mediaElement.NaturalDuration.HasTimeSpan)
            //{
            //    videoSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds - 1;
            //}

            //videoSlider.IsEnabled = true;
            //videoSlider.Value = 0;
            //lbVideoTime.Content = "";

            timer.Start();
        }


        private void StopPlay()
        {
            //if (mediaElement.Source != null)
            //{
            //    mediaElement.Pause();
            //    mediaElement.Source = null;
            //}
            //timer.Stop();
            //lbVideoTime.Content = "";
            //videoSlider.Value = 0;
            //resultGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            //resultGrid.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Star);
            //System.Windows.Forms.Application.DoEvents();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StopPlay();
        }

        private void videoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //mediaElement.Position = TimeSpan.FromSeconds(videoSlider.Value);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 0;
            exitQueryForm.Visibility = System.Windows.Visibility.Visible;
            //System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("退出导航吗？", "",System.Windows.Forms.MessageBoxButtons.YesNo);
            //if (result== System.Windows.Forms.DialogResult.Yes)
            //{
            //    Application.Current.Shutdown();
            //}
            //else
            //{
            //    return;
            //} 
        }

        private void LoadNotification()
        {

            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.NotificationsWorker.GetNotificationRequest(
                    this._userInfo.PhoneNumber,
                    successCallback: delegate(LungCare.SupportPlatform.Models.GetNotificationsResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            this._notification = response;
                            if (this._notification != null)
                            {
                                if (this._notification.NotificationList != null)
                                {
                                    this.listMessages = this._notification.NotificationList;
                                    this.dataGridMessage.DataContext = this._notification.NotificationList;
                                    tbMessageNum.Text = this._notification.NotificationList.Count.ToString();
                                }
                            }
                            //ResumeGUI();
                        }));
                    },
                    failureCallback: delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show(failureReason);
                        }));
                    },
                    errorCallback: delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "获取消息列表出错。");
                        }));
                    });
            });
        }
        private void btnShowMessage_Click(object sender, RoutedEventArgs e)
        {
            LoadNotification();
            tabControl.SelectedIndex = 4;
            tabcontrolPerson.SelectedIndex = 1;


           
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                int tab = this.tabControl.SelectedIndex;
                if (tab == 2 || tab == 3)
                {
                    loadAllData();
                }
            }
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            MedSys.PresentationCore.AdjustWindow.ChangeWindowSize changeWindowSize = new MedSys.PresentationCore.AdjustWindow.ChangeWindowSize(this);
            changeWindowSize.RegisterHook();
        }

        private void btnChangePersonInfo_Click(object sender, RoutedEventArgs e)
        {
            GetCertificateApproveStatus();

            tabcontrolPerson.SelectedIndex = 0;
        }

        private void btnListMessage_Click(object sender, RoutedEventArgs e)
        {

            LoadNotification();
           tabcontrolPerson.SelectedIndex = 1;

        }

        private void btnModifyPassword_Click(object sender, RoutedEventArgs e)
        {
            tabcontrolPerson.SelectedIndex = 2;
        }


        /// <summary>
        /// 上传医生资格证照片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUploadPersonImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FileInfo file = new FileInfo(openFileDialog.FileName);
                string extension = file.Extension;
                if (extension == ".bmp" || extension == ".jpg" || extension == ".png" || extension == ".gif" || extension == ".BMP" || extension == ".JPG" || extension == ".PNG" || extension == ".GIF")
                {

                }
                else
                {
                    MessageBox.Show("请上传图片文件!");
                    return;
                }

                if (file.Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("文件不能超过5M");
                    return;
                }

                lbCertificateApproveStatus.Content = "正在上传...";
                PauseGUI();
                Uri uri = new Uri(openFileDialog.FileName);
                BitmapImage bitmap1 = new BitmapImage();
                //if (file.Length > 200 * 1024)
                if(false)
                {
                    bitmap1.BeginInit();
                    bitmap1.UriSource = uri;
                    bitmap1.DecodePixelWidth = 100;
                    bitmap1.DecodePixelHeight = 100;
                    bitmap1.EndInit();
                }
                else
                {
                    bitmap1 = new BitmapImage(uri);
                }

                imagePersonCertify.Source = bitmap1;

                string base64String = ToBase64(bitmap1);

                string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/UploadCertificate";

                UploadCertificateRequest userIconRequest = new UploadCertificateRequest();
                userIconRequest.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
                userIconRequest.CertificateImage = base64String;
                userIconRequest.Sender = "PC Client";


                ThreadPool.QueueUserWorkItem(delegate
                {
                    LungCare.SupportPlatform.WebAPIWorkers.UploadCertificateWorker.SendUploadCertificateRequest(
                       base64String,
                        successCallback: delegate (LungCare.SupportPlatform.Models.UploadCertificateResponse response)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {

                                ResumeGUI();
                                //lbCertificateApproveStatus.Content = "等待工作人员审核";
                                GetCertificateApproveStatus();
                                _userInfo.CertificateImage = base64String;
                                MessageBox.Show("资格证书上传成功！");
                            }));
                        },
                        failureCallback: delegate (string failureReason)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {

                                ResumeGUI();
                                lbCertificateApproveStatus.Content = _certificateApproveStatus.Result;
                                MessageBox.Show(failureReason);
                            }));
                        },
                        errorCallback: delegate (Exception ex)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                ResumeGUI();
                                LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "上传资格证书出错。");
                                lbCertificateApproveStatus.Content = _certificateApproveStatus.Result;
                            }));
                        });
                });

                return;
                new Thread(new ThreadStart(delegate
                {
                    using (var client = new HttpClient())
                    {
                        string serializedProduct = JsonConvert.SerializeObject(userIconRequest);
                        Console.WriteLine(URI);
                        Console.WriteLine(serializedProduct);

                        var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                        try
                        {
                            client.PostAsync(URI, content).ContinueWith((postTask) =>
                            {
                                if (postTask.Status == TaskStatus.Canceled)
                                {
                                    return;
                                }

                                HttpStatusCode statusCode = postTask.Result.StatusCode;
                                Console.WriteLine(statusCode + Environment.NewLine);

                                postTask.Result.Content.ReadAsStringAsync().ContinueWith((readStringTask) =>
                                {
                                    string responseString = readStringTask.Result;
                                    Console.WriteLine(responseString);

                                    UploadUserIconResponse response =
                                        JsonConvert.DeserializeObject<UploadUserIconResponse>(responseString);

                                    if (response.Success)
                                    {
                                        Dispatcher.Invoke(new Action(delegate
                                        {
                                            MessageBox.Show(response.Success.ToString());
                                            Cursor = Cursors.Arrow;
                                            this.IsEnabled = true;
                                            //imagePersonCertify.Source = ;
                                        }));

                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(new Action(delegate
                                        {
                                            MessageBox.Show(response.ErrorMsg.ToString());
                                            Cursor = Cursors.Arrow;
                                            this.IsEnabled = true;

                                        }));
                                        return;
                                    }

                                });
                            }).Wait();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                            Cursor = Cursors.Arrow;
                            this.IsEnabled = true;
                        }
                    }
                })).Start();
            }
        }



        private void LoadNotification1()
        {

            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.NotificationsWorker.GetNotificationRequest(
                    this._userInfo.PhoneNumber,
                    successCallback: delegate (LungCare.SupportPlatform.Models.GetNotificationsResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {

                            dataGridMessage.DataContext = response.NotificationList;
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show(failureReason);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "登录出错。");
                        }));
                    });
            });
        }

        private DownloadCertificateRespnse _certificateRespnse;
        private void loadCertifyImage()
        {
            string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/DownloadCertificate";
            DownloadCertificateRequest userIconRequest = new DownloadCertificateRequest();
            userIconRequest.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            userIconRequest.Sender = "PC Client";

            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.DownloadCertificateWorker.SendDownloadCertificateRequest(
                    successCallback: delegate (LungCare.SupportPlatform.Models.DownloadCertificateRespnse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            this._certificateRespnse = response;
                            imagePersonCertify.Source = BitmapSourceFromBase64(response.CertificateImage);
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show(failureReason);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {

                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "下载资格证出错。");
                        }));
                    });
            });


            return;
            new Thread(new ThreadStart(delegate
            {
                using (var client = new HttpClient())
                {
                    string serializedProduct = JsonConvert.SerializeObject(userIconRequest);
                    Console.WriteLine(URI);
                    Console.WriteLine(serializedProduct);

                    var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                    try
                    {
                        client.PostAsync(URI, content).ContinueWith((postTask) =>
                        {
                            if (postTask.Status == TaskStatus.Canceled)
                            {
                                MessageBox.Show("Download image cancelled.");
                                return;
                            }

                            HttpStatusCode statusCode = postTask.Result.StatusCode;
                            Console.WriteLine(statusCode + Environment.NewLine);

                            postTask.Result.Content.ReadAsStringAsync().ContinueWith((readStringTask) =>
                            {
                                string responseString = readStringTask.Result;
                                Console.WriteLine(responseString);

                                DownloadCertificateRespnse response =
                                    JsonConvert.DeserializeObject<DownloadCertificateRespnse>(responseString);

                                if (response.Success)
                                {
                                    Dispatcher.Invoke(new Action(delegate
                                    {
                                        MessageBox.Show(response.Success.ToString());
                                        imagePersonCertify.Source = BitmapSourceFromBase64(response.CertificateImage);
                                    }));
                                }
                                else
                                {
                                    Dispatcher.Invoke(new Action(delegate
                                    {
                                        MessageBox.Show(response.ErrorMsg.ToString());

                                    }));
                                    return;
                                }

                            });
                        }).Wait();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            })).Start();
        }


        private DownloadUserIconRespnse _downloadUserIcon;
        private void loadUserIconImage()
        {
            string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/DownloadUserIcon";
            DownloadUserIconRequest userIconRequest = new DownloadUserIconRequest();
            userIconRequest.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            userIconRequest.Sender = "PC Client";

            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.DownloadUserIconWorker.SendUserIconRequest(
                    successCallback: delegate (LungCare.SupportPlatform.Models.DownloadUserIconRespnse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            this._downloadUserIcon = response;
                            personHeadImage.Source = BitmapSourceFromBase64(response.UserIconImage);
                            personHeadImageHead.Source = BitmapSourceFromBase64(response.UserIconImage);
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show(failureReason);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {

                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "下载用户头像出错。");
                        }));
                    });
            });
            return;
            new Thread(new ThreadStart(delegate
            {
                using (var client = new HttpClient())
                {
                    string serializedProduct = JsonConvert.SerializeObject(userIconRequest);
                    Console.WriteLine(serializedProduct);

                    var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                    try
                    {
                        client.PostAsync(URI, content).ContinueWith((postTask) =>
                        {
                            if (postTask.Status == TaskStatus.Canceled)
                            {
                                return;
                            }

                            HttpStatusCode statusCode = postTask.Result.StatusCode;
                            Console.WriteLine(statusCode + Environment.NewLine);

                            postTask.Result.Content.ReadAsStringAsync().ContinueWith((readStringTask) =>
                            {
                                string responseString = readStringTask.Result;
                                Console.WriteLine(responseString);

                                DownloadUserIconRespnse response = JsonConvert.DeserializeObject<DownloadUserIconRespnse>(responseString);
                                if (response == null)
                                {
                                    MessageBox.Show("服务端没有响应！");
                                    return;
                                }
                                if (response.Success)
                                {
                                    Dispatcher.Invoke(new Action(delegate
                                    {
                                        MessageBox.Show(response.Success.ToString());
                                        personHeadImage.Source = BitmapSourceFromBase64(response.UserIconImage);
                                    }));

                                }
                                else
                                {
                                    Dispatcher.Invoke(new Action(delegate
                                    {
                                        MessageBox.Show(response.ErrorMsg.ToString());

                                    }));
                                    return;
                                }

                            });
                        }).Wait();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            })).Start();
        }

        private GetUserInfoResponse _userInfo;
        private void LoadUserInfo()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.RetrieveUserInfoWorker.SendUserInfoRequest(
                    successCallback: delegate (LungCare.SupportPlatform.Models.GetUserInfoResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            this._userInfo = response;
                            tbUserName.Text = response.ChineseName + "  医生";
                            usernameHome.Text = response.ChineseName;
                            tbHospitalName.Text = response.Institution + "  " + response.Department;
                            userNameHead.Text = "Hi , " + response.ChineseName;

                            GetCertificateApproveStatus();
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show(failureReason);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {

                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "获取用户信息出错。");
                        }));
                    });
            });
        }


        public GetCertificateApproveStatusResponse _certificateApproveStatus;

        private void GetCertificateApproveStatus()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.GetCertificateApproveStatusWorker.SendGetCertificateApproveStatusRequeset(
                    this._userInfo.PhoneNumber,
                    successCallback: delegate (LungCare.SupportPlatform.Models.GetCertificateApproveStatusResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            string status = "未知";
                            _certificateApproveStatus = response;
                            switch (_certificateApproveStatus.Result)
                            {
                                case "YES":
                                    status = "通过审核";
                                    break;
                                case "NO":
                                    status = "没有通过审查";
                                    break;
                                case "NotUploadedYet":
                                    status = "未上传行医执照";
                                    break;
                                case "WaitingApprove":
                                    status = "等待工作人员审核";
                                    break;
                                default:
                                    break;
                            }
                            lbCertificateApproveStatus.Content = status;
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            lbCertificateApproveStatus.Content = _certificateApproveStatus.Result;
                            if (failureReason == null)
                            {
                                // MessageBox.Show("该用户还未上传资格证书照片！");
                            }
                            else
                            {
                                MessageBox.Show(failureReason);
                            }

                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {

                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "获取证书审查状态出错。");
                        }));
                    });
            });
        }
        private void btnForgetPassword_Click(object sender, RoutedEventArgs e)
        {
            this.tabcontrolPerson.SelectedIndex = 3;
        }

        string ToBase64(BitmapSource bitmapSource)
        {
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        BitmapSource BitmapSourceFromBase64(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            using (var stream = new MemoryStream(Convert.FromBase64String(value)))
            {
                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                BitmapSource result = decoder.Frames[0];
                result.Freeze();
                return result;
            }
        }

        private void PauseGUI()
        {
            ((UIElement)Content).IsEnabled = false;
            Cursor = Cursors.Wait;
        }

        private void ResumeGUI()
        {
            ((UIElement)Content).IsEnabled = true;
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// 上传个人头像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChangeHeaderImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FileInfo file = new FileInfo(openFileDialog.FileName);
                string extension = file.Extension;
                if (extension == ".bmp" || extension == ".jpg" || extension == ".png" || extension == ".gif" || extension == ".BMP" || extension == ".JPG" || extension == ".PNG" || extension == ".GIF")
                {

                }
                else
                {
                    MessageBox.Show("请上传图片文件!");
                    return;
                }
                if (file.Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("文件不能超过5M");
                    return;
                }
                btnChangeHeaderImage.Content = "正在上传..";
                PauseGUI();
                Uri uri = new Uri(openFileDialog.FileName);
                BitmapImage bitmap1 = new BitmapImage();
                //if (file.Length > 200 * 1024)
                if(false)
                {
                    bitmap1.BeginInit();
                    bitmap1.UriSource = uri;
                    bitmap1.DecodePixelWidth = 100;
                    bitmap1.DecodePixelHeight = 100;
                    bitmap1.EndInit();
                }
                else
                {
                    bitmap1 = new BitmapImage(uri);
                }

                personHeadImage.Source = bitmap1;
                personHeadImageHead.Source = bitmap1;
                string base64String = ToBase64(bitmap1);




                string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/UploadUserIcon";


                UploadUserIconRequest userIconRequest = new UploadUserIconRequest();
                userIconRequest.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
                userIconRequest.UserIconImage = base64String;
                userIconRequest.Sender = "PC Client";


                ThreadPool.QueueUserWorkItem(delegate
                {
                    LungCare.SupportPlatform.WebAPIWorkers.UploadUserIconWorker.SendUserIconRequest(
                        base64String,
                        successCallback: delegate (LungCare.SupportPlatform.Models.UploadUserIconResponse response)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {


                                ResumeGUI();
                                btnChangeHeaderImage.Content = "更换照片";
                                MessageBox.Show("上传头像成功!");
                            }));
                        },
                        failureCallback: delegate (string failureReason)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {

                                MessageBox.Show(failureReason);

                                ResumeGUI();
                                btnChangeHeaderImage.Content = "更换照片";
                            }));
                        },
                        errorCallback: delegate (Exception ex)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {

                                ResumeGUI();
                                btnChangeHeaderImage.Content = "更换照片";
                                LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "上传头像出错。");
                            }));
                        });
                });


                return;
                new Thread(new ThreadStart(delegate
                {
                    using (var client = new HttpClient())
                    {
                        string serializedProduct = JsonConvert.SerializeObject(userIconRequest);
                        Console.WriteLine(serializedProduct);

                        var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                        try
                        {
                            client.PostAsync(URI, content).ContinueWith((postTask) =>
                            {
                                if (postTask.Status == TaskStatus.Canceled)
                                {
                                    return;
                                }

                                HttpStatusCode statusCode = postTask.Result.StatusCode;
                                Console.WriteLine(statusCode + Environment.NewLine);

                                postTask.Result.Content.ReadAsStringAsync().ContinueWith((readStringTask) =>
                                {
                                    string responseString = readStringTask.Result;
                                    Console.WriteLine(responseString);

                                    LungCare.SupportPlatform.Models.UploadUserIconResponse response =
                                        JsonConvert.DeserializeObject<LungCare.SupportPlatform.Models.UploadUserIconResponse>(responseString);

                                    if (response.Success)
                                    {
                                        Dispatcher.Invoke(new Action(delegate
                                        {
                                            MessageBox.Show(response.Success.ToString());
                                            Cursor = Cursors.Arrow;
                                            this.IsEnabled = true;
                                        }));

                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(new Action(delegate
                                        {
                                            MessageBox.Show(response.ErrorMsg.ToString());
                                            Cursor = Cursors.Arrow;
                                            this.IsEnabled = true;

                                        }));
                                        return;
                                    }

                                });
                            }).Wait();
                        }
                        catch (Exception ex)
                        {
                            Cursor = Cursors.Arrow;
                            this.IsEnabled = true;
                        }
                    }
                })).Start();
            }
        }

        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbTitle.Text))
            {
                MessageBox.Show("请先填写标题！");
                return;
            }

            if (string.IsNullOrEmpty(tbContent.Text))
            {
                MessageBox.Show("请先填写内容!");
                return;
            }
            string title = tbTitle.Text;
            string content = tbContent.Text;
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.FeedbackWorker.FeedbackRequest(
                    title,
                    content,
                    _userInfo.PhoneNumber,
                    successCallback: delegate (LungCare.SupportPlatform.Models.FeedbackResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            MessageBox.Show("意见反馈成功! ");
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {

                            MessageBox.Show(failureReason);

                            ResumeGUI();
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {

                            ResumeGUI();
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "意见反馈出错。");
                        }));
                    });
            });
        }

        private void btnCancelCommit_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("确定取消提交吗？", "", System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                tbTitle.Text = "";
                tbContent.Text = "";
            }
        }

        private void btnCheckVersion_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("该版本已是最新版本！");
        }

        private void btnDeleteMessage_Click(object sender, RoutedEventArgs e)
        {

            if (dataGridMessage.Items.Count <= 0)
            {
                MessageBox.Show("您目前没有可删除的消息！");
                return;
            }

            if (System.Windows.Forms.MessageBox.Show("确定删除吗？", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                List<NotificationItem> tempList = new List<NotificationItem>();
                int i = 0;

                IEnumerator<NotificationItem> list = (IEnumerator<NotificationItem>)dataGridMessage.ItemsSource.GetEnumerator();
                list.MoveNext();
                List<string> listIds = new List<string>();

                foreach (var item in dataGridMessage.Items)
                {
                    DataGridTemplateColumn templeColumn = dataGridMessage.Columns[0] as DataGridTemplateColumn;
                    FrameworkElement fwElement = dataGridMessage.Columns[0].GetCellContent(item);
                    if (fwElement == null)
                    {
                        continue;
                    }
                    object obj = templeColumn.CellTemplate.FindName("cbMessage", fwElement);
                    if (obj == null)
                    {
                        break;
                    }
                    else
                    {
                        CheckBox cBox = templeColumn.CellTemplate.FindName("cbMessage", fwElement) as CheckBox
   ;
                        if (cBox.IsChecked.Value)
                        {
                            if (list.Current != null)
                            {
                                tempList.Add(list.Current);
                                listIds.Add(list.Current.ID);
                            }

                        }
                        i++;

                        list.MoveNext();
                    }

                }


                foreach (NotificationItem item in tempList)
                {
                    listMessages.Remove(item);
                }

                ThreadPool.QueueUserWorkItem(delegate
                {
                    LungCare.SupportPlatform.WebAPIWorkers.NotificationsWorker.DeleteNotificationRequest(
                        _userInfo.PhoneNumber,
                        listIds,
                        successCallback: delegate (LungCare.SupportPlatform.Models.DelNotificationResponse response)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                ResumeGUI();
                                dataGridMessage.DataContext = null;
                                dataGridMessage.DataContext = listMessages;
                                tbMessageNum.Text = listMessages.Count.ToString();

                                System.Windows.Forms.Application.DoEvents();
                                //MessageBox.Show("删除成功！" + response.Success);
                            }));
                        },
                        failureCallback: delegate (string failureReason)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {

                                MessageBox.Show(failureReason);

                                ResumeGUI();
                            }));
                        },
                        errorCallback: delegate (Exception ex)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {

                                ResumeGUI();
                                LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "删除消息出错。");
                            }));
                        });
                });


            }
        }

        private void btnSelectAllMessage_Click(object sender, RoutedEventArgs e)
        {

            foreach (var item in dataGridMessage.Items)
            {
                DataGridTemplateColumn templeColumn = dataGridMessage.Columns[0] as DataGridTemplateColumn;
                FrameworkElement fwElement = dataGridMessage.Columns[0].GetCellContent(item);
                if (fwElement == null)
                {
                    continue;
                }
                CheckBox cBox = templeColumn.CellTemplate.FindName("cbMessage", fwElement) as CheckBox
 ;
                if (cBox != null)
                {
                    cBox.IsChecked = true;
                }
            }

            dataGridMessage.SelectAll();
        }

        private void dataGridMessage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NotificationItem notification = this.dataGridMessage.SelectedItem as NotificationItem;
            if (notification != null)
            {
                MsgWindow1 msg = new MsgWindow1("", "   " + notification.Content);
                msg.ShowDialog();
            }
        }
        private void CheckSavePassword()
        {
            if (labelCheckPassword1.Content != "" || labelCheckPassword2.Content != "" || labelCheckPassword3.Content != "")
            {
                //验证不通过，按钮变灰
                btnSavePassword.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF343233"));
                btnSavePassword.IsEnabled = false;
            }
            else
            {
                btnSavePassword.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF59275A"));
                btnSavePassword.IsEnabled = true;
            }

        }


        private void CheckSaveForgetPassword()
        {
            if (labelCheckPhoneNumber.Content != "" || labelCheckValidate.Content != "" || labelCheckPassword4.Content != "" || labelCheckPassword5.Content != "")
            {
                btnSaveForgetPassword.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF343233"));
                btnSaveForgetPassword.IsEnabled = false;
            }
            else
            {
                btnSaveForgetPassword.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF59275A"));
                btnSaveForgetPassword.IsEnabled = true;
            }


        }
        private void tbOldPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbOldPassword.Password.Trim() == "")
            {
                labelCheckPassword1.Content = "请填写原密码";
            }
            else if (tbOldPassword.Password.Length < 6 || tbOldPassword.Password.Length > 20)
            {
                labelCheckPassword1.Content = "密码长度为6-20";
            }
            else
            {
                labelCheckPassword1.Content = "";
            }
            CheckSavePassword();
        }

        private void tbNewpassword1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbNewpassword1.Password.Trim() == "")
            {
                labelCheckPassword2.Content = "请填写新密码";
            }
            else if (tbNewpassword1.Password.Length < 6 || tbNewpassword1.Password.Length > 20)
            {
                labelCheckPassword2.Content = "密码长度为6-20";
            }
            else
            {
                labelCheckPassword2.Content = "";
            }

            CheckSavePassword();
        }

        private void tbNewpassword2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbNewpassword2.Password.Length < 6 || tbNewpassword2.Password.Length > 20)
            {
                labelCheckPassword3.Content = "密码长度为6-20";
            }
            else if (tbNewpassword2.Password != tbNewpassword1.Password)
            {
                labelCheckPassword3.Content = "两次输入的密码不相同！！！";
            }
            else
            {
                labelCheckPassword3.Content = "";
            }

            CheckSavePassword();
        }

        private void btnSavePassword_Click(object sender, RoutedEventArgs e)
        {

            if (tbOldPassword.Password.Trim() == "")
            {
                labelCheckPassword1.Content = "请填写原密码";
                return;
            }
            else if (tbOldPassword.Password.Length < 6 || tbOldPassword.Password.Length > 20)
            {
                labelCheckPassword1.Content = "密码长度为6-20";
                return;
            }
            else
            {
                labelCheckPassword1.Content = "";
            }


            if (tbNewpassword1.Password.Trim() == "")
            {
                labelCheckPassword2.Content = "请填写新密码";
                return;
            }
            else if (tbNewpassword1.Password.Length < 6 || tbNewpassword1.Password.Length > 20)
            {
                labelCheckPassword2.Content = "密码长度为6-20";
                return;
            }
            else
            {
                labelCheckPassword2.Content = "";
            }

            if (tbNewpassword2.Password.Length < 6 || tbNewpassword2.Password.Length > 20)
            {
                labelCheckPassword3.Content = "密码长度为6-20";
                return;
            }
            else if (tbNewpassword2.Password != tbNewpassword1.Password)
            {
                labelCheckPassword3.Content = "两次输入的密码不相同！！！";
                return;
            }
            else
            {
                labelCheckPassword3.Content = "";
            }

            lbSavePasswordWait.Visibility = System.Windows.Visibility.Visible;
            PauseGUI();

            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.UpdateUserInfoWorker.UpdateUserInfoRequest(
                    this._userInfo.PhoneNumber,
                    Util.Encrypt(tbOldPassword.Password),
                    Util.Encrypt(tbNewpassword1.Password),
                    this._userInfo.ChineseName,
                    this._userInfo.Institution,
                    this._userInfo.Department,
                    this._userInfo.CertificateImage,
                    successCallback: delegate (LungCare.SupportPlatform.Models.UpdateUserInfoResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            lbSavePasswordWait.Visibility = System.Windows.Visibility.Hidden;
                            MessageBox.Show("密码保存成功！");
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            lbSavePasswordWait.Visibility = System.Windows.Visibility.Hidden;
                            MessageBox.Show(failureReason);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            lbSavePasswordWait.Visibility = System.Windows.Visibility.Hidden;
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "登录出错。");
                        }));
                    });
            });


        }

        private void tbPhoneNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbPhoneNumber.Text.Trim() == "")
            {
                labelCheckPhoneNumber.Content = "请填写手机号码";
            }
            else
            {
                labelCheckPhoneNumber.Content = "";
            }

            CheckSaveForgetPassword();


        }
        private void btnGetValidateNumber_Click(object sender, RoutedEventArgs e)
        {
            CheckSaveForgetPassword();
            if (string.IsNullOrEmpty(tbPhoneNumber.Text))
            {
                labelCheckPhoneNumber.Content = "请填写手机号码";
                return;
            }




            string phoneNumber = tbPhoneNumber.Text.Trim();
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.AskForgetPasswordVerifCodeWorker.SendDownloadCertificateRequest(
                    phoneNumber,
                    successCallback: delegate (LungCare.SupportPlatform.Models.AskForgetPasswordVerifCodeResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("发送成功！");

                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show(failureReason);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "登录出错。");
                        }));
                    });
            });
        }
        private void tbValidateNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbValidateNumber.Text.Trim() == "")
            {
                labelCheckValidate.Content = "请输入验证码";
            }
            else
            {
                labelCheckValidate.Content = "";
            }

            CheckSaveForgetPassword();
        }
        private void tbNewpassword_LostFocus(object sender, RoutedEventArgs e)
        {

            if (tbNewpassword.Password.Trim() == "")
            {
                labelCheckPassword4.Content = "请填写新密码";
            }
            else if (tbNewpassword.Password.Length < 6 || tbNewpassword.Password.Length > 20)
            {
                labelCheckPassword4.Content = "密码长度为6-20";
            }
            else
            {
                labelCheckPassword4.Content = "";
            }

            CheckSaveForgetPassword();
        }

        private void tbNewpassword11_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbNewpassword11.Password.Trim() == "")
            {
                labelCheckPassword5.Content = "请再填一次密码";
            }
            else if (tbNewpassword11.Password.Length < 6 || tbNewpassword11.Password.Length > 20)
            {
                labelCheckPassword5.Content = "密码长度为6-20";
            }
            else if (tbNewpassword11.Password != tbNewpassword.Password)
            {
                labelCheckPassword5.Content = "两次输入的密码不相同";
            }
            else
            {
                labelCheckPassword5.Content = "";
            }

            CheckSaveForgetPassword();
        }

        private void btnSaveForgetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbPhoneNumber.Text))
            {
                MessageBox.Show("请填写手机号！");
                return;
            }


            if (string.IsNullOrEmpty(tbValidateNumber.Text))
            {
                MessageBox.Show("请填写验证码！");
                return;
            }

            if (string.IsNullOrEmpty(tbValidateNumber.Text))
            {
                MessageBox.Show("请填写验证码！");
                return;
            }
            if (string.IsNullOrEmpty(tbNewpassword.Password))
            {
                MessageBox.Show("请填写新密码！");
                return;
            }

            if (string.IsNullOrEmpty(tbNewpassword11.Password))
            {
                MessageBox.Show("请确认新密码！");
                return;
            }

            if (tbNewpassword11.Password != tbNewpassword.Password)
            {
                labelCheckPassword5.Content = "两次输入的密码不相同！！！";
                return;
            }
            else
            {
                labelCheckPassword5.Content = "";
            }

            lbSaveGorgetPassword.Visibility = System.Windows.Visibility.Visible;
            PauseGUI();

            string verifyCode = tbValidateNumber.Text.Trim();
            string username = tbPhoneNumber.Text.Trim();
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.ResetPasswordByVerifyCodeWorker.ResetPasswordByVerifyCodeRequest(
                    verifyCode,
                    username,
                    Util.Encrypt(tbNewpassword.Password),
                    successCallback: delegate (LungCare.SupportPlatform.Models.ResetPasswordByVerifyCodeResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            lbSaveGorgetPassword.Visibility = System.Windows.Visibility.Hidden;
                            MessageBox.Show("密码保存成功！");
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            lbSaveGorgetPassword.Visibility = System.Windows.Visibility.Hidden;
                            MessageBox.Show(failureReason);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            lbSaveGorgetPassword.Visibility = System.Windows.Visibility.Hidden;
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "登录出错。");
                        }));
                    });
            });

        }

        private void btnCancelExit_Click(object sender, RoutedEventArgs e)
        {
            exitQueryForm.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnIsExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void tbNewpassword2_LostMouseCapture(object sender, MouseEventArgs e)
        {
            MessageBox.Show("tbNewpassword2_LostMouseCapture");
        }

        private void btnDownloadPersonImage_Click(object sender, RoutedEventArgs e)
        {
            loadCertifyImage();
        }

        private void btn获取用户资料_Click(object sender, RoutedEventArgs e)
        {
            string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/GetUserInfo";
            LungCare.SupportPlatform.Models.GetUserInfoRequest userIconRequest = new LungCare.SupportPlatform.Models.GetUserInfoRequest();
            userIconRequest.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            userIconRequest.Sender = "PC Client";
            userIconRequest.UserId = LungCare.SupportPlatform.Security.SessionManager.UserName;

            new Thread(new ThreadStart(delegate
            {
                using (var client = new HttpClient())
                {
                    string serializedProduct = JsonConvert.SerializeObject(userIconRequest);
                    Console.WriteLine(URI);
                    Console.WriteLine(serializedProduct);

                    var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                    try
                    {
                        client.PostAsync(URI, content).ContinueWith((postTask) =>
                        {
                            if (postTask.Status == TaskStatus.Canceled)
                            {
                                MessageBox.Show("Download image cancelled.");
                                return;
                            }

                            HttpStatusCode statusCode = postTask.Result.StatusCode;
                            Console.WriteLine(statusCode + Environment.NewLine);

                            postTask.Result.Content.ReadAsStringAsync().ContinueWith((readStringTask) =>
                            {
                                string responseString = readStringTask.Result;
                                Console.WriteLine(responseString);

                                LungCare.SupportPlatform.Models.GetUserInfoResponse response =
                                    JsonConvert.DeserializeObject<LungCare.SupportPlatform.Models.GetUserInfoResponse>(responseString);

                                if (response.Success)
                                {
                                    Dispatcher.Invoke(new Action(delegate
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        sb.AppendLine(response.PhoneNumber);
                                        sb.AppendLine(response.ChineseName);
                                        sb.AppendLine(response.Institution);
                                        sb.AppendLine(response.Department);

                                        tbUserName.Text = sb.ToString();

                                        //MessageBox.Show(response.Success.ToString());
                                        if (response.CertificateImage != null && response.CertificateImage != string.Empty)
                                        {
                                            imagePersonCertify.Source = BitmapSourceFromBase64(response.CertificateImage);
                                        }
                                        if (response.UserIconImage != null && response.UserIconImage != string.Empty)
                                        {
                                            personHeadImage.Source = BitmapSourceFromBase64(response.UserIconImage);
                                        }

                                    }));
                                }
                                else
                                {
                                    Dispatcher.Invoke(new Action(delegate
                                    {
                                        MessageBox.Show(response.ErrorMsg.ToString());

                                    }));
                                    return;
                                }

                            });
                        }).Wait();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            })).Start();
        }

        private void btn获取行医执照审查信息_Click(object sender, RoutedEventArgs e)
        {
            string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/GetCertificateApproveStatus";
            LungCare.SupportPlatform.Models.GetCertificateApproveStatusRequest userIconRequest = new LungCare.SupportPlatform.Models.GetCertificateApproveStatusRequest();
            userIconRequest.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            userIconRequest.Sender = "PC Client";
            userIconRequest.UserId = LungCare.SupportPlatform.Security.SessionManager.UserName;

            new Thread(new ThreadStart(delegate
            {
                using (var client = new HttpClient())
                {
                    string serializedProduct = JsonConvert.SerializeObject(userIconRequest);
                    Console.WriteLine(URI);
                    Console.WriteLine(serializedProduct);

                    var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                    try
                    {
                        client.PostAsync(URI, content).ContinueWith((postTask) =>
                        {
                            if (postTask.Status == TaskStatus.Canceled)
                            {
                                MessageBox.Show("Download image cancelled.");
                                return;
                            }

                            HttpStatusCode statusCode = postTask.Result.StatusCode;
                            Console.WriteLine(statusCode + Environment.NewLine);

                            postTask.Result.Content.ReadAsStringAsync().ContinueWith((readStringTask) =>
                            {
                                string responseString = readStringTask.Result;
                                Console.WriteLine(responseString);

                                GetCertificateApproveStatusResponse response = JsonConvert.DeserializeObject<GetCertificateApproveStatusResponse>(responseString);

                                if (response.Success)
                                {
                                    Dispatcher.Invoke(new Action(delegate
                                    {
                                        MessageBox.Show(response.Success.ToString());
                                    }));
                                }
                                else
                                {
                                    Dispatcher.Invoke(new Action(delegate
                                    {
                                        MessageBox.Show(response.ErrorMsg.ToString());

                                    }));
                                    return;
                                }

                            });
                        }).Wait();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            })).Start();
        }

        private void btn获取数据列表_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                RetrieveDataListWorker.SendRetrieveDataListRequest(
                    successCallback: delegate (RetrieveDataListResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("获取数据失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "获取数据失败。");
                        }));
                    }
               );
            });
        }

        private void btn下载头像_Click(object sender, RoutedEventArgs e)
        {
            loadUserIconImage();
        }

        private void btn创建订单_Click(object sender, RoutedEventArgs e)
        {
            new MESPDownloadUpload().CreateOrderAsync(SessionManager.UserName, null, null);
        }

        private void btn更新订单数据_Click(object sender, RoutedEventArgs e)
        {
            LungCare.SupportPlatform.Models.CTDicomInfo ctDicomInfo = new CTDicomInfo();

            ctDicomInfo.InstitutionName = "InstitutionName";
            ctDicomInfo.PatientAge = "35";
            ctDicomInfo.PatientName = "PatientName";
            ctDicomInfo.PatientSex = "F";
            ctDicomInfo.SeriesInstanceUID = Guid.NewGuid().ToString();
            ctDicomInfo.StudyInstanceUID = Guid.NewGuid().ToString();
            ctDicomInfo.UploadTimestamp = DateTime.Now;

            //UpdateDataWorker.SendUpdateDataRequest(ctDicomInfo, "test.dcm",
            //      successCallback: delegate (LungCare.SupportPlatform.Models.GeneralWebAPIResponse response)
            //      {
            //          Dispatcher.Invoke(new Action(delegate
            //          {
            //          }));
            //      },
            //        failureCallback: delegate (string failureReason)
            //        {
            //            Dispatcher.Invoke(new Action(delegate
            //            {
            //                MessageBox.Show("登录失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            //            }));
            //        },
            //        errorCallback: delegate (Exception ex)
            //        {
            //            Dispatcher.Invoke(new Action(delegate
            //            {
            //                LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "登录出错。");
            //            }));
            //        });
        }

        private void btnCommitFeekback_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelCommit_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void btnADMIN_Click(object sender, RoutedEventArgs e)
        {
            LungCare_Airway_PlanNav.ZLF.Admin.AdminWnd AdminWnd = new LungCare_Airway_PlanNav.ZLF.Admin.AdminWnd();
            AdminWnd.Show();
        }
    }

    public class MessageEntity : INotifyPropertyChanged
    {
        public string SentBy { get; set; }
        public string Content { get; set; }


        private bool _IsSelected = false;
        public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; OnChanged("IsSelected"); } }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        #endregion
    }
}
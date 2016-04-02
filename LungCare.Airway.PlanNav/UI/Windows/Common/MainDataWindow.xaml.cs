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
using System.ComponentModel;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;
using LungCare.SupportPlatform.Models;
using LungCare.SupportPlatform.WebAPIWorkers;
using LungCare.SupportPlatform.Security;
using System.Diagnostics;
using AirwayCT.Entity;
using LungCare.SupportPlatform.SupportPlatformDAO.VTK;
using LungCare.SupportPlatform.SupportPlatformDAO.Airway;
using LungCare.SupportPlatform.UI.UserControls.Common;
using LungCare.SupportPlatform.Entities;
namespace LungCare.SupportPlatform.UI.Windows.Common
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class MainDataWindow : Window
    {
        public static MainDataWindow Instance;

        public void Switch2PersonalCenter()
        {
            tabControl.SelectedIndex = 4;
        }

        public void Switch2UploadWindow()
        {
            tabControl.SelectedIndex = 1;
        }

        public void Switch2UploadWindowThenPopupFileSelectDialog()
        {
            Switch2UploadWindow();
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
            }));
        }

        public void EnableAll()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                this.Cursor = Cursors.Arrow;
            }));
        }

        private DispatcherTimer timer;
        public MainDataWindow()
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
            Login();

        }

        public void Login()
        {
            LungCare.SupportPlatform.Models.LoginRequest LoginRequest = new LungCare.SupportPlatform.Models.LoginRequest();
            LoginRequest.UserName = "15261595318";
            LoginRequest.Password = "1234567";
            LoginRequest.Sender = "PC Client";

            ThreadPool.QueueUserWorkItem(delegate
            {
                LoginWorker.SendLoginRequest(
                    LoginRequest.UserName,
                    LoginRequest.Password,
                    successCallback: delegate(LungCare.SupportPlatform.Models.LoginResponse response)
                    {
                        //Dispatcher.Invoke(new Action(delegate
                        {
                            LungCare.SupportPlatform.Security.TokenManager.Token = response.Token;
                            LungCare.SupportPlatform.Security.SessionManager.UserName = LoginRequest.UserName;

                            loadHandledData();
                        }
                        //));
                    },
                    failureCallback: delegate(string failureReason)
                    {
                        //Dispatcher.Invoke(new Action(delegate
                        {
                            //MessageBox.Show("登录失败。" + failureReason, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk);
                        }
                        //));
                    },
                    errorCallback: delegate(Exception ex)
                    {
                        //Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "登录出错。");
                        }
                        //));
                    });
            });
        }



        private void loadHandledData()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    dataListUserControl.StartLoading();
                }));

                RetrieveDataListWorker.SendRetrieveDataListRequest(
                    successCallback:
                    delegate(RetrieveDataListResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            foreach (var item in response.DataList)
                            {
                                if (item.PatientAge == null)
                                {
                                    item.PatientAge = "未知";
                                }
                                if (item.PatientName == null)
                                {

                                    item.PatientName = "未知";
                                }

                                if (AirwayDAO.isExistLocalDicom(item))
                                {
                                    //如果已下载
                                    item.LocalDicom = "浏览";
                                }
                                else
                                {
                                    item.LocalDicom = "下载";
                                }
                              
                                if (item.Status == "待上传")
                                {
                                    item.LocalDicom = "未上传";
                                }

                              
                                if (item.Status == "处理完成")
                                {
                                    if (AirwayDAO.isExistLocalAirway(item))
                                    {
                                        item.ExportAirwayPackage = "导出";
                                        item.AirwayStatus = "预处理";
                                    }
                                    else
                                    {
                                        item.ExportAirwayPackage = "";
                                        item.AirwayStatus = "下载";
                                    }
                                }

                                item.UserId = _userInfo.PhoneNumber;
                                //if (item.Status == "待上传")
                                //{
                                //    item.Status = "上传";
                                //}
                                //else if (item.Status == "已上传" || item.Status == "处理完成")
                                //{
                                //    item.Status = "预处理";
                                //}

                                //else if (item.Status == "预处理")
                                //{
                                //    item.Status = "测量";
                                //}
                                //else if (item.Status == "测量")
                                //{
                                //    item.Status = "出报告";
                                //}
                                //else if (item.Status == "出报告")
                                //{
                                //    item.Status = "处理完成";
                                //}
                            }
                            dataListUserControl.FinishLoading();
                            dataListUserControl.SetDataSource(response.DataList);
                        }));
                    },
                    failureCallback:
                    delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            dataListUserControl.FinishLoading();
                            MessageBox.Show("获取数据列表失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback:
                    delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            dataListUserControl.FinishLoading();
                            Util.ShowExceptionMessage(ex, "获取数据列表出错。");
                        }));
                    });
            });
        }
        

        
        private GetNotificationsResponse _notification;
        private const int maxBitmapSize = 200 * 1024;
        private const double resizeRatio = 0.4;
        public MainDataWindow(GetUserInfoResponse userInfo ,DataListItem[] dataList )
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
            

            headBorder.MouseDown += headBorder_MouseDown;
            _userInfo = userInfo;
            //dataGridMessage.DataContext = listMessages;

            if (_userInfo != null)
            {
                usernameHome.Text = "Hi , " + _userInfo.ChineseName+ " ,欢迎使用！";
                userNameHead.Text = "Hi , " + _userInfo.ChineseName+ " ，欢迎登陆朗开医疗肺e助手！";
            }

            dataListUserControl.DownloadBatchDicomEvevt += dataListUserControl_DownloadBatchDicomEvevt;
            dataListUserControl.DownloadBatchAirwayEvevt += dataListUserControl_DownloadBatchAirwayEvevt;
            dataListUserControl.UpdateData += dataListUserControl_UpdateData;
            SetDataSource(dataList);
            tabControl.SelectedIndex = 1;
            Instance = this;

            //CCUpDownFile file = new CCUpDownFile(dataList[90] , 云数据类型Enum.Dicom数据压缩包);
            //downloadStackPanel.Children.Add(file);
            //tabControl.SelectedIndex = 2;
        }

        void dataListUserControl_UpdateData(object sender,EventArgs e)
        {
            loadHandledData();
            //throw new NotImplementedException();
        }

        void dataListUserControl_DownloadBatchAirwayEvevt(object sender, DataListItemArgs e)
        {
            CCUpDownFile download = new CCUpDownFile(e.DataListItem, 云数据类型Enum.处理结果);
            downloadStackPanel.Children.Add(download);
            //throw new NotImplementedException();
        }

        void dataListUserControl_DownloadBatchDicomEvevt(object sender, DataListItemArgs e)
        {
            CCUpDownFile download = new CCUpDownFile(e.DataListItem , 云数据类型Enum.Dicom数据压缩包);
            downloadStackPanel.Children.Add(download);
            //throw new NotImplementedException();
        }


        private void SetDataSource(DataListItem[] data)
        {
            dataListUserControl.FinishLoading();
            dataListUserControl.SetDataSource(data);
        }
        //private List<NotificationItem> listMessages;
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
                loading = new Loading();
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
                Dispatcher.Invoke(new Action(delegate
                {
                }));

                RetrieveDataListWorker.SendRetrieveDataListRequest(
                    successCallback:
                    delegate (RetrieveDataListResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            foreach (var item in response.DataList)
                            {
                                if (item.PatientAge == null)
                                {
                                    item.PatientAge = "未知";
                                }
                                if (item.PatientName == null)
                                {
                                    item.PatientName = "未知";
                                }

                                if (item.Status == "待上传")
                                {
                                    item.Status = "上传";
                                }
                                if (item.Status == "已上传" ||item.Status=="处理完成")
                                {
                                    item.Status = "预处理";
                                }

                                //if (item.Status == "预处理")
                                //{
                                //    item.Status = "测量";
                                //}
                                //if (item.Status == "测量")
                                //{
                                //    item.Status = "出报告";
                                //}
                            }

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
        
        void headBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {
                
            }
          
            //throw new NotImplementedException();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Instance = this;
        }


        private void timer_Tick(object sender, EventArgs e)
        {
            LoadNotification();
            GetCertificateApproveStatus();
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

        private int _oldNotificationCount = 0;
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

                            if (response.NotificationList != null)
                            {
                                response.NotificationList.Sort(delegate(NotificationItem t1, NotificationItem t2)
                                {
                                    return -t1.TimeStamp.CompareTo(t2.TimeStamp);
                                });

                                if (response.NotificationList.Count <= 0)
                                {
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }
                            this._notification = response;
                            
                            if (this._notification != null)
                            {
                                if (this._notification.NotificationList != null)
                                {
                                    int read = 0;
                                    for (int i = 0; i < this._notification.NotificationList.Count; i++)
                                    {
                                        if (this._notification.NotificationList[i].Read)
                                        {
                                            read++;
                                        }
                                    }
                                    //if (_oldNotificationCount != this._notification.NotificationList.Count)
                                    {
                                        _oldNotificationCount = this._notification.NotificationList.Count;
                                    }
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
                            //LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "获取消息列表出错。");
                        }));
                    });
            });
        }
        private void btnShowMessage_Click(object sender, RoutedEventArgs e)
        {
            LoadNotification();
            tabControl.SelectedIndex = 4;
        }


        private ImportDicomWindow3 import;
        private DateTime lastClickTimeStamp = DateTime.MinValue;
        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (e.Source is TabControl)
            {
                int tab = this.tabControl.SelectedIndex;
                if (tab == 1)
                {
                    //Login();
                    loadHandledData();
                }
                else if (tab == 3)
                {
                    tabControl.SelectedIndex = 1;
                    import = new ImportDicomWindow3();
                    import.Loaded += delegate
                    {
                        this.Hide();
                    };
                    if (import.ShowDialog() == true)
                    {
                        this.Show();
                    }

                    loadHandledData();
                    return;
                }
                else if(tab == 4)
                {
                    tabControl.SelectedIndex = 1;
                    ImportResultData();

                    return;
                }

            }
        }


        private bool validateExtention(string extent)
        {
            return extent == ".rar" || extent == ".zip" || extent == ".7z" || extent == ".rar5";
        }
        private void ImportResultData()
        {
            {
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.Filter = "压缩包文件|*.zip;*rar;*.7z;*.rar5";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
                    var psi = new ProcessStartInfo(System.Windows.Forms.Application.StartupPath + @"\HaoZipC.exe", args);
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
                    //ReloadPatientDataGridView();
                     System.Windows.Forms.MessageBox.Show(string.Format(@"导入成功。病人姓名：{0}", patient.Name), @"导入成功", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
            }
            return;
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

        }

        private void btnListMessage_Click(object sender, RoutedEventArgs e)
        {

            LoadNotification();

        }

        private void btnModifyPassword_Click(object sender, RoutedEventArgs e)
        {
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

                PauseGUI();
                Uri uri = new Uri(openFileDialog.FileName);
                BitmapImage bitmap0 = new BitmapImage(uri);
                BitmapImage bitmap1 = new BitmapImage();
                double ratio = maxBitmapSize / (double)file.Length;
                //if (file.Length > 200 * 1024)
                if(file.Length > maxBitmapSize)
                {
                    bitmap1.BeginInit();
                    bitmap1.UriSource = uri;
                    bitmap1.DecodePixelWidth = Convert.ToInt16(bitmap0.Width * ratio);
                    bitmap1.DecodePixelHeight = Convert.ToInt16(bitmap0.Height * ratio);
                    bitmap1.EndInit();
                }
                else
                {
                    bitmap1 = new BitmapImage(uri);
                }
                 
               

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
                                MessageBox.Show(failureReason);
                            }));
                        },
                        errorCallback: delegate (Exception ex)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                ResumeGUI();
                                LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "上传资格证书出错");
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
                            usernameHome.Text = response.ChineseName;
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
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
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
                            //LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "获取证书审查状态出错。");
                        }));
                    });
            });
        }
        private void btnForgetPassword_Click(object sender, RoutedEventArgs e)
        {
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

        public void PauseGUI()
        {
            ((UIElement)Content).IsEnabled = false;
            Cursor = Cursors.Wait;
        }

        public void ResumeGUI()
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
                PauseGUI();
                Uri uri = new Uri(openFileDialog.FileName);
                BitmapImage bitmap0 = new BitmapImage(uri);
                BitmapImage bitmap1 = new BitmapImage();
                //if (file.Length > 200 * 1024)
                double ratio = maxBitmapSize / (double)file.Length;
                if(file.Length>maxBitmapSize)
                {
                    bitmap1.BeginInit();
                    bitmap1.UriSource = uri;
                    bitmap1.DecodePixelWidth = Convert.ToInt16( bitmap0.Width*ratio);
                    bitmap1.DecodePixelHeight = Convert.ToInt16( bitmap0.Height*ratio);
                    bitmap1.EndInit();
                }
                else
                {
                    bitmap1 = new BitmapImage(uri);
                }

             
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

        }

        private void btnCancelCommit_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("确定取消提交吗？", "", System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
            }
        }

        private void btnCheckVersion_Click(object sender, RoutedEventArgs e)
        {
            var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(30);
            Cursor = System.Windows.Input.Cursors.Wait;
            try
            {
                http.GetStringAsync(new Uri("http://112.126.78.187:8080/DownloadInstall/file/version.txt")).ContinueWith((task) =>
                {
                    Version latestVersion = new Version(task.Result);

                    //get my own version to compare against latest.
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                    Version myVersion = new Version("1.0.0.0");
                    Dispatcher.Invoke(new Action(delegate
                    {
                        Cursor = System.Windows.Input.Cursors.Arrow;
                        if (latestVersion > myVersion)
                        {
                            if (MessageBox.Show("检测到软件有新版本，请问是否下载升级？", "升级提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                System.Diagnostics.Process.Start("http://112.126.78.187:8080/DownloadInstall/file/LungcareE.exe");
                            }
                        }
                        else
                        {
                          
                            MessageBox.Show("您的版本已经是最新版本");
                        }

                    }));
                });

            }
            catch (Exception)
            {

                Cursor = System.Windows.Input.Cursors.Arrow;
                MessageBox.Show("获取最新版本失败，请检查网络连接");
            }
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent is valid.  
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child 
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree 
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.  
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search 
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name 
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found. 
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }
        private void btnDeleteMessage_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnSelectAllMessage_Click(object sender, RoutedEventArgs e)
        {
 //           foreach (var item in this._notification.NotificationList)
 //           {
 //               item.IsSelected = true;
 //           }

 //           System.Windows.Forms.Application.DoEvents();
 //           _checkedMessageNum = this._notification.NotificationList.Count;
 //           return;
 //           foreach (var item in dataGridMessage.Items)
 //           {
 //               DataGridTemplateColumn templeColumn = dataGridMessage.Columns[0] as DataGridTemplateColumn;
 //               FrameworkElement fwElement = dataGridMessage.Columns[0].GetCellContent(item);
 //               if (fwElement == null)
 //               {
 //                   continue;
 //               }
 //               CheckBox cBox = templeColumn.CellTemplate.FindName("cbMessage", fwElement) as CheckBox
 //;
 //               if (cBox != null)
 //               {
 //                   cBox.IsChecked = true;
 //               }
           // dataGridMessage.SelectAll();
        }

        private void dataGridMessage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        private void CheckSavePassword()
        {

        }


        private void CheckSaveForgetPassword()
        {

        }
        private void tbOldPassword_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void tbNewpassword1_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void tbNewpassword2_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void btnSavePassword_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbPhoneNumber_LostFocus(object sender, RoutedEventArgs e)
        {

        }
        private void btnGetValidateNumber_Click(object sender, RoutedEventArgs e)
        {
        }
        private void tbValidateNumber_LostFocus(object sender, RoutedEventArgs e)
        {
        }
        private void tbNewpassword_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void tbNewpassword11_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void btnSaveForgetPassword_Click(object sender, RoutedEventArgs e)
        {

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

        private int _checkedMessageNum = 0;
        private void cbMessage_Checked(object sender, RoutedEventArgs e)
        {
            ++_checkedMessageNum;
        }

        private void cbMessage_Unchecked(object sender, RoutedEventArgs e)
        {
            --_checkedMessageNum;
        }

        private void dataGridMessage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void TabItem_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

      
    }

    //public class MessageEntity : INotifyPropertyChanged
    //{
    //    public string SentBy { get; set; }
    //    public string Content { get; set; }


    //    private bool _IsSelected = false;
    //    public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; OnChanged("IsSelected"); } }

    //    #region INotifyPropertyChanged Members

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void OnChanged(string prop)
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    //    }

    //    #endregion
    //}
}
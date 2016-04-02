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
using LungCare.SupportPlatform.UI.Windows.Common;

namespace LungCare_Airway_PlanNav
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class UserWindow : Window
    {
      
        public UserWindow()
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
           

        
        }

        private GetNotificationsResponse _notification;
        private const int maxBitmapSize = 200 * 1024;
        private const double resizeRatio = 0.4;
        public UserWindow(GetUserInfoResponse userInfo)
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
           
            this._userInfo = userInfo;
            LoadUserInfo();
            loadCertificateImage();
            loadAllData();
            GetCertificateApproveStatus();

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
            RetrieveDataListRequest updateDicomDataRequest = new RetrieveDataListRequest();
            updateDicomDataRequest.Sender = "PC Client";

            updateDicomDataRequest.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            updateDicomDataRequest.UserId = _userInfo.PhoneNumber;

            ThreadPool.QueueUserWorkItem(delegate
            {
                GetOrders(updateDicomDataRequest);
            });
        }



        private void GetOrders(RetrieveDataListRequest updateDicomDataRequest)
        {
            RetrieveDataListWorker.SendRetrieveDataListRequest(
                updateDicomDataRequest,
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
                        }

                        UploadListUserControl.FinishLoading();
                        DownloadListUserControl.FinishLoading();

                        UploadListUserControl.SetDataSource(response.DataList);
                        DownloadListUserControl.SetDataSource(response.DataList);
                    }));
                    Thread.Sleep(1000 * 60);
                    GetOrders(updateDicomDataRequest);
                },
                failureCallback:
                delegate(string failureReason)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        MessageBox.Show("获取订单列表失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }));
                },
                errorCallback:
                delegate(Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        Util.ShowExceptionMessage(ex, "获取订单列表出错。");
                    }));
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
        }

        private void LinkLabelPlay_Click(object sender, RoutedEventArgs e)
        {
        }


        private void StartPlay(string filename)
        {
        }


        private void StopPlay()
        {
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

    


        
        /// <summary>
        /// 上传医生资格证照片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUploadPersonImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.FileName = "资格证书";
            openFileDialog.DefaultExt = ".jpg";
            openFileDialog.Filter = "jpg|*.jpg|jpeg|*.jpeg";
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
                BitmapImage bitmap0 = new BitmapImage(uri);
                BitmapImage bitmap1 = new BitmapImage();
                double ratio = maxBitmapSize / (double)file.Length;
                //if (file.Length > 200 * 1024)
                if (file.Length > maxBitmapSize)
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

                ThreadPool.QueueUserWorkItem(delegate
                {
                    UploadCertificateByAdminWorker.SendUploadCertificateRequest(
                       base64String,
                        _userInfo.PhoneNumber,
                        successCallback: delegate(UploadCertificateByAdminResponse response)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                _bitmapCertificate = bitmap1;
                                imagePersonCertify.Source = bitmap1;
                                ResumeGUI();
                                //lbCertificateApproveStatus.Content = "等待工作人员审核";
                                GetCertificateApproveStatus();
                                _userInfo.CertificateImage = base64String;
                                MessageBox.Show("资格证书上传成功！");
                            }));
                        },
                        failureCallback: delegate(string failureReason)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                ResumeGUI();
                                lbCertificateApproveStatus.Content = _certificateApproveStatus.Result;
                                MessageBox.Show(failureReason);
                            }));
                        },
                        errorCallback: delegate(Exception ex)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                ResumeGUI();
                                LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "上传资格证书出错");
                                lbCertificateApproveStatus.Content = _certificateApproveStatus.Result;
                            }));
                        });
                });
            }
        }


        private GetUserInfoResponse _userInfo;
        private void LoadUserInfo()
        {
            if (_userInfo != null)
            {
                tbUserName.Text = _userInfo.ChineseName + "  医生";
                tbHospitalName.Text = _userInfo.Institution + "  " + _userInfo.Department;
                userNameHead.Text = "用户 ：" + _userInfo.ChineseName;
            }
                      
        }

        private BitmapSource _bitmapCertificate;
        private void loadCertificateImage()
        {
            if (_userInfo != null && _userInfo.CertificateImage != null)
            {
                _bitmapCertificate = BitmapSourceFromBase64(_userInfo.CertificateImage);
                imagePersonCertify.Source = _bitmapCertificate;
            }
        }
        public GetCertificateApproveStatusResponse _certificateApproveStatus;
        private void GetCertificateApproveStatus()
        {
            
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.GetCertificateApproveStatusWorker.SendGetCertificateApproveStatusRequeset(
                    _userInfo.PhoneNumber,
                    successCallback: delegate(LungCare.SupportPlatform.Models.GetCertificateApproveStatusResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                             string status = "未知";
                            _certificateApproveStatus = response;
                            switch (_certificateApproveStatus.Result)
                            {
                                case "YES":
                                    status = "通过审核";
                                    //btnUploadPersonImage.IsEnabled = false;
                                    //btnUploadPersonImage.Visibility = System.Windows.Visibility.Hidden;
                                    break;
                                case "NO":
                                    status = "没有通过审查:" + _certificateApproveStatus.RejectReason;
                                    //btnUploadPersonImage.IsEnabled = true;
                                    //btnUploadPersonImage.Visibility = System.Windows.Visibility.Visible;
                                    break;
                                case "NotUploadedYet":
                                    status = "未上传行医执照";
                                    //btnUploadPersonImage.IsEnabled = true;
                                    //btnUploadPersonImage.Visibility = System.Windows.Visibility.Visible;
                                    break;
                                case "WaitingApprove":
                                    status = "等待工作人员审核";
                                    //btnUploadPersonImage.IsEnabled = false;
                                    //btnUploadPersonImage.Visibility = System.Windows.Visibility.Visible;
                                    break;
                                default:
                                    break;
                            }
                            lbCertificateApproveStatus.Content = status;
                            
                        }));
                    },
                    failureCallback: delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                             lbCertificateApproveStatus.Content = _certificateApproveStatus.Result;
                            if (failureReason == null)
                            {
                                 MessageBox.Show("该用户还未上传资格证书照片！");
                            }
                            else
                            {
                                MessageBox.Show(failureReason);
                            }

                            ResumeGUI();
                        }));
                    },
                    errorCallback: delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "获取证书审查状态出错。");
                        }));
                    });
            });
    
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
                try { 
                    //var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    //BitmapSource result = decoder.Frames[0];
                    //result.Freeze();
                    //return result;
                    BitmapDecoder decoder = BitmapDecoder.Create(stream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad); // enables closing the stream immediately
                    return decoder.Frames[0];
                }catch(Exception ex){
                    BitmapImage bitImage = new BitmapImage();
                    bitImage.BeginInit();
                    bitImage.StreamSource = stream;
                    bitImage.EndInit();
                    //JpegBitmapDecoder jpeDecoder=new JpegBitmapDecoder(imageStreamSource,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad);
                    //ImageSource imageSource=jpeDecoder.Frames[0];
                    BitmapSource result = bitImage;
                    return result; 
                }
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
                BitmapImage bitmap0 = new BitmapImage(uri);
                BitmapImage bitmap1 = new BitmapImage();
                //if (file.Length > 200 * 1024)
                double ratio = maxBitmapSize / (double)file.Length;
                if (file.Length > maxBitmapSize)
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







                ThreadPool.QueueUserWorkItem(delegate
                {
                    LungCare.SupportPlatform.WebAPIWorkers.UploadUserIconWorker.SendUserIconRequest(
                        base64String,
                        successCallback: delegate(LungCare.SupportPlatform.Models.UploadUserIconResponse response)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                personHeadImage.Source = bitmap1;
                                personHeadImageHead.Source = bitmap1;

                                ResumeGUI();
                                btnChangeHeaderImage.Content = "更换照片";
                                MessageBox.Show("上传头像成功!");
                            }));
                        },
                        failureCallback: delegate(string failureReason)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {

                                MessageBox.Show(failureReason);

                                ResumeGUI();
                                btnChangeHeaderImage.Content = "更换照片";
                            }));
                        },
                        errorCallback: delegate(Exception ex)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {

                                ResumeGUI();
                                btnChangeHeaderImage.Content = "更换照片";
                                LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "上传头像出错。");
                            }));
                        });
                });

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
      
        private void btnCancelExit_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnIsExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

    
        private void btnDownloadPersonImage_Click(object sender, RoutedEventArgs e)
        {
        }

     
      

     

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

     

   
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
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
using LungCare.SupportPlatform.UI;
using LungCare.SupportPlatform.UI.Windows.Common;

namespace LungCare_Airway_PlanNav
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class UserCertificateCheckWindow : Window
    {
        public static MainWindow Instance;

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

        public UserCertificateCheckWindow()
        {
            this.InitializeComponent();
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;// 获得窗体的 样式

            int oldstyle = MedSys.PresentationCore.AdjustWindow.NativeMethods.GetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE, oldstyle & ~MedSys.PresentationCore.AdjustWindow.NativeMethods.WS_CAPTION);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetLayeredWindowAttributes(hwnd, 1 | 2 << 8 | 3 << 16, 0, MedSys.PresentationCore.AdjustWindow.NativeMethods.LWA_ALPHA);

            this.RefreshRoundWindowRect();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            headBorder.MouseDown+=headBorder_MouseDown;

        }

        private GetUserInfoResponse _UserInfo;
        public UserCertificateCheckWindow(GetUserInfoResponse userInfo)
        {
            this.InitializeComponent();
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;// 获得窗体的 样式

            int oldstyle = MedSys.PresentationCore.AdjustWindow.NativeMethods.GetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE, oldstyle & ~MedSys.PresentationCore.AdjustWindow.NativeMethods.WS_CAPTION);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetLayeredWindowAttributes(hwnd, 1 | 2 << 8 | 3 << 16, 0, MedSys.PresentationCore.AdjustWindow.NativeMethods.LWA_ALPHA);

            this.RefreshRoundWindowRect();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.SizeChanged += MainWindow_SizeChanged;
            headBorder.MouseDown += headBorder_MouseDown;
            _UserInfo = userInfo;
            List<GetUserInfoResponse> list = new List<GetUserInfoResponse>();
            list.Add(_UserInfo);

            listViewUser.DataContext =
              new System.ComponentModel.BindingList<LungCare.SupportPlatform.Models.GetUserInfoResponse>(list);
            //listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
            listViewUser.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());

            loadCertifyImage();
            GetCertificateApproveStatus();


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
        private DownloadCertificateRespnse _CertificateRespnse;
        private BitmapSource _BitmapSourceCertificate;

        private void loadCertifyImage()
        {
            if (_UserInfo != null && _UserInfo.CertificateImage!=null)
            {
                this._BitmapSourceCertificate = BitmapSourceFromBase64(_UserInfo.CertificateImage);
                imageCertificate.Source = _BitmapSourceCertificate;
            }
            //ThreadPool.QueueUserWorkItem(delegate
            //{
            //    LungCare.SupportPlatform.WebAPIWorkers.DownloadCertificateWorker.SendDownloadCertificateRequest(
            //        successCallback: delegate(LungCare.SupportPlatform.Models.DownloadCertificateRespnse response)
            //        {
            //            Dispatcher.Invoke(new Action(delegate
            //            {
            //                this._CertificateRespnse = response;
            //                this._BitmapSourceCertificate = BitmapSourceFromBase64(response.CertificateImage);
            //                imageCertificate.Source = _BitmapSourceCertificate;
            //            }));
            //        },
            //        failureCallback: delegate(string failureReason)
            //        {
            //            Dispatcher.Invoke(new Action(delegate
            //            {
            //                ResumeGUI();
            //                MessageBox.Show(failureReason);
            //            }));
            //        },
            //        errorCallback: delegate(Exception ex)
            //        {
            //            Dispatcher.Invoke(new Action(delegate
            //            {
            //                ResumeGUI();
            //                LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "下载资格证出错。");
            //            }));
            //        });
            //});
        }

        private GetCertificateApproveStatusResponse _CertificateApproveStatus;
        private void GetCertificateApproveStatus()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.GetCertificateApproveStatusWorker.SendGetCertificateApproveStatusRequeset(
                    _UserInfo.PhoneNumber,
                    successCallback: delegate(LungCare.SupportPlatform.Models.GetCertificateApproveStatusResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            _CertificateApproveStatus = response;
                             string status = "未知";
                             if (_CertificateApproveStatus != null)
                             {

                                 switch (_CertificateApproveStatus.Result)
                                 {
                                     case "YES":
                                         status = "通过审核";
                                         rbCheckSuccess.IsChecked = true;
                                         break;
                                     case "NO":
                                         status = "没有通过审查:" + _CertificateApproveStatus.RejectReason;
                                         rbCheckFailed.IsChecked = true;
                                         break;
                                     case "NotUploadedYet":
                                         status = "未上传行医执照";
                                         rbCheckFailed.IsChecked = true;
                                         break;
                                     case "WaitingApprove":
                                         status = "等待工作人员审核";
                                         break;
                                     default:
                                         break;
                                 }
                                 lbCertificateStatus.Content = status;
                             }
                        }));
                    },
                    failureCallback: delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {

                            if (failureReason == null)
                            {
                                //MessageBox.Show("该用户还未上传资格证书照片！");
                            }
                            else
                            {
                                MessageBox.Show(failureReason);
                            }

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
                    MessageBox.Show(ex.ToString());
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
        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.RefreshRoundWindowRect();
        }

    
    
     
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
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

        private void RefreshRoundWindowRect()
        {
            // 获取窗体句柄
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

            // 创建圆角窗体
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowRgn(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.CreateRoundRectRgn(0, 0, Convert.ToInt32(this.ActualWidth) + 1, Convert.ToInt32(this.ActualHeight) + 1, 5, 5), true);
        }

        private void rbCheckSuccess_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void rbCheckFailed_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnComfirm_Click(object sender, RoutedEventArgs e)
        {
            //if (_BitmapSourceCertificate != null)
            {
                if (rbCheckSuccess.IsChecked.Value)
                {
                    UpdateCertificateApproveStatus(true, null);
                }
                else
                {
                    UpdateCertificateApproveStatus(false, tbCheckReason.Text);
                }

                if (UpdateHandler != null)
                {
                    UpdateHandler(sender, e);
                }
                
            }
        }

        public event EventHandler<RoutedEventArgs> UpdateHandler;

        private void UpdateCertificateApproveStatus(bool result, string rejectReason)
        {
            UpdateCertificateApproveStatusRequest request = new UpdateCertificateApproveStatusRequest();
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.UserId = _UserInfo.PhoneNumber;
            request.Sender = "PC Client";
            request.Result = result;
            request.RejectReason = rejectReason;
            PauseGUI();
            ThreadPool.QueueUserWorkItem(delegate
            {
                UpdateCertificateApproveStatusWorker.SendUpdateCertificateApproveStatusRequeset(
                    request.UserId,
                    request.Result,
                    request.RejectReason,
                    successCallback: delegate(UpdateCertificateApproveStatusResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            if (result)
                            {
                                NotificationsWorker.SendAddNotification(_UserInfo.PhoneNumber, "恭喜您，您的医生资格证通过审核。");
                                lbCertificateStatus.Content = "通过审核";
                            }
                            else
                            {
                                NotificationsWorker.SendAddNotification(_UserInfo.PhoneNumber, "很遗憾，您的医生资格证未通过审核。");
                                lbCertificateStatus.Content = "审核不通过："+tbCheckReason.Text;
                            }
                          
                            MessageBox.Show("确认结束");
                            ResumeGUI();
                        }));
                    },
                    failureCallback: delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("审核医师资格证失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            ResumeGUI();
                        }));
                    },
                    errorCallback: delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "审核医师资格证出错。");
                            ResumeGUI();
                        }));
                    });
            });
        }

        private const int maxBitmapSize = 200 * 1024;
        private const double resizeRatio = 0.4;
        private void btnUploadUserFile_Click(object sender, RoutedEventArgs e)
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

                //lbCertificateApproveStatus.Content = "正在上传...";
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

               

                string certificateImagebase64String = ToBase64(bitmap1);

                string userid = _UserInfo.PhoneNumber;
                

                ThreadPool.QueueUserWorkItem(delegate
                {
                    UploadCertificateByAdminWorker.SendUploadCertificateRequest(
                       certificateImagebase64String,
                        userid,
                        successCallback: delegate(UploadCertificateByAdminResponse response)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                _BitmapSourceCertificate = bitmap1;
                                imageCertificate.Source = _BitmapSourceCertificate;
                                _UserInfo.CertificateImage = certificateImagebase64String;

                                if (UpdateHandler != null)
                                {
                                    UpdateHandler(sender, e);
                                }
                                MessageBox.Show("上传成功！");
                                ResumeGUI();
                            }));
                        },
                        failureCallback: delegate(string failureReason)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                MessageBox.Show("医师资格证上传失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                                ResumeGUI();
                            }));
                        },
                        errorCallback: delegate(Exception ex)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                Util.ShowExceptionMessage(ex, "医师资格证上传出错。");
                                ResumeGUI();
                            }));
                        });
                });
            }
        }

        private void btnDownloadUserFile_Click(object sender, RoutedEventArgs e)
        {
            if (_BitmapSourceCertificate != null)
            {
                var openFileDialog = new Microsoft.Win32.SaveFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    
                    WriteJpeg(openFileDialog.FileName , 100 , _BitmapSourceCertificate);
                    MessageBox.Show("下载成功！");
                }
            }
        }


        private void WriteJpeg(string fileName, int quality, BitmapSource bmp)
        {

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            BitmapFrame outputFrame = BitmapFrame.Create(bmp);
            encoder.Frames.Add(outputFrame);
            encoder.QualityLevel = quality;

            using (FileStream file = File.OpenWrite(fileName))
            {
                encoder.Save(file);
            }
        }

        private void btnCancelFile_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }


    }

    
}
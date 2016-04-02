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
using LungCare.SupportPlatform.UI.Windows.Admin;
using LungCare.SupportPlatform.UI.Windows.Common;

namespace LungCare_Airway_PlanNav
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class NotificationWindow : Window
    {
        public static MainWindow Instance;


        public NotificationWindow()
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
            //this.Width = SystemParameters.WorkArea.Width;
            //this.Height = SystemParameters.WorkArea.Height;
            borderNewOrder.MouseLeftButtonDown += borderNewOrder_MouseLeftButtonDown;
            borderNewCetifi.MouseLeftButtonDown+=borderNewCetifi_MouseLeftButtonDown;
            lbMessageConfigure.MouseLeftButtonDown += borderNewUser_MouseLeftButtonDown;
            borderUserList.MouseLeftButtonDown+=borderUserList_MouseLeftButtonDown;

            lbNewOrder1.MouseLeftButtonDown += borderNewOrder_MouseLeftButtonDown;
            lbNewOrder.MouseLeftButtonDown += borderNewOrder_MouseLeftButtonDown;

            lbNewCertifi.MouseLeftButtonDown += borderNewCetifi_MouseLeftButtonDown;
            lbNewCertifi1.MouseLeftButtonDown += borderNewCetifi_MouseLeftButtonDown;

            lbMessageConfigure.MouseLeftButtonDown += borderNewUser_MouseLeftButtonDown;
            lbMessageConfigure1.MouseLeftButtonDown += borderNewUser_MouseLeftButtonDown;

            lbUserList.MouseLeftButtonDown += borderUserList_MouseLeftButtonDown;
            lbUserList1.MouseLeftButtonDown += borderUserList_MouseLeftButtonDown;
        }


        private List<GetUserInfoResponse> _waitingApproveUserInfoList;
        private LungCare.SupportPlatform.Models.DataListItem[] _datalist;
        private GetUserInfoResponse[] _UserNameList;
        private DispatcherTimer checkNewOrderTimer;
        public NotificationWindow(List<GetUserInfoResponse> waitingApproveUserNameList,
           LungCare.SupportPlatform.Models.DataListItem[] datalist,
           GetUserInfoResponse[] userNameList)
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
            //this.Width = SystemParameters.WorkArea.Width;
            //this.Height = SystemParameters.WorkArea.Height;
            borderNewOrder.MouseLeftButtonDown += borderNewOrder_MouseLeftButtonDown;
            borderNewOrder.MouseEnter += borderNewOrder_MouseEnter;
            borderNewOrder.MouseLeave += borderNewOrder_MouseLeave;

            borderNewCetifi.MouseLeftButtonDown += borderNewCetifi_MouseLeftButtonDown;
            borderNewCetifi.MouseEnter += borderNewCetifi_MouseEnter;
            borderNewCetifi.MouseLeave += borderNewCetifi_MouseLeave;

            borderMessageConfiguration.MouseLeftButtonDown += borderNewUser_MouseLeftButtonDown;
            borderMessageConfiguration.MouseEnter += borderNewUser_MouseEnter;
            borderMessageConfiguration.MouseLeave += borderNewUser_MouseLeave;


            borderUserList.MouseLeftButtonDown += borderUserList_MouseLeftButtonDown;
            borderUserList.MouseEnter += borderUserList_MouseEnter;
            borderUserList.MouseLeave += borderUserList_MouseLeave;


            lbNewOrder.MouseLeftButtonDown += borderNewOrder_MouseLeftButtonDown;
            lbNewOrder.MouseEnter += borderNewOrder_MouseEnter;
            lbNewOrder.MouseLeave += borderNewOrder_MouseLeave;
            lbNewOrder1.MouseLeftButtonDown += borderNewOrder_MouseLeftButtonDown;
            lbNewOrder1.MouseEnter += borderNewOrder_MouseEnter;
            lbNewOrder1.MouseLeave += borderNewOrder_MouseLeave;

            lbNewCertifi.MouseLeftButtonDown += borderNewCetifi_MouseLeftButtonDown;
            lbNewCertifi.MouseEnter += borderNewCetifi_MouseEnter;
            lbNewCertifi.MouseLeave += borderNewCetifi_MouseLeave;
            lbNewCertifi1.MouseLeftButtonDown += borderNewCetifi_MouseLeftButtonDown;
            lbNewCertifi1.MouseEnter += borderNewCetifi_MouseEnter;
            lbNewCertifi1.MouseLeave += borderNewCetifi_MouseLeave;

            lbMessageConfigure.MouseLeftButtonDown += borderNewUser_MouseLeftButtonDown;
            lbMessageConfigure.MouseEnter += borderNewUser_MouseEnter;
            lbMessageConfigure.MouseLeave += borderNewUser_MouseLeave;
            lbMessageConfigure1.MouseLeftButtonDown += borderNewUser_MouseLeftButtonDown;
            lbMessageConfigure1.MouseEnter += borderNewUser_MouseEnter;
            lbMessageConfigure1.MouseLeave += borderNewUser_MouseLeave;

            lbUserList.MouseLeftButtonDown += borderUserList_MouseLeftButtonDown;
            lbUserList.MouseEnter += borderUserList_MouseEnter;
            lbUserList.MouseLeave += borderUserList_MouseLeave;
            lbUserList1.MouseLeftButtonDown += borderUserList_MouseLeftButtonDown;
            lbUserList1.MouseEnter += borderUserList_MouseEnter;
            lbUserList1.MouseLeave += borderUserList_MouseLeave;

            _UserNameList = userNameList;
            lbUserList1.Content = userNameList.Length;

            _waitingApproveUserInfoList = waitingApproveUserNameList;
            lbNewCertifi1.Content = _waitingApproveUserInfoList.Count;

            _datalist = datalist;
            lbNewOrder1.Content = datalist.Length;


            checkNewOrderTimer = new DispatcherTimer();
            checkNewOrderTimer.Interval = TimeSpan.FromSeconds(10);
            checkNewOrderTimer.Tick += checkNewOrderTimer_Tick;
            //checkNewOrderTimer.Start();
            _lastCheckOrderTime = new DateTime();
        }

        private DateTime _lastCheckOrderTime;
        void checkNewOrderTimer_Tick(object sender, EventArgs e)
        {
            checkNewOrder();
            //throw new NotImplementedException();
        }

        private void checkNewOrder()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                Dispatcher.Invoke(new Action(delegate
                {
                }));

                RetrieveAllOrdersInSystemWorker.SendRetrieveDataListRequest(
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

                            response.DataList = response.DataList;

                            response.DataList = response.DataList.OrderBy(item => string.IsNullOrEmpty(item.UploadTimestamp) ? DateTime.MinValue : DateTime.ParseExact(item.UploadTimestamp, "yyyyMMdd HHmmss", null)).Reverse().ToArray();
                            response.DataList = response.DataList.Where(item => item.Status == "已上传").ToArray();
                            if (response.DataList.Length > _datalist.Length)
                            {
                                GetUserInfoResponse userInfo = _UserNameList.FirstOrDefault(t => t.PhoneNumber == response.DataList[0].UserId);
                                if(userInfo!=null)
                                {
                                    Console.WriteLine("you have new order.");
                                    _datalist = response.DataList;
                                    lbNewOrder1.Content = _datalist.Length;
                                    SendNotificateEmail(userInfo.Institution + " " + userInfo.ChineseName, response.DataList[0].DataID);
                                    SendPhoneMessage(userInfo.Institution+" "+ userInfo.ChineseName, response.DataList[0].DataID);
                                }
                            }                           
                        }));
                    },
                    failureCallback:
                    delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("获取数据列表失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback:
                    delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "获取数据列表出错。");
                        }));
                    });
            });
        }

        private void SendPhoneMessage(string sendUserName , string orderID)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.SendPhoneMessageWorker.SendPhoneMessageRequeset(
                    "您收到用户"+sendUserName+"上传的新订单，订单号："+orderID+"【朗开医疗】",
                    "15261595318",
                    successCallback: delegate(LungCare.SupportPlatform.Models.SendPhoneMessageResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {

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
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "获取用户信息出错。");
                        }));
                    });
            });
        }
        private void SendNotificateEmail(string sendUserName , string orderID)
        {
            //smtp.163.com
            //string senderServerIp = "202.108.5.83";
            //smtp.gmail.com
            //string senderServerIp = "74.125.127.109";
            string senderServerIp = "smtp.qq.com";
            //smtp.qq.com
            //string senderServerIp = "58.251.149.147";
            //string senderServerIp = "smtp.sina.com";
            string toMailAddress = "419655660@qq.com";
            string subjectInfo ="新订单通知 【朗开医疗】";
            string bodyInfo = "您收到用户" + sendUserName + "上传的新订单，订单号：" + orderID + " 【朗开医疗】";
            string mailPort = "25";
            //string attachPath = "E:\\1.txt; E:\\2.txt";
            EmailWork email = new EmailWork(senderServerIp, toMailAddress,subjectInfo, bodyInfo,mailPort, false, false);
            //email.AddAttachments(att,achPath);
            email.Send();
            Console.WriteLine("发送成功");
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
        private void borderNewOrder_MouseEnter(object sender, RoutedEventArgs e)
        {
            borderNewOrder.Opacity = 0.4;
        }
        private void borderNewOrder_MouseLeave(object sender, RoutedEventArgs e)
        {
            borderNewOrder.Opacity = 0.9;
        }

        private void borderNewCetifi_MouseEnter(object sender, RoutedEventArgs e)
        {
            borderNewCetifi.Opacity = 0.4;
        }
        private void borderNewCetifi_MouseLeave(object sender, RoutedEventArgs e)
        {
            borderNewCetifi.Opacity = 0.9;
        }


        private void borderNewUser_MouseEnter(object sender, RoutedEventArgs e)
        {
            borderMessageConfiguration.Opacity = 0.4;
        }
        private void borderNewUser_MouseLeave(object sender, RoutedEventArgs e)
        {
            borderMessageConfiguration.Opacity = 0.9;
        }


        private void borderUserList_MouseEnter(object sender, RoutedEventArgs e)
        {
            borderUserList.Opacity = 0.4;
        }
        private void borderUserList_MouseLeave(object sender, RoutedEventArgs e)
        {
            borderUserList.Opacity = 0.9;
        }
      
       
        private void borderNewOrder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AdminWindow window = new AdminWindow(_UserNameList , _datalist , _waitingApproveUserInfoList);
            window.SetOrderTab();
            window.ShowDialog();
        }
        private void borderNewCetifi_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AdminWindow window = new AdminWindow(_UserNameList, _datalist, _waitingApproveUserInfoList);
            window.SetCertificateCheckTab();
            window.ShowDialog();
        }
        private void borderNewUser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (_UserNameList != null)
            //{
            //    UserCertificateCheckWindow window = new UserCertificateCheckWindow(_UserNameList[5]);
            //    window.ShowDialog();
            //}

            AdminWindow window = new AdminWindow(_UserNameList, _datalist, _waitingApproveUserInfoList);
            window.SetMessageCOnfigurationTab();
            window.ShowDialog();
        }
        private void borderUserList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (_UserNameList != null)
            //{
            //    UserWindow mindow = new UserWindow(_UserNameList[5]);
            //    mindow.ShowDialog();
            //}
            AdminWindow window = new AdminWindow(_UserNameList, _datalist, _waitingApproveUserInfoList);
            window.SetUsrListTab();
            window.ShowDialog();
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }


      
        private GetNotificationsResponse _notification;
        private const int maxBitmapSize = 200 * 1024;
        private const double resizeRatio = 0.4;
      
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

        private void NotificationWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
             this.RefreshRoundWindowRect();
        }


        private void btnShowMessage_Click(object sender, RoutedEventArgs e)
        {


        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult MsgBoxResult;
            MsgBoxResult = MessageBox.Show("是否退出系统？", "提示", MessageBoxButton.OKCancel);
            if (MsgBoxResult == MessageBoxResult.Cancel)
            {
                return;
            }
            else { 
                Application.Current.Shutdown();
            }
        }


        private void btnADMIN_Click(object sender, RoutedEventArgs e)
        {
        }
    }

    
}
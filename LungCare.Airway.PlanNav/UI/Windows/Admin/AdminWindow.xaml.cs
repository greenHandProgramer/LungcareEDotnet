using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using LungCare.SupportPlatform.WebAPIWorkers;
using LungCare.SupportPlatform.Models;
using LungCare_Airway_PlanNav.ZLF.Admin;
using LungCare_Airway_PlanNav;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;

namespace LungCare.SupportPlatform.UI.Windows.Admin
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AdminWindow : Window
    {
        
        public AdminWindow()
        {
            InitializeComponent();
        }

        private List<GetUserInfoResponse> _WaitingApproveUserList;
        private GetUserInfoResponse[] _AllUserInfoList = new GetUserInfoResponse[] { };
        private DataListItem[] _AllOrder = new DataListItem[] { };

        private string _emailReceiveAddress;
        private string _messageReceiveAddress;
        private DispatcherTimer checkNewOrderAndUserTimer;

        public AdminWindow(GetUserInfoResponse[] allUserInfoList, DataListItem[] allOrder, List<GetUserInfoResponse> waitingApproveUserList)
        {
            InitializeComponent();
            headBorder.MouseDown += headBorder_MouseDown;
            _AllUserInfoList = allUserInfoList;
            _WaitingApproveUserList = waitingApproveUserList;
            _AllOrder = allOrder;

            dingdanliebiao.SetDataSource(_AllOrder);
            dingdanliebiao.DeleteHandler += yonghuliebiao_DeleteHandler;

            yonghuliebiao.SetDataSource(_AllUserInfoList);
            yonghuliebiao.DeleteHandler += yonghuliebiao_DeleteHandler;

            yonghushenhe.SetDataSource(_WaitingApproveUserList);
            yonghushenhe.UpdateHandler += yonghuliebiao_DeleteHandler;

            

            _oldWaitingApproveUserNameList = _WaitingApproveUserList.Select(item => item.PhoneNumber).ToList();
            _oldOrderList = _AllOrder;

            StreamReader sr = new StreamReader("configuration.txt");
            string line;
            
            if ((line = sr.ReadLine()) != null)
            {
                _messageReceiveAddress = line.ToString();
            }
            if ((line = sr.ReadLine()) != null) {
                _emailReceiveAddress = line.ToString();
            }
            sr.Close();

            tbEmailReceiveAddress.Text = _emailReceiveAddress;
            tbMessageReceiveAddress.Text = _messageReceiveAddress;

            checkNewOrderAndUserTimer = new DispatcherTimer();
            checkNewOrderAndUserTimer.Interval = TimeSpan.FromSeconds(120);
            checkNewOrderAndUserTimer.Tick +=checkNewOrderAndUserTimer_Tick;
            checkNewOrderAndUserTimer.Start();
        }

        void yonghuliebiao_DeleteHandler(object sender, RoutedEventArgs e)
        {
            getAllOrderInfo();
            //throw new NotImplementedException();
        }

        void checkNewOrderAndUserTimer_Tick(object sender, EventArgs e)
        {
            getAllOrderInfo();
            //throw new NotImplementedException();
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

        public void SetOrderTab()
        {
            tabControl.SelectedIndex = 0;
        }

        public void SetCertificateCheckTab()
        {
            tabControl.SelectedIndex = 1;
        }
        public void SetMessageCOnfigurationTab()
        {
            tabControl.SelectedIndex = 3;
        }


        public void SetUsrListTab()
        {
            tabControl.SelectedIndex = 2;
        }
     

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult MsgBoxResult;
            MsgBoxResult = MessageBox.Show("是否返回？", "提示", MessageBoxButton.OKCancel);
            if (MsgBoxResult == MessageBoxResult.Cancel)
            {
                return;
            }
            else { 
                this.Close();
            }
        }



        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                int tab = this.tabControl.SelectedIndex;
                if (tab == 0)
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        getAllOrderInfo();
                    });
                }

                if (tab == 1) {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        //GetWaitingApproveUser();
                    });
                }
              
                if (tab == 2)
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        getAllUserInfo();
                    });
                }
            }
        }


        private DataListItem[] _oldOrderList; 
        private void getAllOrderInfo(){
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

                            response.DataList = response.DataList.OrderBy(item => string.IsNullOrEmpty(item.UploadTimestamp) ? DateTime.MinValue : DateTime.ParseExact(item.UploadTimestamp, "yyyyMMdd HHmmss", null)).Reverse().ToArray();
                            //response.DataList = response.DataList.Where(item => item.Status == "已上传"||item.Status=="核验成功"||item.Status=="核验失败").ToArray();
                            _AllOrder = response.DataList;
                            if (_oldOrderList == null)
                            {
                                _oldOrderList = response.DataList;
                            }

                            if (response.DataList.Length > _oldOrderList.Length)
                            {
                                GetUserInfoResponse userInfo = _AllUserInfoList.FirstOrDefault(t => t.PhoneNumber == response.DataList[0].UserId);
                                if (userInfo != null)
                                {
                                    Console.WriteLine("you have new order.");
                                    string subjectInfo = "新订单通知 【朗开医疗】";
                                    string bodyInfo = "您收到" + userInfo.Institution + " " + userInfo.ChineseName + "上传的新订单，订单号：" + response.DataList[0].DataID + " 【朗开医疗】";
                                    SendNotificateEmail(subjectInfo , bodyInfo);
                                    SendPhoneMessage(bodyInfo);
                                }

                                _oldOrderList = response.DataList;
                            }
                            //foreach (DataListItem item in _AllOrder)
                            //{
                            //    GetUserInfoResponse userInfo = _AllUserInfoList.FirstOrDefault(t => t.PhoneNumber == item.UserId);
                            //    if (userInfo != null)
                            //    {
                            //        item.ChineseName = userInfo.ChineseName;
                            //    }
                            //}
                            //dingdanliebiao.SetDataSource(_AllOrder);

                            GetWaitingApproveUser();
                        }));
                    },
                    failureCallback:
                    delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            GetWaitingApproveUser();
                            MessageBox.Show("获取数据列表失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback:
                    delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            GetWaitingApproveUser();
                            Util.ShowExceptionMessage(ex, "获取数据列表出错。");
                        }));
                    });
            });
        }

        private List<string> _waitingApproveUserNameList;
        private List<string> _oldWaitingApproveUserNameList;
        private void GetWaitingApproveUser()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                GetCertiWaitingApprovedUserNamesWorker.SendGetCertiWaitingApprovedUserNamesRequest(
                    successCallback: delegate(GetCertiWaitingApprovedUserNamesResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            if (response.UserNameList != null)
                            {
                                _waitingApproveUserNameList = response.UserNameList;
                                if (_oldWaitingApproveUserNameList == null)
                                {
                                    _oldWaitingApproveUserNameList = response.UserNameList;
                                }
                                if (_oldWaitingApproveUserNameList.Count < response.UserNameList.Count)
                                {
                                    Console.WriteLine("new user");
                                    string newUserID = response.UserNameList[response.UserNameList.Count - 1];
                                    if (_AllUserInfoList != null)
                                    {
                                        GetUserInfoResponse newUser = _AllUserInfoList.FirstOrDefault(item => item.PhoneNumber == newUserID);
                                        if (newUser != null)
                                        {
                                            string emailSubject = "用户审核通知";
                                            string emailBody = "您收到用户 " + newUser.ChineseName + "(手机号 " + newUser.PhoneNumber + " ) 提交资格证书审核，请查阅 【朗开医疗】"; ;
                                            SendNotificateEmail(emailSubject, emailBody);
                                            SendPhoneMessage(emailBody);
                                        }
                                    }
                                    _oldWaitingApproveUserNameList = response.UserNameList;
                                }
                            }
                            getAllUserInfo();
                        }));
                    },
                    failureCallback: delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            getAllUserInfo();
                            //MessageBox.Show("获取用户列表。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback: delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            getAllUserInfo();
                            Util.ShowExceptionMessage(ex, "获取用户列表出错。");
                        }));
                    }
                );
            });
        }


        private GetUserInfoResponse[] _oldAllUserInfo;
        private void getAllUserInfo()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                GetAllUserInfoWorker.SendGetAllUserInfoRequest(
                     successCallback: delegate(GetAllUserInfoResponse response)
                     {
                         Dispatcher.Invoke(new Action(delegate
                         {
                             response.DataList = response.DataList.Where(item=>item.RegistrationTimeStamp != null).ToArray();
                             _AllUserInfoList = response.DataList;
                             if (_oldAllUserInfo == null)
                             {
                                 _oldAllUserInfo = response.DataList;
                             }
                             yonghuliebiao.SetDataSource(_AllUserInfoList);
                             if (_waitingApproveUserNameList != null)
                             {
                                 _WaitingApproveUserList = new List<GetUserInfoResponse>();
                                 for (int i = 0; i < _waitingApproveUserNameList.Count; i++)
                                 {
                                     GetUserInfoResponse userInfo = _AllUserInfoList.FirstOrDefault(t => t.PhoneNumber == _waitingApproveUserNameList[i]);
                                     if (userInfo != null)
                                     {
                                         _WaitingApproveUserList.Add(userInfo);
                                     }
                                 }
                                 if (_WaitingApproveUserList != null)
                                 {
                                     yonghushenhe.SetDataSource(_WaitingApproveUserList);
                                 }
                             }
                             foreach (DataListItem item in _AllOrder)
                             {
                                 GetUserInfoResponse userInfo = _AllUserInfoList.FirstOrDefault(t => t.PhoneNumber == item.UserId);
                                 if (userInfo != null)
                                 {
                                     item.ChineseName = userInfo.ChineseName;
                                 }
                             }
                             if (_AllOrder != null)
                             {
                                 dingdanliebiao.SetDataSource(_AllOrder);
                             }
                         }));
                     },
                     failureCallback: delegate(string failureReason)
                     {
                         Dispatcher.Invoke(new Action(delegate
                         {
                             MessageBox.Show("获取用户列表。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                         }));
                     },
                     errorCallback: delegate(Exception ex)
                     {
                         Dispatcher.Invoke(new Action(delegate
                         {
                             Util.ShowExceptionMessage(ex, "获取用户列表出错。");
                         }));
                     }
                 );
            });
        }

        private void SendNotificateEmail(string _subjectInfo, string _bodyInfo)
        {
            //smtp.163.com
            //string senderServerIp = "202.108.5.83";
            //smtp.gmail.com
            //string senderServerIp = "74.125.127.109";
            string senderServerIp = "smtp.qq.com";
            //smtp.qq.com
            //string senderServerIp = "58.251.149.147";
            //string senderServerIp = "smtp.sina.com";
            string toMailAddress = _emailReceiveAddress;
            string subjectInfo = _subjectInfo;
            string bodyInfo = _bodyInfo;
            string mailPort = "25";
            //string attachPath = "E:\\1.txt; E:\\2.txt";
            EmailWork email = new EmailWork(senderServerIp, toMailAddress,subjectInfo, bodyInfo, mailPort, false, false);
            //email.AddAttachments(att,achPath);
            email.Send();
            Console.WriteLine("发送成功");
        }

        private void SendPhoneMessage( string message)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.SendPhoneMessageWorker.SendPhoneMessageRequeset(
                    message,
                    _messageReceiveAddress,
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

        private void btnSetEmailAddress_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbEmailReceiveAddress.Text))
            {
                _emailReceiveAddress = tbEmailReceiveAddress.Text;
                try { 
                    changeTxtFile();
                }
                catch (Exception ex){
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void btnSetMessagePhoneAddress_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbMessageReceiveAddress.Text))
            {
                _messageReceiveAddress = tbMessageReceiveAddress.Text;
                try
                {
                    changeTxtFile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void changeTxtFile() {
            FileStream fs = new FileStream("configuration.txt",FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(_messageReceiveAddress);
            sw.WriteLine(_emailReceiveAddress);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

    }
}

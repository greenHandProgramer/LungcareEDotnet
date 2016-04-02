using LungCare.SupportPlatform.Models;
using LungCare_Airway_PlanNav.ZLF.Admin;
using LungCare_Airway_PlanNav;
using System.Linq;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LungCare.SupportPlatform.WebAPIWorkers;
using System.Diagnostics;
using System.Collections.Generic;
using LungCare.SupportPlatform.UI.Windows.Examination;

namespace LungCare.SupportPlatform.UI
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            //textBoxUserName.Text = "admin";
            //passwordBoxPassword.Password = "admin18600406312";
            //textBoxUserName.Text = "18600757116";
            //passwordBoxPassword.Password = "1234567";
            textBoxUserName.Text = "18552024921";
            passwordBoxPassword.Password = "123456";
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {
                
               // throw;
            }
          

            // print the Left and Top property values of the Window into the Output window
            //Console.WriteLine(this.Left.ToString() + " " + this.Top.ToString());
        }
      
    
        void headBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            //throw new NotImplementedException();
        }

        private void textBoxUserName_TextInput(object sender, TextCompositionEventArgs e)
        {
        }

        private void textBoxUserName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxUserName.Text == textBoxUserName.Tag.ToString())
            {
                textBoxUserName.Text = string.Empty;
            }
        }

        private void textBoxUserName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxUserName.Text == string.Empty)
            {
                textBoxUserName.Text = textBoxUserName.Tag.ToString();
            }
        }

        private void textBoxPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            //textBoxPassword.LostFocus -= textBoxPassword_LostFocus;
            textBoxPassword.Visibility = Visibility.Hidden;
            passwordBoxPassword.Visibility = Visibility.Visible;
            passwordBoxPassword.Focus();
            //textBoxPassword.LostFocus += textBoxPassword_LostFocus;
        }

        private void textBoxPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (passwordBoxPassword.Password == string.Empty)
            {
                textBoxPassword.Visibility = Visibility.Visible;
                passwordBoxPassword.Visibility = Visibility.Hidden;
            }
        }

        private void PauseGUI()
        {
            labelWaitLogin.Visibility = Visibility.Visible;
            ((UIElement)Content).IsEnabled = false;
            Cursor = Cursors.Wait;
        }

        private void ResumeGUI()
        {
            labelWaitLogin.Visibility = Visibility.Hidden;
            ((UIElement)Content).IsEnabled = true;
            Cursor = Cursors.Arrow;
        }

        private void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow 上传列表Wnd = new MainWindow();
            //上传列表Wnd.Loaded += delegate
            //{
            //    Visibility = Visibility.Hidden;
            //};
            //上传列表Wnd.ShowDialog();

            //return;

            {
                bool userNameMissing = textBoxUserName.Text.Trim() == textBoxUserName.Tag.ToString() || textBoxUserName.Text.Trim() == string.Empty;
                bool passwordMissing = passwordBoxPassword.Password.Trim() == string.Empty;

                if (userNameMissing && passwordMissing)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        MessageBox.Show("请输入用户名和密码。");
                    }));

                    return;
                }
                if (userNameMissing)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        MessageBox.Show("请输入用户名。");
                    }));

                    return;
                }
                if (passwordMissing)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        MessageBox.Show("请输入密码。");
                    }));

                    return;
                }
                //if (!IsPhoneNumberValid())
                if(false)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        MessageBox.Show("请正确输入11位手机号。");
                    }));
                    return;
                }
            }

            labelWaitLogin.Content = "正在登陆，请稍候";
            PauseGUI();

            LoginRequest LoginRequest = new LoginRequest();
            LoginRequest.UserName = textBoxUserName.Text;
            LoginRequest.Password = passwordBoxPassword.Password;
            LoginRequest.Sender = "PC Client";

            ThreadPool.QueueUserWorkItem(delegate
            {
                attemptCount = 0;
                
                AttemptLogin(LoginRequest, attemptCount);
            });
        }

        int attemptCount = 0;
        private List<string> _waitingApproveUserNameList;
        private LungCare.SupportPlatform.Models.DataListItem[] _datalist;
        private GetUserInfoResponse[] _UserNameList;
        private void getAllNewOrder()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                //Dispatcher.Invoke(new Action(delegate
                //{
                //}));
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
                            _datalist = response.DataList;
                            
                            _datalist = _datalist.OrderBy(item => string.IsNullOrEmpty(item.UploadTimestamp) ? DateTime.MinValue : DateTime.ParseExact(item.UploadTimestamp, "yyyyMMdd HHmmss", null)).Reverse().ToArray();
                            //_datalist = _datalist.Where(item => item.Status == "已上传"||item.Status=="核验成功"||item.Status=="核验失败").ToArray();
                            GetWaitingApproveUser();

                        }));
                    },
                    failureCallback:
                    delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            MessageBox.Show("获取数据列表失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback:
                    delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            Util.ShowExceptionMessage(ex, "获取数据列表出错。");
                        }));
                    });
            });
        }

        private void GetWaitingApproveUser()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                GetCertiWaitingApprovedUserNamesWorker.SendGetCertiWaitingApprovedUserNamesRequest(
                    successCallback: delegate(GetCertiWaitingApprovedUserNamesResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            _waitingApproveUserNameList = response.UserNameList;

                            //for (int i = 0; i < response.UserNameList.Count; i++)
                            //{
                            //    GetUserInfo(response.UserNameList[i]);
                            //}

                            GetAllUserInfo();
                        }));
                    },
                    failureCallback: delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            //MessageBox.Show("获取用户列表。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            GetAllUserInfo();
                        }));
                    },
                    errorCallback: delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "获取用户列表出错。");
                            GetAllUserInfo();
                        }));
                    }
                );
            });
        }

        List<UserInfo> _WaitingApproveUserInfoList = new List<UserInfo>();

        private void GetUserInfo(string userID)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                GetUserInfoWorker.SendGetUserInfoRequest(userID,
                    successCallback: delegate(UserInfo response)
                {
                    UserInfo item = new UserInfo();
                    item.ChineseName = response.ChineseName;
                    item.PhoneNumber = response.PhoneNumber;
                    item.Institution = response.Institution;
                    item.Department = response.Department;
                    _WaitingApproveUserInfoList.Add(item);
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

        private List<GetUserInfoResponse> _WaitingUserInfoList;
        private void GetAllUserInfo()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                GetAllUserInfoWorker.SendGetAllUserInfoRequest(
                     successCallback: delegate(GetAllUserInfoResponse response)
                     {
                         Dispatcher.Invoke(new Action(delegate
                         {
                             _UserNameList = response.DataList;
                             _UserNameList = _UserNameList.Where(item => item.RegistrationTimeStamp != null).ToArray();
                             _WaitingUserInfoList = new List<GetUserInfoResponse>();
                             if (_waitingApproveUserNameList != null) { 
                                 for (int i = 0; i < _waitingApproveUserNameList.Count; i++)
                                 {
                                     GetUserInfoResponse userInfo = _UserNameList.FirstOrDefault(t => t.PhoneNumber == _waitingApproveUserNameList[i]);
                                     if (userInfo != null)
                                     {
                                         _WaitingUserInfoList.Add(userInfo);
                                     }
                                 }

                             }
                                 foreach(DataListItem item in _datalist){
                                     GetUserInfoResponse userInfo = _UserNameList.FirstOrDefault(t => t.PhoneNumber == item.UserId);
                                     if (userInfo != null) {
                                         item.ChineseName = userInfo.ChineseName;
                                     }
                                 }
                                 //AdminWindow adminWindow = new AdminWindow(_UserNameList , _datalist , _WaitingApproveUserInfoList);
                                 //adminWindow.Loaded += delegate
                                 //{
                                 //    Visibility = Visibility.Hidden;
                                 //};
                                 //adminWindow.ShowDialog();
                                 NotificationWindow admin = new NotificationWindow(_WaitingUserInfoList, _datalist, _UserNameList);
                                 admin.Loaded += delegate
                                 {
                                     Visibility = System.Windows.Visibility.Hidden;
                                 };

                                 admin.ShowDialog();
                         }));
                     },
                     failureCallback: delegate(string failureReason)
                     {
                         Dispatcher.Invoke(new Action(delegate
                         {
                             ResumeGUI();
                             MessageBox.Show("获取用户列表。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                         }));
                     },
                     errorCallback: delegate(Exception ex)
                     {
                         Dispatcher.Invoke(new Action(delegate
                         {
                             ResumeGUI();
                             Util.ShowExceptionMessage(ex, "获取用户列表出错。");
                         }));
                     }
                 );
            });
        }
        private void AttemptLogin(LoginRequest LoginRequest, int attemptCount)
        {
           
            LoginWorker.SendLoginRequest(
                LoginRequest.UserName,
                LoginRequest.Password,
                successCallback: delegate (LoginResponse response)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        Security.TokenManager.Token = response.Token;
                        Security.SessionManager.UserName = LoginRequest.UserName;
                        labelWaitLogin.Content = "正在获取用户信息";
                        if (LoginRequest.UserName == "admin")
                        {
                            getAllNewOrder();
                        }
                        else
                        {
                            LoadUserInfo();
                        }
                    }));
                },
                failureCallback: delegate (string failureReason)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        ResumeGUI();
                        string failureReasonChinese = failureReason;
                        if (failureReason == "UserNotRegistrated")
                        {
                            failureReasonChinese = string.Format("您输入的手机号{0}尚未注册，请先注册。", LoginRequest.UserName);
                        }
                        if (failureReason == "PasswordIncorrect")
                        {
                            failureReasonChinese = "您输入的密码错误，请重新输入";
                        }
                        MessageBox.Show("登录失败。" + failureReasonChinese, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }));
                },
                errorCallback: delegate (Exception ex)
                {
                    if (attemptCount < 5)
                    {
                        attemptCount++;
                        AttemptLogin(LoginRequest, attemptCount);
                    }
                    else
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            Util.ShowExceptionMessage(ex, "登录出错。");
                        }));
                    }
                });
        }

        private GetCertificateApproveStatusResponse _certificateApproveStatus;
        private DownloadCertificateRespnse _certificateRespnse;
        private DownloadUserIconRespnse _downloadUserIcon;
        private GetUserInfoResponse _userInfo;
        private GetNotificationsResponse _notifications;
        private void LoadUserInfo()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.RetrieveUserInfoWorker.SendUserInfoRequest(
                    successCallback: delegate (LungCare.SupportPlatform.Models.GetUserInfoResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            _userInfo = response;

                            GetCertificateApproveStatus();


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
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "获取用户信息出错。");
                        }));
                    });
            });
        }


        private void SendPhoneMessage()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.SendPhoneMessageWorker.SendPhoneMessageRequeset(
                    "收到订单【朗开医疗】",
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
                            ResumeGUI();
                            MessageBox.Show(failureReason);
                        }));
                    },
                    errorCallback: delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "获取用户信息出错。");
                        }));
                    });
            });
        }

        private void GetCertificateApproveStatus()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.GetCertificateApproveStatusWorker.SendGetCertificateApproveStatusRequeset(
                    _userInfo.PhoneNumber,
                    successCallback: delegate (LungCare.SupportPlatform.Models.GetCertificateApproveStatusResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            _certificateApproveStatus = response;
                            LoadNotification();
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            loadUserIconImage();

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
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "获取证书审查状态出错。");
                        }));
                    });
            });
        }


        private void loadUserIconImage()
        {

            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.DownloadUserIconWorker.SendUserIconRequest(
                    successCallback: delegate (LungCare.SupportPlatform.Models.DownloadUserIconRespnse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            this._downloadUserIcon = response;
                            loadCertifyImage();
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
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "下载用户头像出错。");
                        }));
                    });
            });
        }

        private void loadCertifyImage()
        {

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
                            ResumeGUI();
                            MessageBox.Show(failureReason);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "下载资格证出错。");
                        }));
                    });
            });
        }

        private void LoadNotification()
        {

            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.NotificationsWorker.GetNotificationRequest(
                    this._userInfo.PhoneNumber,
                    successCallback: delegate (LungCare.SupportPlatform.Models.GetNotificationsResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            if (response.NotificationList != null)
                            {
                                response.NotificationList.Sort(delegate (NotificationItem t1, NotificationItem t2)
                                {
                                    return -t1.TimeStamp.CompareTo(t2.TimeStamp);
                                });
                            }
                            this._notifications = response;
                            ResumeGUI();
                           
                                MaWindow wnd = new MaWindow(_userInfo, _downloadUserIcon, _certificateRespnse, _certificateApproveStatus, this._notifications);
                                wnd.Loaded += delegate
                                {
                                    Visibility = Visibility.Hidden;
                                };
                                wnd.ShowDialog();
                           
                              
                            
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
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "获取消息列表出错。");
                        }));
                    });
            });
        }

        private void llSignup_Click(object sender, RoutedEventArgs e)
        {
            UI.SignupWnd SignupWnd = new UI.SignupWnd();
            SignupWnd.ShowDialog();
        }

        private void llForgetPassword_Click(object sender, RoutedEventArgs e)
        {
            UI.ForgetPasswordWnd SignupWnd = new UI.ForgetPasswordWnd();
            SignupWnd.ShowDialog();
        }

        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            AdminWnd AdminWnd = new AdminWnd();
            AdminWnd.Owner = this;
            AdminWnd.Closed += delegate
            {
                Show();
            };
            AdminWnd.ShowDialog();
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ask4Exit();
        }

        private void Ask4Exit()
        {
            if (MessageBox.Show("是否退出朗开医疗肺e助手？", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                //Process.GetCurrentProcess().Kill();
                Application.Current.Shutdown();
            }
        }

        private void passwordBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonLogin_Click(sender, e);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Ask4Exit();
            }
        }

        private bool IsPhoneNumberValid()
        {
            if (String.IsNullOrEmpty(textBoxUserName.Text))
            {
                return false;
            }
            if (textBoxUserName.Text.Length != 11)
            {
                return false;
            }
            try
            {
                //num = Convert.ToInt32(txtTEL.Text.Trim().ToString());  
                Convert.ToInt64(textBoxUserName.Text.Trim());
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private void btnMin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }
    }
}

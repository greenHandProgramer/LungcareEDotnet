using LungCare.SupportPlatform.Models;
using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace LungCare.SupportPlatform.UI.Windows.Examination
{
    /// <summary>
    /// MsgWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadListWindow1 : Window
    {
        public DownloadListWindow1()
        {
            InitializeComponent();


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


        private void loadUploadFile()
        {

        }
        private void loadHandledData()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    DownloadListUserControl.StartLoading();
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
                            }
                            DownloadListUserControl.FinishLoading();

                            DownloadListUserControl.SetDataSource(response.DataList);
                        }));
                    },
                    failureCallback:
                    delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            DownloadListUserControl.FinishLoading();
                            MessageBox.Show("获取数据列表失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback:
                    delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            DownloadListUserControl.FinishLoading();
                            Util.ShowExceptionMessage(ex, "获取数据列表出错。");
                        }));
                    });
            });
        }
        


        void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
            //throw new NotImplementedException();
        }
        public string MsgText { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void borderMessage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        

        
    }
}

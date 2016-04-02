using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LungCare.SupportPlatform.UI
{
    /// <summary>
    /// ForgetPasswordWnd.xaml 的交互逻辑
    /// </summary>
    public partial class ForgetPasswordWnd : Window
    {
        public ForgetPasswordWnd()
        {
            InitializeComponent();
        }

        private void btnLeave_Click(object sender, RoutedEventArgs e)
        {
            Ask4Abort();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Ask4Abort();
            }
        }
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
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
        private void Ask4Abort()
        {
            if (MessageBox.Show("是否放弃重置密码？", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            CheckValidateStatus();

            string verifyCode = tbVerifyCode.Text.Trim();
            string username = tbPhoneNumber.Text.Trim();
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.ResetPasswordByVerifyCodeWorker.ResetPasswordByVerifyCodeRequest(
                    verifyCode,
                    username,
                    Util.Encrypt(pbNewPassword.Password),
                    successCallback: delegate(LungCare.SupportPlatform.Models.ResetPasswordByVerifyCodeResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("密码重置成功！");
                            this.Close();
                        }));
                    },
                    failureCallback: delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            string failureReasonChinese = failureReason;
                            if (failureReason == "UserNotRegistrated")
                            {
                                failureReasonChinese = string.Format("您输入的手机号{0}尚未注册，请先注册。", username);
                            }
                            if (failureReason == "VerifyCodeIncorrect")
                            {
                                failureReasonChinese = string.Format("您输入的验证码错误或已过期，请重新输入或获取验证码。");
                            }
                            MessageBox.Show("密码重置失败。" + failureReasonChinese);
                        }));
                    },
                    errorCallback: delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "登录出错。");
                        }));
                    });
            });

        }

        private void btnLeave_Click(object sender, MouseButtonEventArgs e)
        {
            Ask4Abort();
        }

        private void CheckValidateStatus()
        {
            if(labelPhoneNumberErrorPrompt.Content!="" || labelPhoneNumberErrorPrompt_Copy.Content!="" || labelPasswordErrorPrompt.Content!=""
                || labelPasswordRetypeErrorPrompt.Content != "")
            {
                btnResetPassword.IsEnabled = false;
                return;
            }
            else
            {
            }
        }
        private void tbPhoneNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbPhoneNumber.Text.Trim() == "")
            {
                labelPhoneNumberErrorPrompt.Content = "请填写手机号码";
                btnGetVerifyCode.IsEnabled = false;
            }
            else
            {
                labelPhoneNumberErrorPrompt.Content = "";
                btnGetVerifyCode.IsEnabled = true;

            }

            CheckValidateStatus();
        }

        private void tbPhoneNumber_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbPhoneNumber.Text.Trim() == "")
            {
                labelPhoneNumberErrorPrompt.Content = "请填写手机号码";
                btnGetVerifyCode.IsEnabled = false;
            }
            else if(tbPhoneNumber.Text.Length>=11)
            {
                labelPhoneNumberErrorPrompt.Content = "";
                btnGetVerifyCode.IsEnabled = true;

            }

            CheckValidateStatus();
        }


        private void tbVerifyCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbVerifyCode.Text.Trim() == "")
            {
                labelPhoneNumberErrorPrompt_Copy.Content = "请输入验证码";
            }
            else
            {
                labelPhoneNumberErrorPrompt_Copy.Content = "";
                pbNewPassword.IsEnabled = true;
                pbNewPasswordRetype.IsEnabled = true;
            }

            CheckValidateStatus();
        }


        private void tbVerifyCode_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbVerifyCode.Text.Trim() == "")
            {
                labelPhoneNumberErrorPrompt_Copy.Content = "请输入验证码";
            }
            else
            {
                labelPhoneNumberErrorPrompt_Copy.Content = "";
                pbNewPassword.IsEnabled = true;
                pbNewPasswordRetype.IsEnabled = true;
            }

            CheckValidateStatus();
        }



        private void pbNewPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (pbNewPassword.Password.Trim() == "")
            {
                labelPasswordErrorPrompt.Content = "请填写密码";
            }
            else if (pbNewPassword.Password.Length < 6 || pbNewPassword.Password.Length > 20)
            {
                labelPasswordErrorPrompt.Content = "密码长度为6-20";
            }
            else
            {
                labelPasswordErrorPrompt.Content = "";
            }

            CheckValidateStatus();
        }

        private void pbNewPasswordRetype_LostFocus(object sender, RoutedEventArgs e)
        {
            if (pbNewPasswordRetype.Password.Trim() == "")
            {
                labelPasswordRetypeErrorPrompt.Content = "请输入密码";
            }
            else if (pbNewPasswordRetype.Password.Length < 6 || pbNewPasswordRetype.Password.Length > 20)
            {
                labelPasswordRetypeErrorPrompt.Content = "密码长度为6-20";
            }
            else if (pbNewPassword.Password != pbNewPasswordRetype.Password)
            {
                labelPasswordRetypeErrorPrompt.Content = "两次输入的密码不相同";
            }
            else
            {
                labelPasswordRetypeErrorPrompt.Content = "";
                btnResetPassword.IsEnabled = true;
            }
            CheckValidateStatus();
        }

        private void pbNewPasswordRetype_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (pbNewPasswordRetype.Password.Trim() == "")
            {
                labelPasswordRetypeErrorPrompt.Content = "请输入密码";
            }
            else if (pbNewPasswordRetype.Password.Length < 6 || pbNewPasswordRetype.Password.Length > 20)
            {
                labelPasswordRetypeErrorPrompt.Content = "密码长度为6-20";
            }
            else if (pbNewPassword.Password != pbNewPasswordRetype.Password)
            {
                labelPasswordRetypeErrorPrompt.Content = "两次输入的密码不相同";
            }
            else
            {
                labelPasswordRetypeErrorPrompt.Content = "";
                btnResetPassword.IsEnabled = true;
            }
            CheckValidateStatus();
        }

        private DispatcherTimer timer;
        public int seconds = 180;
        private void timer_Tick(object sender, EventArgs e)
        {
            btnGetVerifyCode.Content = seconds.ToString() + "秒后重发";
            seconds -= 1;
            if (seconds == 1)
            {
                btnGetVerifyCode.Content = "重新发送验证码";
                btnGetVerifyCode.IsEnabled = true;
                tbPhoneNumber.IsEnabled = true;
                seconds = 180;
                timer.Stop();
            }
        }
        private void btnGetVerifyCode_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tbPhoneNumber.Text))
            {
                labelPhoneNumberErrorPrompt.Content = "请先输入手机号";
                return;
            }

            string phoneNumber = tbPhoneNumber.Text.Trim();
            tbPhoneNumber.IsEnabled = false;
            btnGetVerifyCode.Content = seconds.ToString() + "秒后重发";
            btnGetVerifyCode.IsEnabled = false;


            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.AskForgetPasswordVerifCodeWorker.SendDownloadCertificateRequest(
                    phoneNumber,
                    successCallback: delegate(LungCare.SupportPlatform.Models.AskForgetPasswordVerifCodeResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            timer = new DispatcherTimer();
                            timer.Tick += new EventHandler(timer_Tick);
                            timer.Interval = TimeSpan.FromSeconds(1);
                            btnGetVerifyCode.IsEnabled = false;
                            timer.Start();
                            MessageBox.Show("验证码已发送到: " + phoneNumber + "，请及时查看手机");
                        }));
                    },
                    failureCallback: delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            string failureReasonChinese = failureReason;
                            if (failureReason == "UserNotRegistrated")
                            {
                                failureReasonChinese = string.Format("您输入的手机号{0}尚未注册，请先注册。", phoneNumber);
                            }
                            tbPhoneNumber.IsEnabled = true;
                            btnGetVerifyCode.Content = "获取验证码";
                            btnGetVerifyCode.IsEnabled = true;

                            MessageBox.Show("获取验证码失败。" + failureReasonChinese);
                        }));
                    },
                    errorCallback: delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            tbPhoneNumber.IsEnabled = true;
                            btnGetVerifyCode.Content = "获取验证码";
                            btnGetVerifyCode.IsEnabled = true;
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "发送出错。");
                        }));
                    });
            });

        }


        private void pbNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (pbNewPassword.Password.Trim() == "")
            {
                labelPasswordErrorPrompt.Content = "请填写密码";
            }
            else if (pbNewPassword.Password.Length < 6 || pbNewPassword.Password.Length > 20)
            {
                labelPasswordErrorPrompt.Content = "密码长度为6-20";
            }
            else
            {
                labelPasswordErrorPrompt.Content = "";
            }

            CheckValidateStatus();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            Ask4Abort();
        }
    }
}

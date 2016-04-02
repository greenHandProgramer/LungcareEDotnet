using System.Linq;
using System;
using System.IO;
using System.Threading;
using System.Windows;

using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LungCare.SupportPlatform.UI
{
    /// <summary>
    /// SignupWnd.xaml 的交互逻辑
    /// </summary>
    public partial class SignupWnd : Window
    {
        System.Windows.Controls.Label[] labels;

        public SignupWnd()
        {
            InitializeComponent();

            labels = new System.Windows.Controls.Label[] {
                labelArgumentErrorPrompt,
                labelChineseNameErrorPrompt,
                labelDepartmentErrorPrompt,
                labelHospErrorPrompt,
                labelPasswordErrorPrompt,
                labelPasswordRetypeErrorPrompt,
                labelPhoneNumberErrorPrompt
            };

            foreach (var label in labels)
            {
                label.Visibility = Visibility.Hidden;
            }

            System.Windows.Controls.TextBox[] inputs = new System.Windows.Controls.TextBox[] {
                tbPhoneNumber,
                tbChineseName,
                tbDepartment,
                tbHosp,
            };

            foreach (var item in inputs)
            {
                item.TextChanged += Item_TextChanged;
            }

            System.Windows.Controls.PasswordBox[] passwords = new System.Windows.Controls.PasswordBox[] {
                pbPassword,
                pbPasswordRetype
            };

            foreach (var item in passwords)
            {
                item.PasswordChanged += Item_PasswordChanged;
            }

            cbLiceneseAgreed.Unchecked += Item_PasswordChanged;
            cbLiceneseAgreed.Checked += Item_PasswordChanged;

            UpdateLabels();
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
        private void Item_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateLabels();
        }

        private void Item_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            //if (!cbLiceneseAgreed.IsChecked.Value)
            //{
            //    labelArgumentErrorPrompt.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    labelArgumentErrorPrompt.Visibility = Visibility.Hidden;
            //}

            if (tbHosp.Text.Trim() == string.Empty)
            {
                labelHospErrorPrompt.Visibility = Visibility.Visible;
            }
            else
            {
                labelHospErrorPrompt.Visibility = Visibility.Hidden;
            }

            if (!IsPhoneNumberValid())
            {
                labelPhoneNumberErrorPrompt.Visibility = Visibility.Visible;
            }
            else
            {
                labelPhoneNumberErrorPrompt.Visibility = Visibility.Hidden;
            }

            if (tbChineseName.Text.Trim() == string.Empty)
            {
                labelChineseNameErrorPrompt.Visibility = Visibility.Visible;
            }
            else
            {
                labelChineseNameErrorPrompt.Visibility = Visibility.Hidden;
            }
            if (tbDepartment.Text.Trim() == string.Empty)
            {
                labelDepartmentErrorPrompt.Visibility = Visibility.Visible;
            }
            else
            {
                labelDepartmentErrorPrompt.Visibility = Visibility.Hidden;
            }
            if (pbPassword.Password.Trim() == string.Empty)
            {
                labelPasswordErrorPrompt.Visibility = Visibility.Visible;
            }
            else
            {
                labelPasswordErrorPrompt.Visibility = Visibility.Hidden;
            }
            if (pbPassword.Password != pbPasswordRetype.Password)
            {
                labelPasswordRetypeErrorPrompt.Visibility = Visibility.Visible;
            }
            else
            {
                labelPasswordRetypeErrorPrompt.Visibility = Visibility.Hidden;
            }

            bool hasError = labels.Count(item => item.Visibility == Visibility.Visible) != 0;
            btnSignup.IsEnabled = !hasError;
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

        private string _certificate=null;
        private BitmapImage _bitmap;
        private void btnUploadCert_Click(object sender, RoutedEventArgs e)
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
                    System.Windows.Forms.MessageBox.Show("请上传图片格式文件");
                    return;
                }
                if (file.Length > 5 * 1024 * 1024)
                {
                    System.Windows.Forms.MessageBox.Show("请上传小于5M的图片文件");
                    return;
                }
                Uri uri = new Uri(openFileDialog.FileName);
                BitmapImage bitmap0 = new BitmapImage(uri);
                BitmapImage bitmap1 = new BitmapImage();
                //if (file.Length > 200 * 1024)
                try
                {
                    _bitmap = new BitmapImage(uri);



                    _certificate = ToBase64(_bitmap);

                    btnView.Visibility = System.Windows.Visibility.Visible;
                }
                catch (Exception)
                {
                    MessageBox.Show("请上传图片文件");
                    
                }
                   
            }


            return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Cursor = System.Windows.Input.Cursors.Wait;
                IsEnabled = false;

                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        Uri uri = new Uri(ofd.FileName);
                        _bitmap = new BitmapImage();
                        _bitmap.BeginInit();
                        using (var stream = new MemoryStream(File.ReadAllBytes(ofd.FileName)))
                        {
                            _bitmap.StreamSource = stream;
                            _bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            _bitmap.EndInit();
                            _bitmap.Freeze();
                        }

                        _certificate = ToBase64(_bitmap);
                        this.Dispatcher.Invoke((Action)delegate
                        {
                           // image.Source = bitmap;
                        });
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            System.Windows.MessageBox.Show("请选择图片文件。" + ex.Message);
                        }));
                    }
                    finally
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            IsEnabled = true;
                            Cursor = System.Windows.Input.Cursors.Arrow;
                        }));
                    }
                });
            }
        }

        private void btnSignup_Click(object sender, RoutedEventArgs e)
        {
            //if (!cbLiceneseAgreed.IsChecked.Value)
            //{
            //    System.Windows.MessageBox.Show("请先选择同意协议。");
            //    return;
            //}
            if (!IsPhoneNumberValid())
            {
                labelPhoneNumberErrorPrompt.Content = "请正确输入11位手机号";
                return;
            }
            if (tbChineseName.Text.Trim() == string.Empty)
            {
                System.Windows.MessageBox.Show("请填姓名");
                return;
            }
            if (tbVerifiyCode.Text.Trim() == string.Empty)
            {
                System.Windows.MessageBox.Show("请填验证码");
                return;
            }
            if (tbDepartment.Text.Trim() == string.Empty)
            {
                System.Windows.MessageBox.Show("请填写科室");
                return;
            }
            if (pbPassword.Password.Trim() == string.Empty)
            {
                System.Windows.MessageBox.Show("密码不能为空");
                return;
            }
            if (pbPassword.Password != pbPasswordRetype.Password)
            {
                System.Windows.MessageBox.Show("两次输入的密码不一致");
                return;
            }

            if (!cbLiceneseAgreed.IsChecked.Value)
            {
                System.Windows.MessageBox.Show("请先阅读相关协议");
                return;
            }
            ((UIElement)Content).IsEnabled = false;
            Cursor = System.Windows.Input.Cursors.Wait;

            string phoneNumber = tbPhoneNumber.Text;
            string password = pbPassword.Password;
            string chineseName = tbChineseName.Text;
            string hosp = tbHosp.Text;
            string department = tbDepartment.Text;
            string verifyCode = tbVerifiyCode.Text;

            ThreadPool.QueueUserWorkItem(delegate
            {
                WebAPIWorkers.SignupWorker.SendSignupRequest(
                    phoneNumber,
                    password,
                    chineseName,
                    hosp,
                    department,
                    _certificate,
                    verifyCode,
                    successCallback: delegate
                    {
                        Resume();
                        System.Windows.MessageBox.Show("注册成功，请登录。");
                        InvokeClose();
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            string failureReasonChinese = failureReason;
                            if (failureReason == "UserAlreadyRegistrated")
                            {
                                failureReasonChinese = string.Format("您输入的手机号{0}已经被注册，请不要重复注册。", phoneNumber);
                            }
                            if (failureReason == "VerifyCodeIncorrect")
                            {
                                failureReasonChinese = "您输入的验证码错误或已过期，请重新输入";
                            }

                            seconds = 1;

                            System.Windows.MessageBox.Show("注册失败。" + failureReasonChinese);

                            Resume();
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Resume();
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "注册出错。");
                        }));
                    });
            });
        }

        private void Resume()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                ((UIElement)Content).IsEnabled = true;
                Cursor = System.Windows.Input.Cursors.Arrow;
            }));
        }

        private void InvokeClose()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                Close();
            }));
        }

        private void btnClose_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (System.Windows.MessageBox.Show("是否放弃注册？", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        private bool IsPhoneNumberValid()
        {
            if (String.IsNullOrEmpty(tbPhoneNumber.Text))
            {
                return false;
            }
            if (tbPhoneNumber.Text.Length != 11)
            {
                return false;
            }
            try
            {
                //num = Convert.ToInt32(txtTEL.Text.Trim().ToString());  
                Convert.ToInt64(tbPhoneNumber.Text.Trim());
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private DispatcherTimer timer;
        public int seconds = 180;
        private void timer_Tick(object sender, EventArgs e)
        {
            btn发送验证码.Content = seconds.ToString() + "秒后重发";
            seconds -= 1;
            if (seconds <= 1)
            {
                btn发送验证码.Content = "重新发送验证码";
                btn发送验证码.IsEnabled = true;
                tbPhoneNumber.IsEnabled = true;
                seconds = 180;
                timer.Stop();
            }
        }

        private void btn发送验证码_Click(object sender, RoutedEventArgs e)
        {
            if (!IsPhoneNumberValid())
            {
                labelPhoneNumberErrorPrompt.Content = "请正确输入11位手机号";
                return;
            }

            btn发送验证码.Content = seconds.ToString() + "秒后重发";
            btn发送验证码.IsEnabled = false;
            tbPhoneNumber.IsEnabled = false;
            string phoneNumber = tbPhoneNumber.Text.Trim();
            ThreadPool.QueueUserWorkItem(delegate
            {
                WebAPIWorkers.GetSignupVerifyCodeWorker.SendGetSignupVerifyCodeRequest(
                    phoneNumber,
                    successCallback: delegate (Models.GetSignupVerifyCodeResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            timer = new DispatcherTimer();
                            timer.Tick += new EventHandler(timer_Tick);
                            timer.Interval = TimeSpan.FromSeconds(1);
                            btn发送验证码.IsEnabled = false;
                            timer.Start();
                            
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            string failureReasonChinese = failureReason;
                            if (failureReason == "UserAlreadyRegistrated")
                            {
                                failureReasonChinese = string.Format("您输入的手机号{0}已经被注册，请不要重复注册。", phoneNumber);
                            }
                            btn发送验证码.Content = "发送验证码";
                            btn发送验证码.IsEnabled = true;
                            tbPhoneNumber.IsEnabled = true;
                            System.Windows.MessageBox.Show("获取验证码失败。" + failureReasonChinese);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            btn发送验证码.Content = "发送验证码";
                            btn发送验证码.IsEnabled = true;
                            tbPhoneNumber.IsEnabled = true;
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "发送出错。");
                        }));
                    });
            });
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap != null)
            {
                ImageWindow window = new ImageWindow(_bitmap);
                window.ShowDialog();
            }
          
        }

        private void protocal_Click(object sender, RoutedEventArgs e)
        {
            Cursor = System.Windows.Input.Cursors.Wait;   
            string filename = "肺e助手 用户注册协议.pdf";
            try
            {
                System.Diagnostics.Process.Start(filename);
            }
            catch (Exception)
            {
                MessageBox.Show("");
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        

    }
}

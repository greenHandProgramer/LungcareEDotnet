using System.Linq;

using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.Threading;
using System.Windows;
using LungCare.SupportPlatform.Models;
using System.ComponentModel;
using LungCare.SupportPlatform.Network;
using System.Collections.Generic;
using LungCare.SupportPlatform.UI;
using System.IO;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LungCare_Airway_PlanNav.ZLF.Admin
{
    /// <summary>
    /// AdminWnd.xaml 的交互逻辑
    /// </summary>
    public partial class AdminWnd : Window
    {
        public AdminWnd()
        {
            InitializeComponent();
        }

        List<string> _UserNameList = new List<string>();

        private void btn获取用户列表_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                GetUserNamesWorker.SendGetUserNamesRequest(
                    successCallback: delegate (GetUserNamesResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            _UserNameList = response.UserNameList;

                            dataGridUserList.DataContext = response.UserNameList.Select(item => new { UserId = item });
                            dataGridUserList.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("获取用户列表。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "获取用户列表出错。");
                        }));
                    }
                );
            });
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            LungCare.SupportPlatform.Models.LoginRequest LoginRequest = new LungCare.SupportPlatform.Models.LoginRequest();
            LoginRequest.UserName = tbUserName.Text;
            LoginRequest.Password = tbPassword.Text;
            LoginRequest.Sender = "PC Client";

            ThreadPool.QueueUserWorkItem(delegate
            {
                LoginWorker.SendLoginRequest(
                    LoginRequest.UserName,
                    LoginRequest.Password,
                    successCallback: delegate (LungCare.SupportPlatform.Models.LoginResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            LungCare.SupportPlatform.Security.TokenManager.Token = response.Token;
                            LungCare.SupportPlatform.Security.SessionManager.UserName = LoginRequest.UserName;

                            lbToken.Content = LungCare.SupportPlatform.Security.TokenManager.Token;

                            btn获取用户列表_Click(sender, e);
                            //NotificationsWorker.SendTest();
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("登录失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "登录出错。");
                        }));
                    });
            });
        }

        string SelectedUser
        {
            get
            {
                object selectedUser = dataGridUserList.SelectedValue;

                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(selectedUser);
                PropertyDescriptor pdID = pdc.Find("UserId", true);
                var id = pdID.GetValue(selectedUser);

                return id.ToString();
            }
        }

        private void btn获取订单列表_Click(object sender, RoutedEventArgs e)
        {
            RetrieveDataListRequest updateDicomDataRequest = new RetrieveDataListRequest();
            updateDicomDataRequest.Sender = "PC Client";

            updateDicomDataRequest.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            updateDicomDataRequest.UserId = SelectedUser;

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
                delegate (RetrieveDataListResponse response)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        dataGridOrderList.DataContext = response.DataList;
                        dataGridOrderList.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
                    }));
                    Thread.Sleep(1000 * 60);
                    GetOrders(updateDicomDataRequest);
                },
                failureCallback:
                delegate (string failureReason)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        MessageBox.Show("获取订单列表失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }));
                },
                errorCallback:
                delegate (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        Util.ShowExceptionMessage(ex, "获取订单列表出错。");
                    }));
                });

        }

        private void btn下载行医执照_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnOrder待上传_Click(object sender, RoutedEventArgs e)
        {
            string newStatus = "待上传";
            UpdateOrderStatus(newStatus, string.Empty);
        }

        private void UpdateOrderStatus(string newStatus, string additionInfo)
        {
            DataListItem[] orderList = (DataListItem[])dataGridOrderList.DataContext;
            DataListItem selectedOrder = orderList[dataGridOrderList.SelectedIndex];

            UpdateDataRequest request = new UpdateDataRequest();
            request.AcquisitionDate = selectedOrder.AcquisitionDate;
            request.AcquisitionTime = selectedOrder.AcquisitionTime;
            request.AdditionalData = selectedOrder.AdditionalData;
            request.DataID = selectedOrder.DataID;
            //request.FileName = selectedOrder.
            request.InstitutionName = selectedOrder.InstitutionName;
            request.PatientAge = selectedOrder.PatientAge;
            request.PatientName = selectedOrder.PatientName;
            request.PatientSex = selectedOrder.PatientSex;
            request.SeriesInstanceUID = selectedOrder.SeriesInstanceUID;
            request.StudyInstanceUID = selectedOrder.StudyInstanceUID;
            request.AdditionalData = request.AdditionalData + Environment.NewLine + string.Format("{0} {1}{2}", DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"), newStatus, additionInfo);
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            //request.UploadTimestamp = selectedOrder.up
            request.Status = newStatus;

            ThreadPool.QueueUserWorkItem(delegate
            {
                UpdateDataWorker.SendUpdateDataRequest(
                    request,
                    successCallback: delegate (GeneralWebAPIResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("修改订单状态失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "修改订单状态出错。");
                        }));
                    });
            });
        }

        private void btnOrder已上传_Click(object sender, RoutedEventArgs e)
        {
            string newStatus = "已上传";
            UpdateOrderStatus(newStatus, string.Empty);
        }

        private void btnOrder核验成功_Click(object sender, RoutedEventArgs e)
        {
            string newStatus = "核验成功";
            UpdateOrderStatus(newStatus, string.Empty);
        }

        private void btnOrder核验失败_Click(object sender, RoutedEventArgs e)
        {
            string newStatus = "核验失败";
            UpdateOrderStatus(newStatus, ": " + tb核验失败原因.Text);
        }

        private void btnOrder处理完成_Click(object sender, RoutedEventArgs e)
        {
            string newStatus = "处理完成";
            UpdateOrderStatus(newStatus, string.Empty);
        }

        private void btn获取审核状态_Click(object sender, RoutedEventArgs e)
        {
            string userId = SelectedUser;
            ThreadPool.QueueUserWorkItem(delegate
            {
                GetCertificateApproveStatusWorker.SendGetCertificateApproveStatusRequeset(
                    userId,
                    successCallback: delegate (GetCertificateApproveStatusResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show(response.Result + "," + response.RejectReason);
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("获取医师资格证审核状态失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "获取医师资格证审核状态出错。");
                        }));
                    });
            });
        }

        private void btn审核通过_Click(object sender, RoutedEventArgs e)
        {
            string rejectReason = tbRejectReason.Text;
            UpdateCertificateApproveStatus(true, null);
            NotificationsWorker.SendAddNotification(SelectedUser, "恭喜您，您的医生资格证通过审核。");
        }

        private void UpdateCertificateApproveStatus(bool result, string rejectReason)
        {
            UpdateCertificateApproveStatusRequest request = new UpdateCertificateApproveStatusRequest();
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.UserId = SelectedUser;
            request.Sender = "PC Client";
            request.Result = result;
            request.RejectReason = rejectReason;

            ThreadPool.QueueUserWorkItem(delegate
            {
                UpdateCertificateApproveStatusWorker.SendUpdateCertificateApproveStatusRequeset(
                    request.UserId,
                    request.Result,
                    request.RejectReason,
                    successCallback: delegate (UpdateCertificateApproveStatusResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("审核医师资格证失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "审核医师资格证出错。");
                        }));
                    });
            });
        }

        private void btn审核不通过_Click(object sender, RoutedEventArgs e)
        {
            string rejectReason = tbRejectReason.Text;
            UpdateCertificateApproveStatus(false, rejectReason);
            NotificationsWorker.SendAddNotification(SelectedUser, "很遗憾，您的医生资格证没有通过审核。原因：" + rejectReason);
        }

        private void btn删除订单_Click(object sender, RoutedEventArgs e)
        {
            DataListItem[] orderList = (DataListItem[])dataGridOrderList.DataContext;
            DataListItem selectedOrder = orderList[dataGridOrderList.SelectedIndex];

            ThreadPool.QueueUserWorkItem(delegate
            {
                CancelOrderWorker.SendCancelOrderRequeset(
                    selectedOrder.DataID,
                    successCallback: delegate (CancelOrderResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("取消订单成功。DataId : " + selectedOrder.DataID, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("取消订单失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "取消订单出错。");
                        }));
                    });
            });
        }

        private void btn删除全部订单_Click(object sender, RoutedEventArgs e)
        {
            string userId = SelectedUser;

            ThreadPool.QueueUserWorkItem(delegate
            {
                CancelOrdersWorker.SendCancelOrdersRequeset(
                    userId,
                    successCallback: delegate (CancelOrdersResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("取消用户所有订单成功。UserId : " + userId, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("取消用户所有订单失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "取消用户所有订单出错。");
                        }));
                    });
            });
        }

        private void btn获取订单文件列表_Click(object sender, RoutedEventArgs e)
        {
            DownloadFile(云数据类型Enum.Dicom数据压缩包, dataGridFileList);
        }

        private void DownloadFile(云数据类型Enum 云数据类型, DataGrid gridFileList)
        {
            DataListItem[] orderList = (DataListItem[]) dataGridOrderList.DataContext;
            DataListItem selectedOrder = orderList[dataGridOrderList.SelectedIndex];

            MESPDownloadUpload.UserId = SelectedUser;
            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = selectedOrder.DataID;

            ThreadPool.QueueUserWorkItem(delegate
            {
                new MESPDownloadUpload().FetchFileListAsync(
                    云数据类型,
                    new EventHandler<ExceptionArgs>(
                        delegate(Object senderInner, ExceptionArgs eInner) { MessageBox.Show(eInner.Exception.Message); }),
                    new EventHandler<FileListFinishedArgs>(delegate(Object senderInner, FileListFinishedArgs eInner)
                    {
                        Dispatcher.BeginInvoke(new Action(delegate()
                        {
                            gridFileList.DataContext = eInner.Result;
                            gridFileList.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty,
                                new System.Windows.Data.Binding());
                        }));
                    }));
            });
        }

        private void btn上传文件_Click(object sender, RoutedEventArgs e)
        {
            Uploadfile(云数据类型Enum.Dicom数据压缩包);
        }

        private void Uploadfile(云数据类型Enum dicom数据压缩包)
        {
            DataListItem[] orderList = (DataListItem[]) dataGridOrderList.DataContext;
            DataListItem selectedOrder = orderList[dataGridOrderList.SelectedIndex];

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            string vrAviFilePath =
                new FileInfo(Path.Combine(Path.Combine(SelectedUser, selectedOrder.DataID), "vr.avi")).FullName;
            openFileDialog.FileName = vrAviFilePath;

            if (openFileDialog.ShowDialog() == true)
            {
                MESPDownloadUpload.UserId = LungCare.SupportPlatform.Security.SessionManager.UserName;
                MESPDownloadUpload.OrderId = selectedOrder.DataID;
                MESPDownloadUpload.OrderNo = selectedOrder.DataID;

                ThreadPool.QueueUserWorkItem(delegate
                {
                    new UploadFileWorker().UploadFile(
                        dicom数据压缩包,
                        filename: openFileDialog.FileName,
                        isResume: false,
                        successCallback:
                            delegate() { this.Dispatcher.BeginInvoke(new Action(delegate() { MessageBox.Show("SUCCESS"); })); },
                        failureCallback: delegate(string errMsg) { MessageBox.Show(errMsg); },
                        errorCallback: delegate(Exception ex) { Util.ShowExceptionMessage(ex); },
                        uploadProgressCallback:
                            delegate(ProgressArgs eProgress) { this.Dispatcher.BeginInvoke(new Action(delegate() { })); }
                        );
                });
            }
        }

        public string SelectedDicomPackageLocalPath
        {
            get
            {
                DataListItem[] orderList = (DataListItem[])dataGridOrderList.DataContext;
                DataListItem selectedOrder = orderList[dataGridOrderList.SelectedIndex];

                List<FileListItem> fileList = (List<FileListItem>)dataGridFileList.DataContext;
                FileListItem selectedFile = fileList[dataGridFileList.SelectedIndex];

                return new FileInfo(Path.Combine(Path.Combine(SelectedUser, selectedOrder.DataID), selectedFile.FileName)).FullName;
            }
        }

        public string SelectedVRAviLocalPath
        {
            get
            {
                DataListItem[] orderList = (DataListItem[])dataGridOrderList.DataContext;
                DataListItem selectedOrder = orderList[dataGridOrderList.SelectedIndex];

                List<FileListItem> fileList = (List<FileListItem>)dataGridFileList.DataContext;
                FileListItem selectedFile = fileList[dataGridFileList.SelectedIndex];

                return new FileInfo(Path.Combine(Path.Combine(SelectedUser, selectedOrder.DataID), "vr.avi")).FullName;
            }
        }

        private void btn下载文件_Click_1(object sender, RoutedEventArgs e)
        {
            DownloadFile(dataGridFileList, 云数据类型Enum.Dicom数据压缩包);
        }

        private void DownloadFile(DataGrid gridFileList, 云数据类型Enum 云数据类型Enum)
        {
            DataListItem[] orderList = (DataListItem[]) dataGridOrderList.DataContext;
            DataListItem selectedOrder = orderList[dataGridOrderList.SelectedIndex];

            List<FileListItem> fileList = (List<FileListItem>) gridFileList.DataContext;
            FileListItem selectedFile = fileList[gridFileList.SelectedIndex];

            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = selectedOrder.DataID;
            
            string selectedDicomPackageLocalPath = 
                new FileInfo(Path.Combine(Path.Combine(SelectedUser, selectedOrder.DataID),
                selectedOrder.PatientName + "-" + selectedOrder.PatientAge + "-" + selectedOrder.PatientSex + "-" + selectedOrder.InstitutionName + "-" + selectedOrder.AcquisitionDate + "-" + selectedFile.FileName)).FullName;

            ThreadPool.QueueUserWorkItem(delegate { Download(selectedFile, selectedDicomPackageLocalPath, 云数据类型Enum); });
        }

        private void Download(FileListItem selectedFile, string selectedDicomPackageLocalPath, 云数据类型Enum 云数据类型Enum)
        {
            DownloadFileWorker.Download(
                selectedFile,
                selectedDicomPackageLocalPath,
                delegate (string filename)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        System.Diagnostics.Process.Start(selectedDicomPackageLocalPath);
                    });
                },
                delegate (string errMsg)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        MessageBox.Show(errMsg);
                    });
                },
                delegate (Exception ex)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        //Util.ShowExceptionMessage(ex);
                        Download(selectedFile, selectedDicomPackageLocalPath, 云数据类型Enum.处理结果);
                    });
                },
                delegate (ProgressArgs progressArgs)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        Console.WriteLine(progressArgs);
                    });
                }, 云数据类型Enum);
        }

        private void btnCompleteTaskUpload_Click(object sender, RoutedEventArgs e)
        {
            DataListItem[] orderList = (DataListItem[])dataGridOrderList.DataContext;
            DataListItem selectedOrder = orderList[dataGridOrderList.SelectedIndex];

            ThreadPool.QueueUserWorkItem(delegate
            {
                CompleteTaskWorker.SendCompleteTaskRequeset(
                    selectedOrder.DataID,
                    "Upload",
                    delegate (CompleteTaskResponse response)
                    {
                        UIUtil.Invoke(this, delegate
                        {
                            MessageBox.Show("Send CompleteTaskUpload:Upload succeed.");
                        });
                    },
                    delegate (string errMsg)
                    {
                        UIUtil.Invoke(this, delegate
                        {
                            MessageBox.Show(errMsg);
                        });
                    },
                    delegate (Exception ex)
                    {
                        UIUtil.Invoke(this, delegate
                        {
                        });
                    }
                );
            });
        }

        private void btnCompleteTaskAnalyse_Click(object sender, RoutedEventArgs e)
        {
            DataListItem[] orderList = (DataListItem[])dataGridOrderList.DataContext;
            DataListItem selectedOrder = orderList[dataGridOrderList.SelectedIndex];

            ThreadPool.QueueUserWorkItem(delegate
            {
                CompleteTaskWorker.SendCompleteTaskRequeset(
                    selectedOrder.DataID,
                    "Analyse",
                    delegate (CompleteTaskResponse response)
                    {
                        UIUtil.Invoke(this, delegate
                        {
                            MessageBox.Show("Send CompleteTaskUpload:Upload succeed.");
                        });
                    },
                    delegate (string errMsg)
                    {
                        UIUtil.Invoke(this, delegate
                        {
                            MessageBox.Show(errMsg);
                        });
                    },
                    delegate (Exception ex)
                    {
                        UIUtil.Invoke(this, delegate
                        {
                        });
                    }
                );
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            btnLogin_Click(sender, e);
        }

        private void btn删除用户_Click(object sender, RoutedEventArgs e)
        {
            DeleteUserWorker.SendDeleteUserRequest(SelectedUser,
                delegate { },
                delegate { },
                delegate { });
        }


        private void btn自动生成虚拟内窥视频_Click(object sender, RoutedEventArgs e)
        {
            string zipFileName = SelectedDicomPackageLocalPath;
            string tmpFolder = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMdd-HHmmss"));

            string haozipExe = @"HaoZipC.exe";
            //string haozipExe = @"C:\Program Files\2345Soft\HaoZip\HaoZipC.exe";
            string args = string.Format("x \"{0}\" -o\"{1}\"", zipFileName, tmpFolder);

            ProcessStartInfo psi = new ProcessStartInfo(haozipExe, args);
            //psi.CreateNoWindow = true;
            //psi.WindowStyle = ProcessWindowStyle.Hidden;

            Process process = Process.Start(psi);
            process.WaitForExit();
            Console.WriteLine("ExitCode: " + process.ExitCode);

            string tmpImgFolder = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(tmpImgFolder);

            string[] files = Directory.GetFiles(tmpFolder, "*.*", SearchOption.AllDirectories);
            
            int counter = 0;
            foreach (var item in files)
            {
                Console.WriteLine(item);
                try
                {
                    Dicom.Imaging.DicomImage dcmImg = new Dicom.Imaging.DicomImage(item);
                    System.Drawing.Image img = dcmImg.RenderImage();

                    System.Drawing.Image resized = new System.Drawing.Bitmap(512, 512);
                    System.Drawing.Graphics.FromImage(resized).DrawImage(img, System.Drawing.Point.Empty);

                    resized.Save(Path.Combine(tmpImgFolder, (++counter) + ".jpg"));

                    resized.Dispose();

                    img.Dispose();
                    img = null;
                }
                catch
                {
                }
            }

            Process.Start(tmpImgFolder);

            //string tmpFolder = @"C:\Users\zlf\AppData\Local\Temp\20151027-170929";

            //DataListItem[] orderList = (DataListItem[])dataGridOrderList.DataContext;
            //DataListItem selectedOrder = orderList[dataGridOrderList.SelectedIndex];
            //string vrAviFilePath = Path.Combine(Path.Combine(SelectedUser, selectedOrder.DataID), "vr.avi");

            string img2aviArgs = string.Format("\"{0}\" \"{1}\"", tmpImgFolder, SelectedVRAviLocalPath);
            Console.WriteLine(img2aviArgs);
            Process img2AviProcess = Process.Start(@"I:\zlfLab\虚拟导航平台\navigate lx 20151026 2130 lx\LungCare.Airway.PlanNav\bin\Release\img2avi\img2avi.exe", img2aviArgs);
            img2AviProcess.WaitForExit();
            Console.WriteLine("ExitCode: " + img2AviProcess.ExitCode);

            try
            {
                Directory.Delete(tmpImgFolder, true);
            }
            catch
            {
            }
        }

        private void btn删除头像_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.UploadUserIconWorker.SendUserIconRequest(
                    null,
                    successCallback: delegate (LungCare.SupportPlatform.Models.UploadUserIconResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("上传头像成功!");
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
                            LungCare.SupportPlatform.WebAPIWorkers.Util.ShowExceptionMessage(ex, "上传头像出错。");
                        }));
                    });
            });

        }

        private void btn获取待审核用户_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                GetCertiWaitingApprovedUserNamesWorker.SendGetCertiWaitingApprovedUserNamesRequest(
                    successCallback: delegate (GetCertiWaitingApprovedUserNamesResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            dataGridUserList.DataContext = response.UserNameList.Select(item => new { UserId = item });
                            dataGridUserList.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
                        }));
                    },
                    failureCallback: delegate (string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("获取用户列表。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback: delegate (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "获取用户列表出错。");
                        }));
                    }
                );
            });
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool Beep(int freq, int dur);

        private void btn获取有待处理订单的用户_Click(object sender, RoutedEventArgs e)
        {
            tbUsersHaveOrder.Text = string.Empty;

            foreach (var username in _UserNameList)
            {
                RetrieveDataListRequest updateDicomDataRequest = new RetrieveDataListRequest();
                updateDicomDataRequest.Sender = "PC Client";

                updateDicomDataRequest.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
                updateDicomDataRequest.UserId = username;

                ThreadPool.QueueUserWorkItem(delegate
                {
                    RetrieveDataListWorker.SendRetrieveDataListRequest(
                        updateDicomDataRequest,
                        successCallback:
                        delegate (RetrieveDataListResponse response)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                DataListItem[] list已上传 = response.DataList.Where(item => item.Status == "已上传").ToArray();
                                foreach (var item in list已上传)
                                {
                                    string[] exceptUserNames = new string[] { "13501299816","18625272916", "18552024921", "15261595318" };
                                    if (!exceptUserNames.Contains(username))
                                    {
                                        Beep(440, 1000); // Concert A, for 1 second
                                        tbUsersHaveOrder.Text += username + Environment.NewLine;
                                        
                                        //MessageBox.Show(username);
                                        Console.WriteLine(username);
                                    }
                                    return;
                                }
                                //dataGridOrderList.DataContext = response.DataList;
                                //dataGridOrderList.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());
                            }));
                        },
                        failureCallback:
                        delegate (string failureReason)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                MessageBox.Show("获取订单列表失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            }));
                        },
                        errorCallback:
                        delegate (Exception ex)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                Util.ShowExceptionMessage(ex, "获取订单列表出错。");
                            }));
                        });
                });
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

        private void btn获取用户信息_Click(object sender, RoutedEventArgs e)
        {
            LungCare.SupportPlatform.Security.SessionManager.UserName = SelectedUser;

            ThreadPool.QueueUserWorkItem(delegate
            {
                LungCare.SupportPlatform.WebAPIWorkers.RetrieveUserInfoWorker.SendUserInfoRequest(
                    successCallback: delegate (LungCare.SupportPlatform.Models.GetUserInfoResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            imageUserIcon.Source = BitmapSourceFromBase64(response.CertificateImage);
                            MessageBox.Show(response.ChineseName);
                            //response.CertificateImage
                            //_userInfo = response;
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

        private void btn获取订单文件列表_处理结果_Click(object sender, RoutedEventArgs e)
        {
            DownloadFile(云数据类型Enum.处理结果, dataGridFileList处理结果);
        }

        private void btn下载文件_处理结果_Click(object sender, RoutedEventArgs e)
        {
            DownloadFile(dataGridFileList处理结果, 云数据类型Enum.处理结果);
        }

        private void btn上传文件_处理结果_Click(object sender, RoutedEventArgs e)
        {
            Uploadfile(云数据类型Enum.处理结果);
        }
    }
}

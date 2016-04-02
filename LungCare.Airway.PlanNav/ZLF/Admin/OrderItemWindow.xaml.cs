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
using System.Diagnostics;
using LungCare.SupportPlatform.UI.Windows.Common;

namespace LungCare_Airway_PlanNav
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class OrderItemWindow : Window
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

        //private DispatcherTimer timer;
        public OrderItemWindow()
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

            //ListBoxItem item1 = new ListBoxItem();
            //item1.Content = "我的是啊大叔.zip";
            //listboxUserFiles.Items.Add(item1);
            
            //ListBoxItem item = new ListBoxItem();
            //item.Content = "我的大叔.zip";
            //listboxUserFiles.Items.Add(item);

            //ListBoxItem item2 = new ListBoxItem();
            //item2.Content = "我的ssss.zip";
            //listboxUserFiles.Items.Add(item2);


            //ListBoxItem item3 = new ListBoxItem();
            //item3.Content = "我的是啊大叔.zip";
            //listboxAdminFiles.Items.Add(item3);

            //ListBoxItem item4 = new ListBoxItem();
            //item4.Content = "我的大叔.zip";
            //listboxAdminFiles.Items.Add(item4);

            //ListBoxItem item5 = new ListBoxItem();
            //item5.Content = "我的ssss.zip";
            //listboxAdminFiles.Items.Add(item5);
            headBorder.MouseDown+=headBorder_MouseDown;
            _UserID = "15121562612";
            _OrderID = _OrderNO = "10000065";
            _selectedOrder = new DataListItem();
            _selectedOrder.DataID = "10000065";
            _selectedOrder.Status = "已上传";
            _selectedOrder.UploadTimestamp = "20151101 103802";
            _selectedOrder.PatientName = "WANG YI JIE";
            _selectedOrder.PatientSex = "M";
            _selectedOrder.PatientAge = "064Y";
            _selectedOrder.InstitutionName = "Shanghai Chest Hospital";
            _selectedOrder.AcquisitionDate = "20151102";
            List<DataListItem> list = new List<DataListItem>();
            list.Add(_selectedOrder);
            listViewUser.DataContext =
                new System.ComponentModel.BindingList<LungCare.SupportPlatform.Models.DataListItem>(list);
            //listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
            listViewUser.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());

            GetUserFiles();
            GetAdminFiles();

            switch (_selectedOrder.Status)
            {
                case"待上传":
                    rbWaitingUp.IsChecked = true;
                    return;
                case "已上传":
                    rbUploaded.IsChecked = true;
                    return;
                case "核验失败":
                    rbCheckFail.IsChecked = true;
                    return;
                case "核验成功":
                    rbCheckSuccess.IsChecked = true;
                    return;
                case "处理完成":
                    rbCompleted.IsChecked = true;
                    return;
                default:
                    break;
            }
            
        }
      
        private string _UserID, _OrderID, _OrderNO;
        private DataListItem _selectedOrder;
        public OrderItemWindow(string userid ,string orderid , string orderNo)
        {
            this.InitializeComponent();
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;// 获得窗体的 样式

            int oldstyle = MedSys.PresentationCore.AdjustWindow.NativeMethods.GetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE, oldstyle & ~MedSys.PresentationCore.AdjustWindow.NativeMethods.WS_CAPTION);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetLayeredWindowAttributes(hwnd, 1 | 2 << 8 | 3 << 16, 0, MedSys.PresentationCore.AdjustWindow.NativeMethods.LWA_ALPHA);
            headBorder.MouseDown += headBorder_MouseDown;
            this.RefreshRoundWindowRect();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.SizeChanged += MainWindow_SizeChanged;

            _UserID = userid;
            _OrderID = orderid;
            _OrderNO = orderNo;

            
            GetUserFiles();
        }

        public OrderItemWindow(DataListItem dataListItem)
        {
            this.InitializeComponent();
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;// 获得窗体的 样式

            int oldstyle = MedSys.PresentationCore.AdjustWindow.NativeMethods.GetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetWindowLong(hwnd, MedSys.PresentationCore.AdjustWindow.NativeMethods.GWL_STYLE, oldstyle & ~MedSys.PresentationCore.AdjustWindow.NativeMethods.WS_CAPTION);
            MedSys.PresentationCore.AdjustWindow.NativeMethods.SetLayeredWindowAttributes(hwnd, 1 | 2 << 8 | 3 << 16, 0, MedSys.PresentationCore.AdjustWindow.NativeMethods.LWA_ALPHA);
            headBorder.MouseDown += headBorder_MouseDown;
            this.RefreshRoundWindowRect();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.SizeChanged += MainWindow_SizeChanged;

            _UserID = dataListItem.UserId;
            _OrderID = dataListItem.DataID;
            _OrderNO = dataListItem.DataID;

            _selectedOrder = dataListItem;
            List<DataListItem> list = new List<DataListItem>();
            list.Add(_selectedOrder);
            listViewUser.DataContext =
                new System.ComponentModel.BindingList<LungCare.SupportPlatform.Models.DataListItem>(list);
            //listView.DataContext = new System.ComponentModel.BindingList<Models.DataListItem>(dataSource);
            listViewUser.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, new System.Windows.Data.Binding());

            GetUserFiles();
            GetAdminFiles();

            switch (_selectedOrder.Status)
            {
                case "待上传":
                    rbWaitingUp.IsChecked = true;
                    return;
                case "已上传":
                    rbUploaded.IsChecked = true;
                    return;
                case "核验失败":
                    rbCheckFail.IsChecked = true;
                    return;
                case "核验成功":
                    rbCheckSuccess.IsChecked = true;
                    return;
                case "处理完成":
                    rbCompleted.IsChecked = true;
                    return;
                default:
                    break;
            }
        }

        private List<FileListItem> _UserFileList;
        private void GetUserFiles()
        {
            MESPDownloadUpload.UserId = _UserID;
            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = _OrderNO;

            ThreadPool.QueueUserWorkItem(delegate
            {
                new MESPDownloadUpload().FetchFileListAsync(
                    云数据类型Enum.Dicom数据压缩包,
                    new EventHandler<ExceptionArgs>(
                        delegate(Object senderInner, ExceptionArgs eInner) { MessageBox.Show(eInner.Exception.Message); }),
                    new EventHandler<FileListFinishedArgs>(delegate(Object senderInner, FileListFinishedArgs eInner)
                    {
                        Dispatcher.BeginInvoke(new Action(delegate()
                        {
                            _UserFileList = eInner.Result;
                            foreach (var item in eInner.Result)
                            {
                                ListBoxItem fileItem = new ListBoxItem();
                                fileItem.Content = item.FileName;
                                listboxUserFiles.Items.Add(fileItem);
                            }
                        }));
                    }));
            });
        }

        private List<FileListItem> _adminFileList;
        private void GetAdminFiles()
        {

            MESPDownloadUpload.UserId = _UserID;
            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = _OrderNO;

            ThreadPool.QueueUserWorkItem(delegate
            {
                new MESPDownloadUpload().FetchFileListAsync(
                    云数据类型Enum.处理结果,
                    new EventHandler<ExceptionArgs>(
                        delegate(Object senderInner, ExceptionArgs eInner) { 
                            //MessageBox.Show("未找到文件!");
                        }),
                    new EventHandler<FileListFinishedArgs>(delegate(Object senderInner, FileListFinishedArgs eInner)
                    {
                        Dispatcher.BeginInvoke(new Action(delegate()
                        {
                            _adminFileList = eInner.Result;
                            foreach (var item in eInner.Result)
                            {
                                ListBoxItem fileItem = new ListBoxItem();
                                fileItem.Content = item.FileName;
                                listboxAdminFiles.Items.Add(fileItem);
                            }
                        }));
                    }));
            });
        }


        private void UploadAdminFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();


            if (openFileDialog.ShowDialog() == true)
            {
                MESPDownloadUpload.UserId = _UserID;
                MESPDownloadUpload.OrderId = _OrderNO;
                MESPDownloadUpload.OrderNo = _OrderNO;
                PauseGUI();
                ThreadPool.QueueUserWorkItem(delegate
                {
                    new UploadFileWorker().UploadFile(
                        云数据类型Enum.处理结果,
                        filename: openFileDialog.FileName,
                        isResume: false,
                        successCallback:
                            delegate() { this.Dispatcher.BeginInvoke(new Action(delegate() {
                                //ListBoxItem fileItem = new ListBoxItem();
                                //fileItem.Content = openFileDialog.SafeFileName;
                                //listboxAdminFiles.Items.Add(fileItem);
                                listboxAdminFiles.Items.Clear();
                                GetAdminFiles();
                                ResumeGUI();
                                MessageBox.Show("SUCCESS"); 
                            })); },
                        failureCallback: delegate(string errMsg) {
                            ResumeGUI();
                            MessageBox.Show(errMsg);
                        },
                        errorCallback: delegate(Exception ex) {
                            ResumeGUI();
                            Util.ShowExceptionMessage(ex); 
                        },
                        uploadProgressCallback:
                            delegate(ProgressArgs eProgress) { this.Dispatcher.BeginInvoke(new Action(delegate() { })); }
                        );
                });
            }
        }


        private void Download(FileListItem selectedFile, string selectedDicomPackageLocalPath, 云数据类型Enum 云数据类型Enum)
        {
            
            DownloadFileWorker.Download(
                selectedFile,
                selectedDicomPackageLocalPath,
                delegate(string filename)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        try {
                            string tempPath = selectedDicomPackageLocalPath.Substring(0,selectedDicomPackageLocalPath.LastIndexOf(Path.DirectorySeparatorChar));
                            System.Diagnostics.Process.Start(tempPath);
                            ResumeGUI();
                        }
                        catch(Exception ex){
                            MessageBox.Show(ex.ToString());
                        }
                    });
                },
                delegate(string errMsg)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        MessageBox.Show(errMsg);
                        ResumeGUI();
                    });
                },
                delegate(Exception ex)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        Util.ShowExceptionMessage(ex);
                        //Download(selectedFile, selectedDicomPackageLocalPath, 云数据类型Enum.处理结果);
                        ResumeGUI();
                    });
                },
                delegate(ProgressArgs progressArgs)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        Console.WriteLine(progressArgs);
                    });
                }, 云数据类型Enum);
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


        private void PauseGUI()
        {
            Dispatcher.Invoke(new Action(delegate { 
                ((UIElement)Content).IsEnabled = false;
                Cursor = Cursors.Wait;
            }));
        }

        private void ResumeGUI()
        {
            Dispatcher.Invoke(new Action(delegate { 
                ((UIElement)Content).IsEnabled = true;
                Cursor = Cursors.Arrow;
            }));
        }

        private void btnDownloadUserFile_Click(object sender, RoutedEventArgs e)
        {
            if (listboxUserFiles.SelectedIndex < 0)
            {
                MessageBox.Show("请先选中文件进行下载");
                return;
            }
            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = _OrderNO;
            FileListItem selectedFile = _UserFileList[listboxUserFiles.SelectedIndex];
            string filename = Path.Combine(Path.Combine(_UserID, _OrderNO),
                _selectedOrder.PatientName + "-" + _selectedOrder.PatientAge + "-" + _selectedOrder.PatientSex + "-" + _selectedOrder.InstitutionName + "-" + _selectedOrder.AcquisitionDate + "-" + selectedFile.FileName);
           

            string selectedDicomPackageLocalPath ="";
            try
            {
                selectedDicomPackageLocalPath =
                new FileInfo(filename).FullName;
            }
            catch (Exception)
            {
                filename = Path.Combine(Path.Combine(_UserID, _OrderNO), selectedFile.FileName);
                selectedDicomPackageLocalPath =
               new FileInfo(filename).FullName;
            }
            PauseGUI();
            ThreadPool.QueueUserWorkItem(delegate { Download(selectedFile, selectedDicomPackageLocalPath, 云数据类型Enum.Dicom数据压缩包); });
        }

        private void btnUploadAdminFile_Click(object sender, RoutedEventArgs e)
        {
            UploadAdminFile();

        }

        private void btnDownloadAdminFile_Click(object sender, RoutedEventArgs e)
        {
            if (listboxAdminFiles.SelectedIndex < 0)
            {
                MessageBox.Show("请先选中文件进行下载");
                return;
            }
            FileListItem selectedFile = _adminFileList[listboxAdminFiles.SelectedIndex];

            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = _selectedOrder.DataID;

            string filename = Path.Combine(Path.Combine(_UserID, _OrderNO),
                _selectedOrder.PatientName + "-" + _selectedOrder.PatientAge + "-" + _selectedOrder.PatientSex + "-" + _selectedOrder.InstitutionName + "-" + _selectedOrder.AcquisitionDate + "-" + selectedFile.FileName);
           

            string selectedDicomPackageLocalPath = "";
            try
            {
                selectedDicomPackageLocalPath =
                new FileInfo(filename).FullName;
            }
            catch (Exception)
            {
                filename = Path.Combine(Path.Combine(_UserID, _OrderNO), selectedFile.FileName);
                selectedDicomPackageLocalPath =
               new FileInfo(filename).FullName;
            }
            
            PauseGUI();
            ThreadPool.QueueUserWorkItem(delegate { 
                Download(selectedFile, selectedDicomPackageLocalPath, 云数据类型Enum.处理结果); 
            });
        }

        private void btnDeleteAdminFile_Click(object sender, RoutedEventArgs e)
        {
            if (listboxAdminFiles.SelectedIndex < 0 || _adminFileList == null)
            {
                return;
            }
            FileListItem selectedFile = _adminFileList[listboxAdminFiles.SelectedIndex];
            if (selectedFile == null)
            {
                return;
            }
            if (System.Windows.Forms.MessageBox.Show("确定删除吗？", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    DeleteOrderDealFileWorker.SendDeleteOrderDealFileRequeset(
                        selectedFile.OrderId,
                        selectedFile.FileName,
                        successCallback: delegate(DeleteOrderDealFileResponse response)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                ResumeGUI();
                                listboxAdminFiles.Items.Clear();
                                GetAdminFiles();
                            }));
                        },
                        failureCallback: delegate(string failureReason)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                ResumeGUI();
                                MessageBox.Show("删除文件失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            }));
                        },
                        errorCallback: delegate(Exception ex)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                Util.ShowExceptionMessage(ex, "删除文件出错。");
                                ResumeGUI();
                            }));
                        });
                });
            }
        }
        public event EventHandler<RoutedEventArgs> updateHandler;
        private string newStatus = "已上传";
        private void btnCompetedFile_Click(object sender, RoutedEventArgs e)
        {
            UpdateOrderStatus(newStatus, string.Empty);
            if(updateHandler!=null){
                updateHandler(sender,e);
            }
        }

        private void UpdateOrderStatus(string newStatus, string additionInfo)
        {

            UpdateDataRequest request = new UpdateDataRequest();
            request.AcquisitionDate = _selectedOrder.AcquisitionDate;
            request.AcquisitionTime = _selectedOrder.AcquisitionTime;
            request.AdditionalData = _selectedOrder.AdditionalData;
            request.DataID = _selectedOrder.DataID;
            //request.FileName = selectedOrder.
            request.InstitutionName = _selectedOrder.InstitutionName;
            request.PatientAge = _selectedOrder.PatientAge;
            request.PatientName = _selectedOrder.PatientName;
            request.PatientSex = _selectedOrder.PatientSex;
            request.SeriesInstanceUID = _selectedOrder.SeriesInstanceUID;
            request.StudyInstanceUID = _selectedOrder.StudyInstanceUID;
            request.AdditionalData = request.AdditionalData + Environment.NewLine + string.Format("{0} {1}{2}", DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"), newStatus, additionInfo);
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            //request.UploadTimestamp = selectedOrder.up
            request.Status = newStatus;
            PauseGUI();
            ThreadPool.QueueUserWorkItem(delegate
            {
                UpdateDataWorker.SendUpdateDataRequest(
                    request,
                    successCallback: delegate(GeneralWebAPIResponse response)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                        }));
                    },
                    failureCallback: delegate(string failureReason)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ResumeGUI();
                            MessageBox.Show("修改订单状态失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }));
                    },
                    errorCallback: delegate(Exception ex)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(ex, "修改订单状态出错。");
                            ResumeGUI();
                        }));
                    });
            });
        }
        private void btnCancelFile_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnUploadUserFile_Click(object sender, RoutedEventArgs e)
        {
            Uploadfile(云数据类型Enum.Dicom数据压缩包);
        }


        private void Uploadfile(云数据类型Enum dicom数据压缩包)
        {

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            string vrAviFilePath =
                new FileInfo(Path.Combine(Path.Combine(_UserID, _selectedOrder.DataID), "vr.avi")).FullName;
            openFileDialog.FileName = vrAviFilePath;

            if (openFileDialog.ShowDialog() == true)
            {
                PauseGUI();
                MESPDownloadUpload.UserId = LungCare.SupportPlatform.Security.SessionManager.UserName;
                MESPDownloadUpload.OrderId = _selectedOrder.DataID;
                MESPDownloadUpload.OrderNo = _selectedOrder.DataID;

                ThreadPool.QueueUserWorkItem(delegate
                {
                    new UploadFileWorker().UploadFile(
                        dicom数据压缩包,
                        filename: openFileDialog.FileName,
                        isResume: false,
                        successCallback:
                            delegate() { 
                                this.Dispatcher.BeginInvoke(new Action(delegate() { 
                                   
                                    //ListBoxItem item = new ListBoxItem();
                                    //item.Content = openFileDialog.SafeFileName;
                                    //listboxUserFiles.Items.Add(item);
                                    listboxUserFiles.Items.Clear();
                                    GetUserFiles();
                                    ResumeGUI();
                                    MessageBox.Show("SUCCESS");
                                })); 
                                
                            },
                        failureCallback: delegate(string errMsg) {
                            MessageBox.Show(errMsg); 
                            ResumeGUI();
                        },
                        errorCallback: delegate(Exception ex) { Util.ShowExceptionMessage(ex); ResumeGUI(); },
                        uploadProgressCallback:
                            delegate(ProgressArgs eProgress) { this.Dispatcher.BeginInvoke(new Action(delegate() { })); }
                        );
                });
            }
        }

        
        private void rbWaitingUp_Checked(object sender, RoutedEventArgs e)
        {
            newStatus = "待上传";
        }

        private void rbUploaded_Checked(object sender, RoutedEventArgs e)
        {
            newStatus = "已上传";
        }

        private void rbCheckSuccess_Checked(object sender, RoutedEventArgs e)
        {
            newStatus = "核验成功";
        }

        private void rbCheckFail_Checked(object sender, RoutedEventArgs e)
        {
            newStatus = "核验失败";
        }

        private void rbCompleted_Checked(object sender, RoutedEventArgs e)
        {
            newStatus = "处理完成";
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void btnDeleteUserFile_Click(object sender, RoutedEventArgs e)
        {
            if (listboxUserFiles.SelectedIndex < 0 || _UserFileList==null )
            {
                return;
            }
            FileListItem selectedFile = _UserFileList[listboxUserFiles.SelectedIndex];
            if (selectedFile == null)
            {
                return;
            }
            if (System.Windows.Forms.MessageBox.Show("确定删除吗？", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    DeleteOrderDicomFileWorker.SendDeleteOrderDicomFileRequeset(
                        selectedFile.OrderId,
                        selectedFile.FileName,
                        successCallback: delegate(DeleteOrderDicomFileResponse response)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                ResumeGUI();
                                listboxUserFiles.Items.Clear();
                                GetUserFiles();
                            }));
                        },
                        failureCallback: delegate(string failureReason)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                ResumeGUI();
                                MessageBox.Show("删除文件失败。" + failureReason, "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            }));
                        },
                        errorCallback: delegate(Exception ex)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                Util.ShowExceptionMessage(ex, "删除文件出错。");
                                ResumeGUI();
                            }));
                        });
                });
            }
        }

    }

    
}
using Dicom;
using Dicom.Imaging;
using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.UI.Windows.Common;
using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace LungCare_Airway_PlanNav.ZLF.Controls
{
    /// <summary>
    /// UploadFileUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class UploadFileUserControl : UserControl
    {
        public UploadFileUserControl()
        {
            InitializeComponent();
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

            return size1.ToString("F1") + units[i];
            return Math.Round(size1) + units[i];
        }

        private void StartParseDicom()
        {
            //this.Dispatcher.BeginInvoke(new Action(delegate ()
            //{
            //    lbDicomInfo.Content = string.Empty;

            //    labelDownloadInfo.Visibility = Visibility.Visible;
            //    labelDownloadInfo.Content = "正在解析dicom信息";
            //    lbUploadError.Visibility = System.Windows.Visibility.Hidden;
            //    uploadProgress.Visibility = System.Windows.Visibility.Hidden;
            //    uploadProgress.Value = 0;
            //    btnUploadFile.Visibility = System.Windows.Visibility.Hidden;
            //    btn续传.Visibility = System.Windows.Visibility.Hidden;
            //}));
        }

        private void DicomOnError()
        {
            // labelDownloadInfo.Visibility =System.Windows.Visibility.Visible;
            // lbUploadError.Visibility =System.Windows.Visibility.Visible;
            // uploadProgress.Visibility =System.Windows.Visibility.Visible;
            // btnUploadFile.Visibility =System.Windows.Visibility.Visible;
            // btn续传.Visibility =System.Windows.Visibility.Visible;
        }

        private void StartUpload()
        {
            //this.Dispatcher.BeginInvoke(new Action(delegate ()
            //{
            //    labelDownloadInfo.Visibility = Visibility.Visible;
            //    labelDownloadInfo.Content = "正在上传";
            //    lbUploadError.Visibility = System.Windows.Visibility.Hidden;
            //    uploadProgress.Visibility = System.Windows.Visibility.Visible;
            //    uploadProgress.Value = 0;
            //    btnUploadFile.Visibility = System.Windows.Visibility.Hidden;
            //    btn续传.Visibility = System.Windows.Visibility.Hidden;
            //}));
        }

        private void FinishUpload()
        {
            //this.Dispatcher.BeginInvoke(new Action(delegate ()
            //{
            //    labelDownloadInfo.Visibility = Visibility.Hidden;
            //    labelDownloadInfo.Content = "上传完毕";
            //    lbUploadError.Visibility = System.Windows.Visibility.Hidden;
            //    uploadProgress.Visibility = System.Windows.Visibility.Hidden;
            //    btnUploadFile.Visibility = System.Windows.Visibility.Visible;
            //    btn续传.Visibility = System.Windows.Visibility.Hidden;
            //}));
        }

        private void UploadErrorOccurred()
        {
            //this.Dispatcher.BeginInvoke(new Action(delegate ()
            //{
            //    labelDownloadInfo.Visibility = Visibility.Hidden;
            //    labelDownloadInfo.Content = "上传出错";
            //    lbUploadError.Visibility = System.Windows.Visibility.Visible;
            //    uploadProgress.Visibility = System.Windows.Visibility.Visible;
            //    btnUploadFile.Visibility = System.Windows.Visibility.Visible;
            //    btn续传.Visibility = System.Windows.Visibility.Visible;
            //}));
        }

        public void Ask4PickFile()
        {
            // labelDownloadInfo
            // lbUploadError
            // uploadProgress
            // btnUploadFile
            // btn续传
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                filename = openFileDialog.FileName;
                string ext = new FileInfo(filename).Extension.ToLower();

                if (ext != ".rar" && ext != ".zip" && ext != ".7z" && ext != ".tar" && ext != ".iso")
                {
                    MessageBox.Show("请选择压缩包。", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (new FileInfo(filename).Length > 500 * 1024 * 1024)
                {
                    MessageBox.Show(string.Format("您选择的压缩包过大，体积为{0}，请选择500M以下的压缩包。", HumanReadableFilesize(new FileInfo(filename).Length)), "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                StartParseDicom();

                MainWindow.Instance.DisableAll();
                Cursor = System.Windows.Input.Cursors.Wait;
                lbDicomError.Visibility = lbParsingDicom.Visibility = Visibility.Hidden;
                lbParsingDicom.Visibility = Visibility.Visible;

                btnUploadFile.IsEnabled = false;

                ThreadPool.QueueUserWorkItem(delegate
                {
                    // haozipc x I:\AirwayCTZip\20150812\甘福明.rar -oi:/temp/20151005
                    //string tmpFolder = System.IO.Path.Combine("i:/temp/" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "_" + Guid.NewGuid().ToString("N"));
                    //string tmpFolder = System.IO.Path.Combine(Path.GetTempPath() + "/temp/" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "_" + Guid.NewGuid().ToString("N"));
                    //string tmpFolder = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMdd-HHmmss"));
                    string tmpFolder = Path.Combine("c:/lctmp/", DateTime.Now.ToString("yyyyMMdd-HHmmss"));

                    string haozipExe = new FileInfo(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"HaoZipC.exe")).FullName;

                    if (!File.Exists(haozipExe))
                    {
                        MessageBox.Show("没有找到" + haozipExe + "。无法解压缩压缩包。");
                        DicomOnError();
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MainWindow.Instance.EnableAll();
                            lbDicomError.Visibility = Visibility.Visible;
                            lbParsingDicom.Visibility = Visibility.Hidden;
                            btnUploadFile.IsEnabled = true;
                            Cursor = System.Windows.Input.Cursors.Arrow;
                        }));
                        return;
                    }

                    //string haozipExe = @"C:\Program Files\2345Soft\HaoZip\HaoZipC.exe";
                    string args = string.Format("x \"{0}\" -o\"{1}\"", filename, tmpFolder);

                    Console.WriteLine(haozipExe);
                    Console.WriteLine(args);

                    ProcessStartInfo psi = new ProcessStartInfo(haozipExe, args);
                    psi.CreateNoWindow = true;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;

                    Process process = Process.Start(psi);
                    process.WaitForExit();
                    Console.WriteLine("ExitCode: " + process.ExitCode);

                    //Process.Start(tmpFolder);

                    string[] files = Directory.GetFiles(tmpFolder, "*.*", SearchOption.AllDirectories);

                    bool foundDicom = false;
                    foreach (var item in files)
                    {
                        Console.WriteLine(item);

                        if (!HasValidHeader(item))
                        {
                            FileInfo fi = new FileInfo(item);
                            if (fi.Extension.ToString() == ".dcm" || (fi.Length > 500000 && fi.Length < 600000))
                            {

                            }
                            else
                            {
                                continue;
                            }
                        }

                        try
                        {
                            Console.WriteLine("dicom " + item);

                            var file = DicomFile.Open(item);
                            string patientName = file.Dataset.Get<string>(DicomTag.PatientName);
                            string patientSex = file.Dataset.Get<string>(DicomTag.PatientSex);
                            string patientAge = file.Dataset.Get<string>(DicomTag.PatientAge);
                            string institutionName = file.Dataset.Get<string>(DicomTag.InstitutionName);
                            string acquisitionDate = file.Dataset.Get<string>(DicomTag.AcquisitionDate);
                            string acquisitionTime = file.Dataset.Get<string>(DicomTag.AcquisitionTime);

                            string studyInstanceUID = file.Dataset.Get<string>(DicomTag.StudyInstanceUID);
                            string seriesInstanceUID = file.Dataset.Get<string>(DicomTag.SeriesInstanceUID);

                            LungCare.SupportPlatform.Models.CTDicomInfo CTDicomInfo = new LungCare.SupportPlatform.Models.CTDicomInfo();
                            CTDicomInfo.PatientName = patientName;
                            CTDicomInfo.PatientSex = patientSex;
                            CTDicomInfo.PatientAge = patientAge;
                            CTDicomInfo.InstitutionName = institutionName;
                            CTDicomInfo.AcquisitionDate = acquisitionDate;
                            CTDicomInfo.AcquisitionTime = acquisitionTime;
                            CTDicomInfo.StudyInstanceUID = studyInstanceUID;
                            CTDicomInfo.SeriesInstanceUID = seriesInstanceUID;

                            CTDicomInfo.FileName = item;
                            //CTDicomInfo.FileName = new FileInfo(_fileName).Name;
                            //CTDicomInfo.UserId = SessionManager.UserName;
                            //CTDicomInfo.Token = SessionManager.UserName;
                            CTDicomInfo.DataID = Guid.NewGuid().ToString("N");
                            CTDicomInfo.UploadTimestamp = DateTime.Now;
                            CTDicomInfo.Status = "已上传";
                            CTDicomInfo.AdditionalData = string.Format("{0} 已上传", DateTime.Now.ToString("yyyyMMdd HHmm"));

                            InstitutionName = institutionName;
                            PatientAge = patientAge;
                            PatientName = patientName;
                            PatientSex = patientSex;
                            SeriesInstanceUID = seriesInstanceUID;
                            StudyInstanceUID = studyInstanceUID;
                            this.acquisitionDate = acquisitionDate;
                            this.acquisitionTime = acquisitionTime;

                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine(PatientName);
                            sb.AppendLine(PatientAge);
                            sb.AppendLine(PatientSex);
                            sb.AppendLine(acquisitionDate + " " + acquisitionTime);

                            //this.Dispatcher.BeginInvoke(new Action(delegate ()
                            //{
                            //    //lbDicomInfo.Content = sb.ToString();
                            //}));

                            Console.WriteLine(patientName);

                            if (!string.IsNullOrEmpty(patientName))
                            {
                                foundDicom = true;
                                break;
                            }
                        }
                        catch (Exception exDicom)
                        {
                            Console.WriteLine(exDicom.Message);
                        }
                    }

                    try
                    {
                        Directory.Delete(tmpFolder, true);
                    }
                    catch (Exception deleteEx)
                    {
                        Console.WriteLine(deleteEx.Message);
                    }

                    if (!foundDicom)
                    {
                        DicomOnError();
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MainWindow.Instance.EnableAll();
                            lbDicomError.Visibility = Visibility.Visible;
                            lbParsingDicom.Visibility = Visibility.Hidden;
                            btnUploadFile.IsEnabled = true;
                            Cursor = System.Windows.Input.Cursors.Arrow;
                        }));
                        return;
                    }

                    GC.Collect();

                    //ThreadPool.QueueUserWorkItem(delegate
                    //{
                    StartUpload();
                    MainWindow.Instance.EnableAll();

                    Dispatcher.Invoke(new Action(delegate
                    {
                        Cursor = System.Windows.Input.Cursors.Arrow;

                        lbDicomError.Visibility = lbParsingDicom.Visibility = Visibility.Hidden;
                        btnUploadFile.IsEnabled = true;

                        LungCare.SupportPlatform.UI.UploadProgressWnd upw = new LungCare.SupportPlatform.UI.UploadProgressWnd();
                        upw.Owner = MainWindow.Instance;

                        upw.FileName = filename;
                        upw.InstitutionName = InstitutionName;
                        upw.PatientAge = PatientAge;
                        upw.PatientName = PatientName;
                        upw.PatientSex = PatientSex;
                        upw.SeriesInstanceUID = SeriesInstanceUID;
                        upw.StudyInstanceUID = StudyInstanceUID;
                        upw.acquisitionDate = acquisitionDate;
                        upw.acquisitionTime = acquisitionTime;

                        upw.ShowDialog();
                    }));
                });
            }

        }

        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            Ask4PickFile();
        }

        /// <summary>
        /// Test if file has a valid preamble and DICOM 3.0 header.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>True if valid DICOM 3.0 file header is detected.</returns>
        public static bool HasValidHeader(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    fs.Seek(128, SeekOrigin.Begin);
                    return fs.ReadByte() == 'D' && fs.ReadByte() == 'I' && fs.ReadByte() == 'C' && fs.ReadByte() == 'M';
                }
            }
            catch
            {
                return false;
            }
        }

        private string acquisitionDate;
        private string acquisitionTime;
        private string InstitutionName;
        private string PatientAge;
        private string PatientName;
        private string PatientSex;
        private string SeriesInstanceUID;
        private string StudyInstanceUID;
        private string filename;
        private long alreadySent;

        private void btnParseDicom_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                string ext = new FileInfo(filename).Extension.ToLower();
                if (ext != ".rar" && ext != ".zip" && ext != ".7z" && ext != ".tar" && ext != ".iso")
                {
                    MessageBox.Show("请选择压缩包。", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (new FileInfo(filename).Length > 1024 * 1024 * 1024)
                {
                    MessageBox.Show(string.Format("您选择的压缩包过大，体积为{0}，请选择1G以下的压缩包。", HumanReadableFilesize(new FileInfo(filename).Length)), "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // haozipc x I:\AirwayCTZip\20150812\甘福明.rar -oi:/temp/20151005
                string tmpFolder = System.IO.Path.Combine("i:/temp/" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "_" + Guid.NewGuid().ToString("N"));

                string haozipExe = @"C:\Program Files\2345Soft\HaoZip\HaoZipC.exe";
                string args = string.Format("x \"{0}\" -o\"{1}\"", filename, tmpFolder);

                Process.Start(haozipExe, args).WaitForExit();

                //Process.Start(tmpFolder);

                string[] files = Directory.GetFiles(tmpFolder, "*.*", SearchOption.AllDirectories);

                foreach (var item in files)
                {
                    if (!HasValidHeader(item))
                    {
                        continue;
                    }

                    var file = DicomFile.Open(item);
                    string patientName = file.Dataset.Get<string>(DicomTag.PatientName);
                    string patientSex = file.Dataset.Get<string>(DicomTag.PatientSex);
                    string patientAge = file.Dataset.Get<string>(DicomTag.PatientAge);
                    string institutionName = file.Dataset.Get<string>(DicomTag.InstitutionName);
                    string acquisitionDate = file.Dataset.Get<string>(DicomTag.AcquisitionDate);
                    string acquisitionTime = file.Dataset.Get<string>(DicomTag.AcquisitionTime);

                    string studyInstanceUID = file.Dataset.Get<string>(DicomTag.StudyInstanceUID);
                    string seriesInstanceUID = file.Dataset.Get<string>(DicomTag.SeriesInstanceUID);

                    LungCare.SupportPlatform.Models.CTDicomInfo CTDicomInfo = new LungCare.SupportPlatform.Models.CTDicomInfo();
                    CTDicomInfo.PatientName = patientName;
                    CTDicomInfo.PatientSex = patientSex;
                    CTDicomInfo.PatientAge = patientAge;
                    CTDicomInfo.InstitutionName = institutionName;
                    CTDicomInfo.AcquisitionDate = acquisitionDate;
                    CTDicomInfo.AcquisitionTime = acquisitionTime;
                    CTDicomInfo.StudyInstanceUID = studyInstanceUID;
                    CTDicomInfo.SeriesInstanceUID = seriesInstanceUID;

                    CTDicomInfo.FileName = item;
                    //CTDicomInfo.FileName = new FileInfo(_fileName).Name;
                    //CTDicomInfo.UserId = SessionManager.UserName;
                    //CTDicomInfo.Token = SessionManager.UserName;
                    CTDicomInfo.DataID = Guid.NewGuid().ToString("N");
                    CTDicomInfo.UploadTimestamp = DateTime.Now;
                    CTDicomInfo.Status = "已上传";
                    CTDicomInfo.AdditionalData = string.Format("{0} 已上传", DateTime.Now.ToString("yyyyMMdd HHmm"));

                    InstitutionName = institutionName;
                    PatientAge = patientAge;
                    PatientName = patientName;
                    PatientSex = patientSex;
                    SeriesInstanceUID = seriesInstanceUID;
                    StudyInstanceUID = studyInstanceUID;
                    this.acquisitionDate = acquisitionDate;
                    this.acquisitionTime = acquisitionTime;

                    Directory.Delete(tmpFolder, true);
                    Console.WriteLine(patientName);
                    break;
                }
            }
        }

        private void btnCancelUpload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn续传_Click(object sender, RoutedEventArgs e)
        {
            //ThreadPool.QueueUserWorkItem(delegate
            //{
            //    new MESPDownloadUpload().UploadFileAsync(
            //        (int)alreadySent,
            //        filename,
            //        LungCare.SupportPlatform.Security.SessionManager.UserName,
            //        MESPDownloadUpload.OrderId,
            //        MESPDownloadUpload.OrderNo,
            //        new EventHandler<ExceptionArgs>(delegate (Object senderUploadFile, ExceptionArgs eUploadFile)
            //        {
            //            Dispatcher.Invoke(new Action(delegate
            //            {
            //                UploadErrorOccurred();
            //                Util.ShowExceptionMessage(eUploadFile.Exception, "上传出错。");
            //            }));
            //            //errorCallback(eUploadFile.Exception);
            //        }),
            //        new EventHandler<FileUploadFinishedArgs>(delegate (Object senderUploadFile, FileUploadFinishedArgs eUploadFile)
            //        {
            //            Dispatcher.Invoke(new Action(delegate
            //            {
            //                FinishUpload();
            //                MessageBox.Show("上传文件完成。");
            //            }));
            //            //successCallback();
            //        }),
            //        new EventHandler<ProgressArgs>(delegate (Object senderProgress, ProgressArgs eProgress)
            //        {
            //            //uploadProgressCallback(eProgress);
            //            this.Dispatcher.BeginInvoke(new Action(delegate ()
            //            {
            //                alreadySent = eProgress.Finished.Value;

            //                StringBuilder stringUploadinfo = new StringBuilder();
            //                stringUploadinfo.AppendLine("已上传 ：" + HumanReadableFilesize(eProgress.Finished.Value));
            //                if (eProgress.RemainTimeInMillisecond.HasValue)
            //                {
            //                    stringUploadinfo.AppendLine("剩余时间：" + eProgress.RemainTimeInMillisecond.Value.ToString("0.0") + " s");
            //                }
            //                if (eProgress.Percentage.HasValue)
            //                {
            //                    stringUploadinfo.AppendLine("上传进度：" + (eProgress.Percentage.Value * 100).ToString("0.00") + " %");
            //                }
            //                stringUploadinfo.AppendLine("文件大小：" + HumanReadableFilesize(eProgress.Total));
            //                //labelDownloadInfo.Content = stringUploadinfo.ToString();
            //                //uploadProgress.Value = eProgress.Percentage.Value;
            //                System.Windows.Forms.Application.DoEvents();

            //                Console.ResetColor();
            //                stringUploadinfo.Clear();
            //            }));
            //        })
            //    );
            //});
        }

        private void btn重新上传_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                new MESPDownloadUpload().UploadFileAsync(
                    云数据类型Enum.Dicom数据压缩包,
                    filename,
                    LungCare.SupportPlatform.Security.SessionManager.UserName,
                    MESPDownloadUpload.OrderId,
                    MESPDownloadUpload.OrderNo,
                    false,
                    new EventHandler<ExceptionArgs>(delegate (Object senderUploadFile, ExceptionArgs eUploadFile)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            Util.ShowExceptionMessage(eUploadFile.Exception, "上传出错。");
                        }));
                        //errorCallback(eUploadFile.Exception);
                    }),
                    new EventHandler<FileUploadFinishedArgs>(delegate (Object senderUploadFile, FileUploadFinishedArgs eUploadFile)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("上传文件完成。");
                        }));
                        //successCallback();
                    }),
                    new EventHandler<ProgressArgs>(delegate (Object senderProgress, ProgressArgs eProgress)
                    {
                        //uploadProgressCallback(eProgress);
                        this.Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            alreadySent = eProgress.Finished.Value;

                            StringBuilder stringUploadinfo = new StringBuilder();
                            stringUploadinfo.AppendLine("已上传 ：" + HumanReadableFilesize(eProgress.Finished.Value));
                            if (eProgress.RemainTimeInMillisecond.HasValue)
                            {
                                stringUploadinfo.AppendLine("剩余时间：" + eProgress.RemainTimeInMillisecond.Value.ToString("0.0") + " s");
                            }
                            if (eProgress.Percentage.HasValue)
                            {
                                stringUploadinfo.AppendLine("上传进度：" + (eProgress.Percentage.Value * 100).ToString("0.00") + " %");
                            }
                            stringUploadinfo.AppendLine("文件大小：" + HumanReadableFilesize(eProgress.Total));
                            //labelDownloadInfo.Content = stringUploadinfo.ToString();
                            //uploadProgress.Value = eProgress.Percentage.Value;
                            System.Windows.Forms.Application.DoEvents();

                            Console.ResetColor();
                            stringUploadinfo.Clear();
                        }));
                    })
                );
            });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lbDicomError.Visibility = lbParsingDicom.Visibility = Visibility.Hidden;
        }
    }
}

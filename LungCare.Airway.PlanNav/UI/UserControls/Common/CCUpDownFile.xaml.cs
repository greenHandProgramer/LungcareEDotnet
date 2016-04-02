using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using Dicom.Imaging;
using Dicom;
using System.Windows.Threading;
using LungCare.SupportPlatform.Network;
using AirwayCT.Entity;
using LungCare.SupportPlatform.Models;
using LungCare.SupportPlatform.WebAPIWorkers;
using LungCare.SupportPlatform.SupportPlatformDAO.Files;

namespace LungCare.SupportPlatform.UI.UserControls.Common
{
    /// <summary>
    /// CCUpDownFile.xaml 的交互逻辑
    /// </summary>
    public partial class CCUpDownFile : UserControl
    {
       
        //上传
        public CCUpDownFile()
        {
            this.InitializeComponent();

        }

        private DataListItem _dataItem;
        private FileListItem _selectedFile;
        private 云数据类型Enum _云数据类型Enum;
        public CCUpDownFile(DataListItem dataItem, 云数据类型Enum 云数据类型Enum)
        {
            this.InitializeComponent();

            this._dataItem = dataItem;
            this._云数据类型Enum = 云数据类型Enum;
            GetFile(_dataItem);
        }

        private void GetFile(DataListItem dataItem)
        {
            MESPDownloadUpload.UserId = dataItem.UserId;
            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = dataItem.DataID;
            try
            {

                ThreadPool.QueueUserWorkItem(delegate
                {
                    new MESPDownloadUpload().FetchFileListAsync(
                        _云数据类型Enum,
                        new EventHandler<ExceptionArgs>(
                            delegate(Object senderInner, ExceptionArgs eInner)
                            {
                                ///MessageBox.Show("未找到文件!");
                            }),
                        new EventHandler<FileListFinishedArgs>(delegate(Object senderInner, FileListFinishedArgs eInner)
                        {
                            Dispatcher.BeginInvoke(new Action(delegate()
                            {

                                _selectedFile = eInner.Result[0];
                                txtFileName.Text = _dataItem.PatientName;
                                txtFileType.Text = _云数据类型Enum == 云数据类型Enum.Dicom数据压缩包 ? "DICOM" : "处理数据";
                                txtFileSize.Text ="0.00 / "+ FileDAO.HumanReadableFilesize( _selectedFile.FileSize);
                                //Progress.Maximum = selectedFile.FileSize;
                                Progress.Maximum = 1;

                                string filename = System.IO.Path.Combine( AirwayPatients.BaseDicomFolder , _selectedFile.FileName);

                                filename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filename) , System.IO.Path.GetFileNameWithoutExtension(filename)+".temp");
                                string selectedDicomPackageLocalPath = "";
                                try
                                {
                                    selectedDicomPackageLocalPath =
                                    new FileInfo(filename).FullName;
                                }
                                catch (Exception)
                                {
                                    //filename = @"C:\AirwayVE\CT\" + items.PatientName + ".zip";
                                    //filename =  Path.Combine(AirwayPatients.BaseDicomFolder, items.PatientName + ".zip");
                                    selectedDicomPackageLocalPath = new FileInfo(filename).FullName;
                                }

               
                                ThreadPool.QueueUserWorkItem(delegate
                                {
                                   DownloadDicom(_selectedFile,dataItem, selectedDicomPackageLocalPath, 云数据类型Enum.Dicom数据压缩包);
                                });
                            }
                             ));
                        }));
                }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        private void DownloadDicom(FileListItem selectedFile, DataListItem dataListItem, string selectedDicomPackageLocalPath, 云数据类型Enum 云数据类型Enum)
        {

            DownloadFileWorker.Download(
                selectedFile,
                selectedDicomPackageLocalPath,
                delegate(string filename)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        try
                        {
                            //string tempPath = selectedDicomPackageLocalPath.Substring(0, selectedDicomPackageLocalPath.LastIndexOf(Path.DirectorySeparatorChar));
                            //System.Diagnostics.Process.Start(tempPath);
                            //ResumeGUI();
                            while (!File.Exists(selectedDicomPackageLocalPath))
                            {

                            }

                            string destFileRAR = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(selectedDicomPackageLocalPath), System.IO.Path.GetFileNameWithoutExtension(selectedDicomPackageLocalPath) + ".zip");
                            File.Copy(selectedDicomPackageLocalPath, destFileRAR);
                            File.Delete(selectedDicomPackageLocalPath);
                            selectedDicomPackageLocalPath = destFileRAR;
                            //System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(selectedDicomPackageLocalPath));
                            string destFolder = AirwayPatients.BaseDicomFolder + "\\" + dataListItem.PatientName + "_" + dataListItem.StudyInstanceUID;
                            //解压缩并用dicomviewer打开
                            FileDAO.upZip(selectedDicomPackageLocalPath, destFolder);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    });
                },
                delegate(string errMsg)
                {
                    UIUtil.Invoke(this, delegate
                    {

                        MessageBox.Show("下载发生异常！", "提示");
                    });
                },
                delegate(Exception ex)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        Util.ShowExceptionMessage(ex);
                    });
                },
                delegate(ProgressArgs progressArgs)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        Console.WriteLine(progressArgs.Percentage.Value);
                        Progress.Value = progressArgs.Percentage.Value;
                        tbPercent.Text = ((progressArgs.Percentage.Value*100).ToString("F2")).ToString() + " %";
                        txtFileSize.Text =  FileDAO.HumanReadableFilesize(progressArgs.Total - progressArgs.Remains)  +" / "+ FileDAO.HumanReadableFilesize(_selectedFile.FileSize);
                        tbSpeed.Text = progressArgs.Speed.HasValue ? FileDAO.HumanReadableFilesize((long)progressArgs.Speed.Value) + "/s" : "";
                        Console.WriteLine(progressArgs);
                    });
                }, 云数据类型Enum);
        }

        private void DownloadAirwayResultFileWithoutOpen(FileListItem selectedFileItem, string selectedAirwayPackageLocalPath, 云数据类型Enum 云数据类型Enum)
        {
            DownloadFileWorker.Download(
                selectedFileItem,
                selectedAirwayPackageLocalPath,
                delegate(string filename)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        try
                        {
                            while (!File.Exists(selectedAirwayPackageLocalPath))
                            {
                            }
                            //Thread.Sleep(1000);
                            Process.Start(System.IO.Path.GetDirectoryName(selectedAirwayPackageLocalPath));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    });
                },
                delegate(string errMsg)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        MessageBox.Show(errMsg);
                    });
                },
                delegate(Exception ex)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        Util.ShowExceptionMessage(ex);
                        //Download(selectedFile, selectedDicomPackageLocalPath, 云数据类型Enum.处理结果);
                    });
                },
                delegate(ProgressArgs progressArgs)
                {
                    UIUtil.Invoke(this, delegate
                    {
                        Console.WriteLine(progressArgs.Percentage.Value);
                        Progress.Value = progressArgs.Percentage.Value;
                        tbPercent.Text = ((progressArgs.Percentage.Value * 100).ToString("F2")).ToString() + " %";
                        Console.WriteLine(progressArgs);
                    });
                }, 云数据类型Enum);
        }


        private void btnZanTing_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnZanTing_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void TxtShanChu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnOpenDir_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
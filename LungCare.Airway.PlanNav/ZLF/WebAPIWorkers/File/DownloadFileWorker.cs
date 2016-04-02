using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class DownloadFileWorker
    {
        public static void DownloadFile(
            云数据类型Enum 云数据类型,
            string orderId,
            Action<string> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback,
            Action<ProgressArgs> uploadProgressCallback)
        {
            new MESPDownloadUpload().FetchFileListAsync(
                云数据类型,
                SessionManager.UserName,
                orderId,
                orderId,
                 new EventHandler<ExceptionArgs>(delegate (Object senderInner, ExceptionArgs eInner)
                 {
                     errorCallback(eInner.Exception);
                 }),
                new EventHandler<FileListFinishedArgs>(delegate (Object senderInner, FileListFinishedArgs eInner)
                {
                    //UIUtil.Invoke(this, delegate
                    //{
                    FileListItem fli = eInner.Result.FirstOrDefault(item => item.FileName.ToLower().EndsWith(".avi")|| item.FileName.ToLower().EndsWith(".m4v"));

                    string orderDataFolder = new DirectoryInfo(Path.Combine(Security.SessionManager.UserName, orderId)).FullName;
                    Console.WriteLine("orderDataFolder  = " + orderDataFolder);

                    if (!Directory.Exists(orderDataFolder))
                    {
                        Console.WriteLine("Creating orderDataFolder = " + orderDataFolder);
                        Directory.CreateDirectory(orderDataFolder);
                    }

                    string tmpVrFileName = Path.Combine(orderDataFolder, fli.FileName + ".tmp");
                    string vrFileName = Path.Combine(orderDataFolder, fli.FileName);

                    if (fli != null)
                    {
                        if (File.Exists(vrFileName) && new FileInfo(vrFileName).Length == fli.FileSize)
                        {
                            successCallback(vrFileName);
                        }
                        else
                        {
                            Download(fli, tmpVrFileName, delegate
                            {
                                if (File.Exists((vrFileName)))
                                {
                                    File.Delete(vrFileName);
                                }

                                File.Move(tmpVrFileName, vrFileName);
                                successCallback(vrFileName);
                            }, failureCallback, errorCallback, uploadProgressCallback, 云数据类型);
                        }
                    }
                    else
                    {
                        failureCallback("没有找到视频规划结果");
                    }
                    //    if (fli == null)
                    //    {
                    //        MessageBox.Show("No video found.");
                    //    }
                    //});
                }
                ));
        }

        public static void Download(
            FileListItem fli,
            string localDstFullName,
            Action<string> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback,
            Action<ProgressArgs> uploadProgressCallback, 云数据类型Enum 云数据类型Enum)
        {
            Console.WriteLine("Downloading to " + localDstFullName);

            new MESPDownloadUpload().DownloadFileAsync(
                云数据类型Enum,
                fli.FileName,
                (int)fli.FileSize,
                localDstFullName,
                 new EventHandler<ExceptionArgs>(delegate (Object senderInner, ExceptionArgs eInner)
                 {
                     errorCallback(eInner.Exception);
                 }),
                new EventHandler<FileDownloadFinishedArgs>(delegate (Object senderInnerDownload, FileDownloadFinishedArgs eInnerDownload)
                {
                    successCallback(fli.FileName);
                }),
                new EventHandler<ProgressArgs>(delegate (Object senderInnerDownload, ProgressArgs eInnerDownload)
                {
                    uploadProgressCallback(eInnerDownload);
                })
            );
        }
    }
}

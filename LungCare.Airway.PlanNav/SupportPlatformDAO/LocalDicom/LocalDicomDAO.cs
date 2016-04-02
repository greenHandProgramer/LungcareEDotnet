using AirwayCT.Entity;
using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.Models;
using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.SupportPlatformDAO.Airway;
using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LungCare.SupportPlatform.SupportPlatformDAO.LocalDicom
{
    class LocalDicomDAO
    {
        public static bool Exists(string dicomFolder, string dataID)
        {
            AirwayPatient patient = AirwayPatients.FindByOrderId(dataID);
            bool hasLocalDicom = false;
            if (Directory.Exists(dicomFolder))
            {
                string[] dirs = Directory.GetDirectories(dicomFolder);
                
                if (dirs.Length > 0)
                {
                    foreach (var item in dirs)
                    {
                        string[] files1 = Directory.GetFiles(item);
                        if (files1.Length > 10)
                        {
                            hasLocalDicom = true;
                        }
                    }
                }

              
            }
            else if (patient != null)
            {
                if (Directory.Exists(patient.DicomFolder))
                {
                    if (Directory.GetFiles(patient.DicomFolder).Length > 10)
                    {
                        hasLocalDicom = true;
                    }
                }
            }


            return hasLocalDicom;
        }


        public static FileListItem GetDicomFileItemByDataID(string dataID, string userID)
        {
            MESPDownloadUpload.UserId = userID;
            MESPDownloadUpload.OrderId = MESPDownloadUpload.OrderNo = dataID;
            FileListItem selectedFile = new FileListItem();
            selectedFile.FileName = "test";
            try
            {
                Thread thread = new Thread(new ThreadStart(delegate
                {
                    new MESPDownloadUpload().FetchFileListAsync(
                       云数据类型Enum.Dicom数据压缩包,
                       new EventHandler<ExceptionArgs>(
                           delegate(Object senderInner, ExceptionArgs eInner)
                           {
                               Console.WriteLine("未找到文件 : "+dataID );
                               //MessageBox.Show("未找到文件!");
                           }),
                       new EventHandler<FileListFinishedArgs>(delegate(Object senderInner, FileListFinishedArgs eInner)
                       {
                           Console.WriteLine("FileListFinishedArgs");
                           selectedFile = eInner.Result[0];

                       }));
                }));
                thread.Start();
                thread.Join();
               //ThreadPool.QueueUserWorkItem(delegate
               //{
                   
               //});

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            while (selectedFile != null && selectedFile.FileName== "test")
            {

            }
            Console.WriteLine("selectedFile");
            return selectedFile;
        }


        public static FileListItem GetFileListItem(DataListItem dataListItem)
        {
            return GetDicomFileItemByDataID(dataListItem.DataID, dataListItem.UserId);
        }


        public static List<FileListItem> GetFileListItem(List<DataListItem> dataListItems)
        {
            List<FileListItem> list = new List<FileListItem>();
            foreach (var item in dataListItems)
            {
                FileListItem file = GetFileListItem(item);
                if (file != null)
                {
                    list.Add(file);
                }
            }

            return list;
        }


        public static List<DataItemFilePair> GetFileListItemPair(List<DataListItem> dataListItems)
        {
            List<DataItemFilePair> list = new List<DataItemFilePair>();
            foreach (var item in dataListItems)
            {
                if (item.DataID.Contains("."))//此条件判断绑定的是UID还是DataID
                {
                    continue;
                }
                FileListItem file = GetFileListItem(item);
                if (file != null)
                {
                    list.Add(new DataItemFilePair()
                        {
                            DataListItem = item,
                            FileListItem = file
                        });
                }
            }

            return list;
        }


        public static void DownloadDicomFileListItem(FileListItem file)
        {
            DownloadFileWorker.Download(
                file,
                "",
                delegate(string filename)
                {
                },
                delegate(string errMsg)
                {
                        MessageBox.Show("下载发生异常！", "提示");
                },
                delegate(Exception ex)
                {
                        Util.ShowExceptionMessage(ex);
                },
                delegate(ProgressArgs progressArgs)
                {
                    
                        progressArgs.ToString();

                        Console.WriteLine(progressArgs);
                }, 云数据类型Enum.Dicom数据压缩包);
        }



        private void GetAllNotUploadDicomPath()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {


                RetrieveDataListWorker.SendRetrieveDataListRequest(
                    successCallback:
                    delegate(RetrieveDataListResponse response)
                    {
                       

                        string[] dirs = Directory.GetDirectories(AirwayPatients.BaseDicomFolder);
                        List<string> studyUIDsNotUpload = new List<string>();
                        if (dirs != null && dirs.Length > 0)
                        {
                            for (int i = 0; i < dirs.Length; i++)
                            {
                                int a  = dirs[i].LastIndexOf('_') ;
                                string studyUID = dirs[i].Substring(a+1 , dirs[i].Length - a - 1);
                                bool isExist = false;
                                foreach (var item in response.DataList)
                                {
                                    if (item.StudyInstanceUID == studyUID)
                                    {
                                        isExist = true;
                                    }
                                }
                                if (!isExist)
                                {
                                    studyUIDsNotUpload.Add(studyUID);
                                }
                            }
                        }

                        for (int i = 0; i < studyUIDsNotUpload.Count; i++)
                        {
                                
                        }
                        


                    },
                    failureCallback:
                    delegate(string failureReason)
                    {
                        //System.Windows.Forms.MessageBox.Show("获取数据列表失败。" + failureReason, "", System.Windows.MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    },
                    errorCallback:
                    delegate(Exception ex)
                    {
                        Util.ShowExceptionMessage(ex, "获取数据列表出错。");
                    });
            });

        }


        public static string GetLocalDicomPath(string studyUID)
        {
            string[] dirs = Directory.GetDirectories(AirwayPatients.BaseDicomFolder);
            if (dirs != null && dirs.Length > 0)
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    if (dirs[i].Contains(studyUID))
                    {
                        return dirs[i];
                    }
                }
            }

            return null;
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
        }
    }
}

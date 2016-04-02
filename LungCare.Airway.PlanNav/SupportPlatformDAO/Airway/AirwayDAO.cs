using AirwayCT.Entity;
using LungCare.SupportPlatform.Entities;
using LungCare.SupportPlatform.Models;
using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.SupportPlatformDAO.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LungCare.SupportPlatform.SupportPlatformDAO.Airway
{
    class AirwayDAO
    {
        private static int localDicomCount = 10;
        public static bool isExistLocalDicom(DataListItem dataListItem)
        {
            string folder = Path.Combine(AirwayCT.Entity.AirwayPatients.BaseDicomFolder,  dataListItem.PatientName + "_" + dataListItem.StudyInstanceUID);
            int count = FileDAO.GetFilesCount(folder);
            if (count < localDicomCount)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        public static AirwayPatient ExistedAirwayData(string dataID)
        {
            AirwayPatient patient = AirwayPatients.FindByOrderId(dataID);
            if (patient != null)
            {
                if (File.Exists(patient.AirwayVTP_FileName) && File.Exists(patient.SegmentedMhd_FileName)
                    && File.Exists(patient.吸气末期MhdFileName))
                {
                    return patient;
                }
            }
            else
            {
                return null;
            }


            return null;

        }

        public static bool isExistLocalAirway(DataListItem dataListItem)
        {

            AirwayPatient patient = AirwayPatients.FindByOrderId(dataListItem.DataID);
            if (patient != null)
            {
                if (File.Exists(patient.AirwayVTP_FileName) && File.Exists(patient.SegmentedMhd_FileName)
                    && File.Exists(patient.吸气末期MhdFileName))
                {
                    return true;
                }
            }
            else
            {
                return false;
            }


            return false;
            return false;

        }



        public static List<DataItemFilePair> GetFileListItemPair(List<DataListItem> dataListItems)
        {
            List<DataItemFilePair> list = new List<DataItemFilePair>();
            foreach (var item in dataListItems)
            {
                FileListItem file = GetFileListItem(item);
                if (file.FileName != "null")
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


        public static FileListItem GetFileListItem(DataListItem dataListItem)
        {
            return GetDicomFileItemByDataID(dataListItem.DataID, dataListItem.UserId);
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
                       云数据类型Enum.处理结果,
                       new EventHandler<ExceptionArgs>(
                           delegate(Object senderInner, ExceptionArgs eInner)
                           {
                               MessageBox.Show("未找到文件!");
                           }),
                       new EventHandler<FileListFinishedArgs>(delegate(Object senderInner, FileListFinishedArgs eInner)
                       {
                           Console.WriteLine("FileListFinishedArgs");
                           if (eInner.Result != null && eInner.Result.Count>0)
                           {
                               selectedFile = eInner.Result[0];
                           }else
                           {
                               selectedFile.FileName = "null";
                           }
                           

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
            while (selectedFile != null && selectedFile.FileName == "test")
            {

            }
            Console.WriteLine("selectedFile");
            return selectedFile;
        }

    }
}

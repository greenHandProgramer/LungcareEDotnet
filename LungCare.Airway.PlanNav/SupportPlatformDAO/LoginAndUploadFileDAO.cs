
using Dicom;
using LungCare.SupportPlatform.Models;
using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LungCare.SupportPlatform.SupportPlatformDAO
{
    class LoginAndUploadFileDAO
    {

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


                            LungCare.SupportPlatform.UI.UploadProgressWnd upw = new LungCare.SupportPlatform.UI.UploadProgressWnd();
                            //upw.Owner = LungCare_Airway_PlanNav.MainWindow.Instance;

                            InstitutionName = "Zhongshan Hospital,Fudan Univ.";
                            PatientName = "Gan^ Fuming";
                            PatientAge = "062Y";
                            PatientSex = "M";
                            SeriesInstanceUID = "1.2.840.113619.2.55.3.269126727.31.1438774442.877.3";
                            StudyInstanceUID = "1.2.840.113619.2.55.3.269126727.31.1438774442.873";
                            acquisitionDate = "20150807";
                            acquisitionTime = "130902";
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



        private string zipFolder2Zip(string folder, CTDicomInfo dicomInfo)
        {
            string fileName = string.Format("[{0}][{3}][{1}][{2}].zip", dicomInfo.PatientName, dicomInfo.InstitutionName, System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

            //string args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\" \"{3}\"", fileName, SelectedPatient.GetFile(""), SelectedPatient.DicomFolder, fileName);
            string args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\" \"{3}\"", fileName, fileName, fileName, fileName);
            var psi = new ProcessStartInfo(Application.StartupPath + @"\HaoZipC.exe", args);
            Process.Start(psi).WaitForExit();
            MessageBox.Show(string.Format(@"压缩完毕。
文件保存到{0}。", fileName));
            //Process.Start(new FileInfo(fileName).Directory.FullName);



            return fileName;
        }

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

        private void PickDCM(string dicomFolder)
        {
            string[] files = Directory.GetFiles(dicomFolder, "*.*", SearchOption.AllDirectories);

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
                Directory.Delete(dicomFolder, true);
            }
            catch (Exception deleteEx)
            {
                Console.WriteLine(deleteEx.Message);
            }

            if (!foundDicom)
            {
                return;
            }

            GC.Collect();

            //ThreadPool.QueueUserWorkItem(delegate
            //{
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
        public void UploadFile(string filename)
        {
            Login();
        }



        private void Download()
        {

        }
    }
}

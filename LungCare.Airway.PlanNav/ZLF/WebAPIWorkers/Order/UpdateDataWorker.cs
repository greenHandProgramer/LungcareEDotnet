using LungCare.SupportPlatform.Network;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class UpdateDataWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/UpdateData";

        public static void SendUpdateDataRequest(
            LungCare.SupportPlatform.Models.UpdateDataRequest updateDicomDataRequest,
            Action<Models.GeneralWebAPIResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Util.PostAsync<Models.GeneralWebAPIResponse>(
                updateDicomDataRequest,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }

        public static void SendUpdateDataRequest(
            LungCare.SupportPlatform.Models.CTDicomInfo ctDicomInfo, string filename,string status,
            Action<Models.GeneralWebAPIResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.UpdateDataRequest updateDicomDataRequest = new Models.UpdateDataRequest();
            updateDicomDataRequest.Sender = "PC Client";

            updateDicomDataRequest.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            updateDicomDataRequest.AcquisitionDate = DateTime.Now.ToLongDateString();
            updateDicomDataRequest.AcquisitionTime = DateTime.Now.ToLongTimeString();

            updateDicomDataRequest.AcquisitionDate = ctDicomInfo.AcquisitionDate;
            updateDicomDataRequest.AcquisitionTime = ctDicomInfo.AcquisitionTime;

            updateDicomDataRequest.DataID = MESPDownloadUpload.OrderId;
            updateDicomDataRequest.FileName = new FileInfo(filename).Name;
            updateDicomDataRequest.InstitutionName = ctDicomInfo.InstitutionName;
            updateDicomDataRequest.PatientAge = ctDicomInfo.PatientAge;
            updateDicomDataRequest.PatientName = ctDicomInfo.PatientName;
            updateDicomDataRequest.PatientSex = ctDicomInfo.PatientSex;
            updateDicomDataRequest.SeriesInstanceUID = ctDicomInfo.SeriesInstanceUID;
            updateDicomDataRequest.StudyInstanceUID = ctDicomInfo.StudyInstanceUID;
            updateDicomDataRequest.UploadTimestamp = ctDicomInfo.UploadTimestamp.ToString("yyyyMMdd HHmmss");

            //updateDicomDataRequest.InstitutionName = "IN";
            //updateDicomDataRequest.StudyInstanceUID = "SUID";
            //updateDicomDataRequest.SeriesInstanceUID = "SUID";
            updateDicomDataRequest.Status = "已上传";
            updateDicomDataRequest.Status = status;
            updateDicomDataRequest.AdditionalData = ctDicomInfo.UploadTimestamp.ToString("yyyy年MM月dd日 HH时mm分ss秒") + " 上传数据。";

            SendUpdateDataRequest(updateDicomDataRequest, successCallback, failureCallback, errorCallback);
        }
    }
}

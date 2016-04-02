using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class UploadCertificateByAdminWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/UploadCertificateByAdmin";

        public static void SendUploadCertificateRequest(
            string CertificateImage,
            string userID,
            Action<LungCare.SupportPlatform.Models.UploadCertificateByAdminResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            LungCare.SupportPlatform.Models.UploadCertificateByAdminRequest request = new LungCare.SupportPlatform.Models.UploadCertificateByAdminRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.CertificateImage = CertificateImage;
            request.UserId = userID;
            Util.PostAsync<LungCare.SupportPlatform.Models.UploadCertificateByAdminResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback
                );

        }
    }
}

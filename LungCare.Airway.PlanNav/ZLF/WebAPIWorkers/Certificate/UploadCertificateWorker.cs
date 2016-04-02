using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class UploadCertificateWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/UploadCertificate";

        public static void SendUploadCertificateRequest(
            string CertificateImage,
            Action<Models.UploadCertificateResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.UploadCertificateRequest request = new Models.UploadCertificateRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.CertificateImage = CertificateImage;
            Util.PostAsync<Models.UploadCertificateResponse>(
                request ,
                URI,
                successCallback,
                failureCallback,
                errorCallback
                );

        }
     
    }
}

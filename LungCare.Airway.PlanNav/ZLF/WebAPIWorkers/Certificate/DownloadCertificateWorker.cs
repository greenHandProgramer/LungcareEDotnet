using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class DownloadCertificateWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/DownloadCertificate";

        public static void SendDownloadCertificateRequest(
            Action<Models.DownloadCertificateRespnse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.DownloadCertificateRequest request = new Models.DownloadCertificateRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            Util.PostAsync<Models.DownloadCertificateRespnse>(
                request ,
                URI,
                successCallback,
                failureCallback,
                errorCallback
                );

        }
     
    }
}

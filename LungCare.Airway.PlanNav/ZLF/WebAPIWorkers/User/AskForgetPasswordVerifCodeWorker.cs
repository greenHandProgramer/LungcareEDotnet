using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class AskForgetPasswordVerifCodeWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/AskForgetPasswordVerifCode";

        public static void SendDownloadCertificateRequest(
            string username,
            Action<Models.AskForgetPasswordVerifCodeResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.AskForgetPasswordVerifCodeRequest request = new Models.AskForgetPasswordVerifCodeRequest();
            request.Sender = "PC Client";
            request.UserName = username;

            Util.PostAsync<Models.AskForgetPasswordVerifCodeResponse>(
                request ,
                URI,
                5,
                successCallback,
                failureCallback,
                errorCallback
                );

        }
     
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class DownloadUserIconWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/DownloadUserIcon";

        public static void SendUserIconRequest(
            Action<Models.DownloadUserIconRespnse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.DownloadUserIconRequest request = new Models.DownloadUserIconRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;

            Util.PostAsync<Models.DownloadUserIconRespnse>(
                request ,
                URI,
                successCallback,
                failureCallback,
                errorCallback
                );

        }
     
    }
}

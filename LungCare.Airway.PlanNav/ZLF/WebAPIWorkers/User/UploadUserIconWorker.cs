using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class UploadUserIconWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/UploadUserIcon";

        public static void SendUserIconRequest(
            string userIconImage,
            Action<Models.UploadUserIconResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.UploadUserIconRequest request = new Models.UploadUserIconRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.UserIconImage = userIconImage;

            Util.PostAsync<Models.UploadUserIconResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback
                );

        }
    }
}

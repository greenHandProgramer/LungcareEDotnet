using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class RetrieveUserInfoWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/GetUserInfo";

        public static void SendUserInfoRequest(
            Action<Models.GetUserInfoResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.GetUserInfoRequest request = new Models.GetUserInfoRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.UserId = LungCare.SupportPlatform.Security.SessionManager.UserName;
            Util.PostAsync<Models.GetUserInfoResponse>(
                request ,
                URI,
                130,
                successCallback,
                failureCallback,
                errorCallback
                );

        }
     
    }
}

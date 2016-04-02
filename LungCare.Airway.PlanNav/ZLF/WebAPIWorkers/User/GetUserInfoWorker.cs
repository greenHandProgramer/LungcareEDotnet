using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LungCare.SupportPlatform.WebAPIWorkers;

namespace LungCare.SupportPlatform.Models
{
    public class GetUserInfoWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/GetUserInfo";

        public static void SendGetUserInfoRequest(string userID,
            Action<Models.UserInfo> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.GetUserInfoRequest request = new Models.GetUserInfoRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;
            request.UserId = userID;

            Util.PostAsync<Models.UserInfo>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

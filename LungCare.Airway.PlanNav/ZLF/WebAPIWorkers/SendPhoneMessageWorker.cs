using LungCare.SupportPlatform.WebAPIWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class SendPhoneMessageWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/SendPhoneMessage";
        public static void SendPhoneMessageRequeset(
            string message,
            string userid,
            Action<LungCare.SupportPlatform.Models.SendPhoneMessageResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            LungCare.SupportPlatform.Models.SendPhoneMessageRequest request = new LungCare.SupportPlatform.Models.SendPhoneMessageRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.UserId = userid;
            request.TextMessage = message;

            Util.PostAsync<LungCare.SupportPlatform.Models.SendPhoneMessageResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

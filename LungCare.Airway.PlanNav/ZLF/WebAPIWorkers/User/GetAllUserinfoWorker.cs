using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class GetAllUserInfoWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/GetAllUserInfo";

        public static void SendGetAllUserInfoRequest(
            Action<Models.GetAllUserInfoResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.GetUserNamesRequest request = new Models.GetUserNamesRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;

            Util.PostAsync<Models.GetAllUserInfoResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

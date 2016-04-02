using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class GetUserNamesWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/GetUserNames";

        public static void SendGetUserNamesRequest(
            Action<Models.GetUserNamesResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.GetUserNamesRequest request = new Models.GetUserNamesRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;

            Util.PostAsync<Models.GetUserNamesResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

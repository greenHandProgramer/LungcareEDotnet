using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class GetCertiWaitingApprovedUserNamesWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/GetCertiWaitingApprovedUserNames";

        public static void SendGetCertiWaitingApprovedUserNamesRequest(
            Action<Models.GetCertiWaitingApprovedUserNamesResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.GetCertiWaitingApprovedUserNamesRequest request = new Models.GetCertiWaitingApprovedUserNamesRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;

            Util.PostAsync<Models.GetCertiWaitingApprovedUserNamesResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

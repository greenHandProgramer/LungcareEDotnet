using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class CancelOrdersWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/CancelOrders";

        public static void SendCancelOrdersRequeset(
            string userId,
            Action<Models.CancelOrdersResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.CancelOrdersRequest request = new Models.CancelOrdersRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;
            request.UserId = userId;

            Util.PostAsync<Models.CancelOrdersResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

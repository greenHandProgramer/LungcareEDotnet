using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class CancelOrderWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/CancelOrder";

        public static void SendCancelOrderRequeset(
            string orderId,
            Action<Models.CancelOrderResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.CancelOrderRequest request = new Models.CancelOrderRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;
            request.OrderId = orderId;

            Util.PostAsync<Models.CancelOrderResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

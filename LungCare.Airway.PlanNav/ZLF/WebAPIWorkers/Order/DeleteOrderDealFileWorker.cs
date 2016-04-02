using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class DeleteOrderDealFileWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/DeleteOrderDealFile";

        public static void SendDeleteOrderDealFileRequeset(
            string orderId,
            string filename,
            Action<Models.DeleteOrderDealFileResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.DeleteOrderDealFileRequest request = new Models.DeleteOrderDealFileRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;
            request.OrderId = orderId;
            request.DealFileName = filename;

            Util.PostAsync<Models.DeleteOrderDealFileResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

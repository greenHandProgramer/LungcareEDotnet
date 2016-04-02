using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class DeleteOrderDicomFileWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/DeleteOrderDicomFile";

        public static void SendDeleteOrderDicomFileRequeset(
            string orderId,
            string filename,
            Action<Models.DeleteOrderDicomFileResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.DeleteOrderDicomFileRequest request = new Models.DeleteOrderDicomFileRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;
            request.OrderId = orderId;
            request.DicomFileName = filename;

            Util.PostAsync<Models.DeleteOrderDicomFileResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

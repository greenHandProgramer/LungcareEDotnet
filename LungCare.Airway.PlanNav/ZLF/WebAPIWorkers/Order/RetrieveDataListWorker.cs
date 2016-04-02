using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class RetrieveDataListWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/RetrieveDataList";

        public static void SendRetrieveDataListRequest(
            Models.RetrieveDataListRequest updateDicomDataRequest,
            Action<Models.RetrieveDataListResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Util.PostAsync<Models.RetrieveDataListResponse>(
                updateDicomDataRequest,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }

        public static void SendRetrieveDataListRequest(
            Action<Models.RetrieveDataListResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.RetrieveDataListRequest updateDicomDataRequest = new Models.RetrieveDataListRequest();
            updateDicomDataRequest.Sender = "PC Client";

            updateDicomDataRequest.Token = Security.TokenManager.Token;
            updateDicomDataRequest.UserId = Security.SessionManager.UserName;

            SendRetrieveDataListRequest(updateDicomDataRequest, successCallback, failureCallback, errorCallback);
        }
    }
}

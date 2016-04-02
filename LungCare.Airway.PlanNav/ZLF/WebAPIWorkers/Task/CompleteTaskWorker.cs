using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class CompleteTaskWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/CompleteTask";

        public static void SendCompleteTaskRequeset(
            string dataId,
            string operation,
            Action<Models.CompleteTaskResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.CompleteTaskRequest request = new Models.CompleteTaskRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;
            request.DataID = dataId;
            request.Operation = operation;

            Util.PostAsync<Models.CompleteTaskResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

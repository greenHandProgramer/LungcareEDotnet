using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class DeleteUserWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/DeleteUser";

        public static void SendDeleteUserRequest(
            string userId,
            Action<Models.DeleteUserResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.DeleteUserRequest request = new Models.DeleteUserRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;
            request.UserId = userId;

            Util.PostAsync<Models.DeleteUserResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

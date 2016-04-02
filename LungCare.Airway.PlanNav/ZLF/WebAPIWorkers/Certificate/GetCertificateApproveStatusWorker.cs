using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class GetCertificateApproveStatusWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/GetCertificateApproveStatus";

        public static void SendGetCertificateApproveStatusRequeset(
            string userId,
            Action<Models.GetCertificateApproveStatusResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.GetCertificateApproveStatusRequest request = new Models.GetCertificateApproveStatusRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;
            request.UserId = userId;

            Util.PostAsync<Models.GetCertificateApproveStatusResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

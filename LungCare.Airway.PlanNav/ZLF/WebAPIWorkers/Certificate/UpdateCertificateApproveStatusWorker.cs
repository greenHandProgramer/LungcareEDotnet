using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class UpdateCertificateApproveStatusWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/UpdateCertificateApproveStatus";

        public static void SendUpdateCertificateApproveStatusRequeset(
            string userId,
            bool result,
            string rejectReason,
            Action<Models.UpdateCertificateApproveStatusResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.UpdateCertificateApproveStatusRequest request = new Models.UpdateCertificateApproveStatusRequest();
            request.Sender = "PC Client";
            request.Token = Security.TokenManager.Token;
            request.UserId = userId;
            request.Result = result;
            request.RejectReason = rejectReason;

            Util.PostAsync<Models.UpdateCertificateApproveStatusResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

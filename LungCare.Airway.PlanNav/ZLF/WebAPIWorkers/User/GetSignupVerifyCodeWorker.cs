using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class GetSignupVerifyCodeWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/GetSignupVerifyCode";

        public static void SendGetSignupVerifyCodeRequest(
            string phoneNumber,
            Action<Models.GetSignupVerifyCodeResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.GetSignupVerifyCodeRequest request = new Models.GetSignupVerifyCodeRequest();
            request.Sender = "PC Client";
            request.PhoneNumber = phoneNumber;

            Util.PostAsync<Models.GetSignupVerifyCodeResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

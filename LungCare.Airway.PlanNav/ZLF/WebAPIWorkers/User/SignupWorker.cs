using System;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class SignupWorker
    {
        static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/Signup";
        //static string URI = @"http://216.11.253.243:11888/lungcare/webapi/lungcare/Signup";

        public static void SendSignupRequest(
            string username,
            string password,
            string chineseName,
            string institution,
            string department,
            string certificateImage,
            string verifyCode,
            Action<Models.SignupResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.SignupRequest request = new Models.SignupRequest();
            request.PhoneNumber = username;
            request.Password = Util.Encrypt(password);
            request.ChineseName = chineseName;
            request.Institution = institution;
            request.Department = department;
            request.CertificateImage =certificateImage;
            request.Sender = "PC Client";
            request.VerifyCode = verifyCode;

            Util.PostAsync<Models.SignupResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }

        public static void SendSignupRequest(
            Models.SignupRequest signupRequest,
            Action<Models.SignupResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Util.PostAsync<Models.SignupResponse>(
                signupRequest,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }
    }
}

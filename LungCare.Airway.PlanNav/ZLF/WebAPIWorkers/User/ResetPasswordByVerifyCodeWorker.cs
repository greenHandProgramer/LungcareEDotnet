using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class ResetPasswordByVerifyCodeWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/ResetPassword";

        public static void ResetPasswordByVerifyCodeRequest(
            string verifyCode,
            string username,
            string newpassword,
            Action<Models.ResetPasswordByVerifyCodeResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.ResetPasswordByVerifyCodeRequest request = new Models.ResetPasswordByVerifyCodeRequest();
            request.Sender = "PC Client";
            request.UserName = username;
            request.VerifyCode = verifyCode;
            request.NewPassword = newpassword;
            Util.PostAsync<Models.ResetPasswordByVerifyCodeResponse>(
                request ,
                URI,
                successCallback,
                failureCallback,
                errorCallback
                );

        }
     
    }
}

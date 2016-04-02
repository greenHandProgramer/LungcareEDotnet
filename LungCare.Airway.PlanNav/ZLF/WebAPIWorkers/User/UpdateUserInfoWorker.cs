using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class UpdateUserInfoWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/UpdateUserInfo";

        public static void UpdateUserInfoRequest(
            string PhoneNumber,
            string OldPassword,
            string NewPassword,
            string ChineseName,
            string Institution,
            string Department,
            string CertificateImage,
            Action<Models.UpdateUserInfoResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.UpdateUserInfoRequest request = new Models.UpdateUserInfoRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.PhoneNumber = PhoneNumber;
            request.OldPassword = OldPassword;
            request.NewPassword = NewPassword;
            request.ChineseName = ChineseName;
            request.Institution = Institution;
            request.Department = Department;
            request.CertificateImage = CertificateImage;
            
            Util.PostAsync<Models.UpdateUserInfoResponse>(
                request ,
                URI,
                50,
                successCallback,
                failureCallback,
                errorCallback
                );

        }
     
    }
}

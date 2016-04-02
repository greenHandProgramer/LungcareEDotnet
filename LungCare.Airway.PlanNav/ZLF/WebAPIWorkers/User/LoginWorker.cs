using System;
using System.Windows;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class LoginWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/Login";
        //private static string URI = "http://216.11.253.243:11888/lungcare/webapi/lungcare/Login";

        public static void SendLoginRequest(
            string username,
            string password,
            Action<Models.LoginResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.LoginRequest request = new Models.LoginRequest();
            request.Password = Util.Encrypt(password);
            request.UserName = username;
            request.Sender = "PC Client";
            try
            {
                Util.PostAsync<Models.LoginResponse>(
              request,
              URI,
              25,
              successCallback,
              failureCallback,
              errorCallback);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
          
        }
    }
}

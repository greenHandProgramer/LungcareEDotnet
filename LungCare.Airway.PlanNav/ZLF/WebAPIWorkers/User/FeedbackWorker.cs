using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class FeedbackWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/Feedback";

        public static void FeedbackRequest(
            string title,
            string content,
            string comitter,
            Action<Models.FeedbackResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.FeedbackRequest request = new Models.FeedbackRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.Title = title;
            request.Content = content;
            //request.Submitter = comitter;
            Util.PostAsync<Models.FeedbackResponse>(
                request ,
                URI,
                successCallback,
                failureCallback,
                errorCallback
                );

        }
     

        
        public static void SendTest(){
            string url = "http://116.11.253.243:11888/lungcare/webapi/lungcare/AddNotification";

            for (int i = 0; i < 10; i++)
            {


                Models.AddNotificationRequest request = new Models.AddNotificationRequest();
                request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
                request.Sender = "PC Client";
                request.NotificationSentBy = "admin";
                request.NotificationSentTo = "15261595318";
                //request.NotificationTimeStamp = "20151023 122322";
                request.NotificationContent = "您有新的处理好数据，请查收！";
                Util.PostAsync<Models.AddNotificationResponse>(
                    request,
                    url,
                    null,
                    null,
                    null
                    );
            }
        }
    }
}

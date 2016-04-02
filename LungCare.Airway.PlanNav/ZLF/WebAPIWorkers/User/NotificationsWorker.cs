using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    class NotificationsWorker
    {
        private static string URI = "http://116.11.253.243:11888/lungcare/webapi/lungcare/GetNotifications";

        public static void GetNotificationRequest(
            string userid,
            Action<Models.GetNotificationsResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.GetNotificationsRequest request = new Models.GetNotificationsRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.UserId = userid;
            Util.PostAsync<Models.GetNotificationsResponse>(
                request,
                URI,
                successCallback,
                failureCallback,
                errorCallback
                );

        }

        public static void DeleteNotificationRequest(
           string userid,
            string notificationId,
           Action<Models.DelNotificationResponse> successCallback,
           Action<string> failureCallback,
           Action<Exception> errorCallback)
        {
            string url = "http://116.11.253.243:11888/lungcare/webapi/lungcare/DelNotification";
            Models.DelNotificationRequest request = new Models.DelNotificationRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.UserId = userid;
            request.NotificationID = notificationId;
            Util.PostAsync<Models.DelNotificationResponse>(
                request,
                url,
                successCallback,
                failureCallback,
                errorCallback
                );

        }

        public static void DeleteNotificationRequest(
          string userid,
           List<string> notificationIdList,
          Action<Models.DelNotificationResponse> successCallback,
          Action<string> failureCallback,
          Action<Exception> errorCallback)
        {
            string url = "http://116.11.253.243:11888/lungcare/webapi/lungcare/DelNotification";
            int n = notificationIdList.Count;
            for (int i = 0; i < notificationIdList.Count; i++)
            {
                 Models.DelNotificationRequest request = new Models.DelNotificationRequest();
                request.Sender = "PC Client";
                request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
                request.UserId = userid;
                request.NotificationID = notificationIdList[i];
                if (i == n - 1)
                {
                    Util.PostAsync<Models.DelNotificationResponse>(
                        request,
                        url,
                        successCallback,
                        failureCallback,
                        errorCallback
                        );
                }
                else
                {
                    Util.PostAsync<Models.DelNotificationResponse>(
                       request,
                       url,
                       null,
                       failureCallback,
                       errorCallback
                       );
                }
            }
            //foreach (var item in notificationIdList)
            //{
            //    Models.DelNotificationRequest request = new Models.DelNotificationRequest();
            //    request.Sender = "PC Client";
            //    request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            //    request.UserId = userid;
            //    request.NotificationID = item;
            //    Util.PostAsync<Models.DelNotificationResponse>(
            //        request,
            //        url,
            //        successCallback,
            //        failureCallback,
            //        errorCallback
            //        );
            //}


        }


        public static void DeleteAllNotificationRequest(
          string userid,
          Action<Models.DelAllNotificationResponse> successCallback,
          Action<string> failureCallback,
          Action<Exception> errorCallback)
        {
            string url = "http://116.11.253.243:11888/lungcare/webapi/lungcare/DelAllNotifications";
            Models.DelAllNotificationRequest request = new Models.DelAllNotificationRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.UserId = userid;
            Util.PostAsync<Models.DelAllNotificationResponse>(
                request,
                url,
                successCallback,
                failureCallback,
                errorCallback
                );

        }


        public static void MarkNotificationReadRequest(
         string notificationID,
         Action<Models.MarkNotificationReadResponse> successCallback,
         Action<string> failureCallback,
         Action<Exception> errorCallback)
        {
            string url = "http://116.11.253.243:11888/lungcare/webapi/lungcare/MarkNotificationRead";
            Models.MarkNotificationReadRequest request = new Models.MarkNotificationReadRequest();
            request.Sender = "PC Client";
            request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
            request.NotificationID = notificationID;
            Util.PostAsync<Models.MarkNotificationReadResponse>(
                request,
                url,
                successCallback,
                failureCallback,
                errorCallback
                );

        }


        public static void SendTest()
        {
            string url = "http://116.11.253.243:11888/lungcare/webapi/lungcare/AddNotification";

            for (int i = 0; i < 30; i++)
            {


                Models.AddNotificationRequest request = new Models.AddNotificationRequest();
                request.Token = LungCare.SupportPlatform.Security.TokenManager.Token;
                request.Sender = "PC Client";
                request.NotificationSentBy = "admin";
                request.NotificationSentTo = "13501299816";
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

        public static void SendAddNotification(string sentTo, string txt)
        {
            string url = "http://116.11.253.243:11888/lungcare/webapi/lungcare/AddNotification";

            Models.AddNotificationRequest request = new Models.AddNotificationRequest();
            request.Token = Security.TokenManager.Token;
            request.Sender = "PC Client";
            request.NotificationSentBy = "admin";
            request.NotificationSentTo = sentTo;
            //request.NotificationTimeStamp = "20151023 122322";
            request.NotificationContent = txt;
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

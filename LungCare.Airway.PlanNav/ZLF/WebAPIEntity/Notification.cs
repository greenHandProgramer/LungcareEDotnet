using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.Models
{
    public class GetNotificationsRequest
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string Sender { get; set; }
    }

    public class GetNotificationsResponse : GeneralWebAPIResponse
    {

        public List<NotificationItem> NotificationList { get; set; }
    }

    public class NotificationItem : INotifyPropertyChanged
    {
        public string ID { get; set; }
        public string SendBy { get; set; }
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }

        public bool Read { get; set; }


        private bool _IsSelected = false;
        public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; OnChanged("IsSelected"); } }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        #endregion
    }


    public class AddNotificationRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }

        public string NotificationSentTo { get; set; }
        public string NotificationSentBy { get; set; }
        public string NotificationContent { get; set; }
        //public string NotificationTimeStamp { get; set; }
    }

    public class AddNotificationResponse : GeneralWebAPIResponse
    {

    }

    public class DelNotificationRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }

        public string UserId { get; set; }
        public string NotificationID { get; set; }
    }

    public class DelNotificationResponse : GeneralWebAPIResponse
    {

    }


    public class DelAllNotificationRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }

        public string UserId { get; set; }
    }

    public class DelAllNotificationResponse : GeneralWebAPIResponse
    {

    }


    public class MarkNotificationReadRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }

        public string NotificationID { get; set; }
    }


    public class MarkNotificationReadResponse : GeneralWebAPIResponse
    {

    }

    
}

using LungCare.SupportPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Models
{
    public class SendPhoneMessageRequest
    {
        public string Token { get; set; }

        public string Sender { get; set; }

        public string TextMessage { get; set; }
        public string UserId { get; set; }
    }

    public class SendPhoneMessageResponse : GeneralWebAPIResponse
    {

    }


}

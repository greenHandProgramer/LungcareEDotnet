using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Models
{
    public class GetUserNamesRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }
    }

    public class GetUserNamesResponse: GeneralWebAPIResponse
    {
        public List<string> UserNameList { get; set; }
    }
}

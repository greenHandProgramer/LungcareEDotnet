using System.Collections.Generic;

namespace LungCare.SupportPlatform.Models
{
    public class GetCertiWaitingApprovedUserNamesRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }
    }

    public class GetCertiWaitingApprovedUserNamesResponse : GeneralWebAPIResponse
    {
        public List<string> UserNameList { get; set; }
    }
}

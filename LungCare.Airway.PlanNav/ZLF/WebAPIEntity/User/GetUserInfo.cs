using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.Models
{
    public class GetUserInfoRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }
        public string UserId { get; set; }
    }

    public class GetUserInfoResponse: GeneralWebAPIResponse
    {
        public string PhoneNumber { get; set; }
        public string ChineseName { get; set; }
        public string Institution { get; set; }
        public string Department { get; set; }
        public string CertificateImage { get; set; }
        public string UserIconImage { get; set; }
        public string RegistrationTimeStamp { get; set; }
        public string LastLoginTimeStamp { get; set; }
    }



    public class GetAllUserInfoResponse : GeneralWebAPIResponse
    {
        public GetUserInfoResponse[] DataList { get; set; }
    }
}

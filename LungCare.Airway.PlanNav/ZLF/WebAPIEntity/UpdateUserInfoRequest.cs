using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.Models
{
    public class UpdateUserInfoRequest
    {
        public string Token { get; set; }
        public string PhoneNumber { get; set; }
        public string OldPassword { get; set; }
        public string ChineseName { get; set; }
        public string NewPassword { get; set; }
        public string Institution { get; set; }
        public string Department { get; set; }
        public string CertificateImage { get; set; }
        public string Sender { get; set; }
    }

    public class UpdateUserInfoResponse : GeneralWebAPIResponse
    {

    }
}

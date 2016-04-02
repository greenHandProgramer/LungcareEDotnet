using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Models
{
    public class UserInfo: GeneralWebAPIResponse
    {
        public string PhoneNumber { get; set; }
        public string ChineseName { get; set; }
        public string Institution { get; set; }
        public string Department { get; set; }
        //public string CertificateImage { get; set; }
        //public string UserIconImage { get; set; }
        //public string ErrorMsg { get; set; }

        //public string Success { get; set; }
    }
}

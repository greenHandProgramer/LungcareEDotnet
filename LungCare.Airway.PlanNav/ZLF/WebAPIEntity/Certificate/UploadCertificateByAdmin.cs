using LungCare.SupportPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Models
{
    class UploadCertificateByAdminRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }
        public string CertificateImage { get; set; }
        public string UserId { get; set; }
    }

    public class UploadCertificateByAdminResponse : GeneralWebAPIResponse
    {
    }
}

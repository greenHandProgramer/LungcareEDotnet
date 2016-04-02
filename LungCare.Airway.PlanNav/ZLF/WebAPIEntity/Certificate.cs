using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.Models
{
    public class UploadCertificateRequest
    {
        public string Token { get; set; }
        public string CertificateImage { get; set; }
        public string Sender { get; set; }
    }

    public class UploadCertificateResponse : GeneralWebAPIResponse
    {
     

    }

    public class DownloadCertificateRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }

    }

    public class DownloadCertificateRespnse : GeneralWebAPIResponse
    {
    
        public bool IsUploaded { get; set; }
        public string CertificateImage { get; set; }
    }
}

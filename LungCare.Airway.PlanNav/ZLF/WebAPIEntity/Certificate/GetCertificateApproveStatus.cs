using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.Models
{
    public class GetCertificateApproveStatusRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }
        public string UserId { get; set; }
    }

    public class GetCertificateApproveStatusResponse: GeneralWebAPIResponse
    {
        public bool Sucess { get; set; }
        public override bool Success
        {
            get
            {
                return Sucess;
            }
            set
            {
                Sucess = value;
            }
        }

        public string Result { get; set; }
        public string RejectReason { get; set; }
    }
}

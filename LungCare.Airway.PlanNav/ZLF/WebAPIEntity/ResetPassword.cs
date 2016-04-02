using LungCare.SupportPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.Models
{
    public class ResetPasswordByVerifyCodeRequest
    {
        public string UserName { get; set; }
        public string NewPassword { get; set; }
        public string VerifyCode { get; set; }
        public string Sender { get; set; }
    }


    public class ResetPasswordByVerifyCodeResponse : GeneralWebAPIResponse
    {

    }
}

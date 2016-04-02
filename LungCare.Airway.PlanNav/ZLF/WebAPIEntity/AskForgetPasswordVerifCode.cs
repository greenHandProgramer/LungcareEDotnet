using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.Models
{
    class AskForgetPasswordVerifCodeRequest
    {
        public string UserName { get; set; }
        public string Sender { get; set; }
    }


    class AskForgetPasswordVerifCodeResponse : GeneralWebAPIResponse
    {
    }
}

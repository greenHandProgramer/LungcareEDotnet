using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.Models
{
    public class UploadUserIconRequest
    {
        public string Token { get; set; }
        public string UserIconImage { get; set; }
        public string Sender { get; set; }
    }

    public class UploadUserIconResponse : GeneralWebAPIResponse
    {
        
    }

    public class DownloadUserIconRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }

    }

    public class DownloadUserIconRespnse : GeneralWebAPIResponse
    {
       
        public bool IsUploaded { get; set; }
        public string UserIconImage { get; set; }
    }
}

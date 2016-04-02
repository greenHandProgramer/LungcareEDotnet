using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.Models
{
    public class FeedbackRequest
    {
        public string Token { get; set; }
        
        public string Sender { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        //public string Submitter { get; set; }
    }

    public class FeedbackResponse : GeneralWebAPIResponse
    {

    }

  
    
}

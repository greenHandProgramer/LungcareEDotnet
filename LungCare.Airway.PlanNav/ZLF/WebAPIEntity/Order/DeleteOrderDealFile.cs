using LungCare.SupportPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Models
{
    public class DeleteOrderDealFileRequest
    {
        public string Token { get; set; }

        public string Sender { get; set; }

        public string DealFileName { get; set; }
        public string OrderId { get; set; }
    }

    public class DeleteOrderDealFileResponse : GeneralWebAPIResponse
    {

    }


}

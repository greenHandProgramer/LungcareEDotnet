using LungCare.SupportPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Models
{
    public class DeleteOrderDicomFileRequest
    {
        public string Token { get; set; }

        public string Sender { get; set; }

        public string DicomFileName { get; set; }
        public string OrderId { get; set; }
    }

    public class DeleteOrderDicomFileResponse : GeneralWebAPIResponse
    {

    }


}

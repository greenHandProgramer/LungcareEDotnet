using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.Models
{
    public class GeneralWebAPIResponse
    {
        public virtual bool Success { get; set; }
        public string ErrorMsg { get; set; }
    }
}

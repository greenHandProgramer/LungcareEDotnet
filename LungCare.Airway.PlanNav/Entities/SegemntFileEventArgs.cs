using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Entities
{
    public class SegemntFileEventArgs : EventArgs
    {
        public string airwayUnconnectiveVtpFileName { get; set; }
        public string airwayConnectiveVtpFileName { get; set; }
        public string lesionVTPFilename { get; set; }
    }
}

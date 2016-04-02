using LungCare.SupportPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Entities
{
    public class DataListItemArgs:EventArgs
    {
        public DataListItem DataListItem { get; set; }
    }
}

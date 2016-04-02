using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Entities
{
    public class WindowWidthLevelArgs:EventArgs
    {
        public int WindowWidth { get; set; }
        public int WindowLevel { get; set; }
        public int Index { get; set; }

        public System.Drawing.Drawing2D.Matrix Matrix{get;set;}
    }
}

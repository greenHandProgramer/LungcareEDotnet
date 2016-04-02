using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Entities
{
    public class DicomSnapshotEventArgs:EventArgs
    {
        public string studyUID { get; set; }
        public string seriesUID { get; set; }
        public int WindowWidth { get; set; }
        public int WindowLevel { get; set; }
        public int Index { get; set; }

        public System.Drawing.Drawing2D.Matrix Matrix { get; set; }
    }
}

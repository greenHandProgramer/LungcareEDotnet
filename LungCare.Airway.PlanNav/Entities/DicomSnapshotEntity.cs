using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Entities
{
    public class DicomSnapshotEntity
    {
        public string id { get; set; }
        public string BitmapPath { get; set; }
        public string studyUID { get; set; }
        public string seriesUID { get; set; }
        public int windowWidth { get; set; }
        public int windowsLevel { get; set; }
        public int index { get; set; }
        public System.Drawing.Drawing2D.Matrix matrix { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Entities
{
    class RectangleEntity
    {
        public Rectangle Rectagle { get; set; }
        public string studyUID { get; set; }
        public string seriesUID { get; set; }
        public int Frame { get; set; }
    }
}

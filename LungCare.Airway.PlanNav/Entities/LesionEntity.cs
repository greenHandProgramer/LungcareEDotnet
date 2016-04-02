using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Entities
{
    public class LesionEntity
    {
        public int Index { get; set; }
        public double[] position { get; set; }
        public string AxialCTImageFile { get; set; }
        public string AxialCTDetailImageFile { get; set; }
        public string Axial3DImageFile { get; set; }

        public string SagitalCTImageFile { get; set; }
        public string SagitalCTDetailImageFile { get; set; }
        public string Sagital3DImageFile { get; set; }

        public string CoronalCTImageFile { get; set; }
        public string CoronalCTDetailImageFile { get; set; }
        public string Coronal3DImageFile { get; set; }
    }
}

using LungCare.SupportPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LungCare.Airway.WinformUIControls.Entities
{
    class LineEntity
    {
        public LineEntity()
        {

        }


        public LineEntity(double[] dataSpacing, OrientationEnum oritation)
        {
            this.dataSpacing = dataSpacing;
            this.orientationEnum = oritation;
        }

        public Point startPoint { get; set; }
        public Point endPoint { get; set; }

        public int Index { get; set; }
        public double length 
        { 
            get
            {
                //return Math.Round(Math.Sqrt( Math.Pow(startPoint.X - endPoint.X , 2) + Math.Pow(startPoint.Y - endPoint.Y , 2)) , 2);
                return GetLength();
            }
            set
            {

            }
            
        }


        private double GetLength()
        {

            double x = 0 , y = 0;
            switch (orientationEnum)
            {
                case OrientationEnum.Sagittal:
                    x = (startPoint.X - endPoint.X)*dataSpacing[1];
                    y = (startPoint.Y - endPoint.Y)*dataSpacing[2];
                    break;
                case OrientationEnum.Coronal:
                    x = (startPoint.X - endPoint.X)*dataSpacing[0];
                    y = (startPoint.Y - endPoint.Y)*dataSpacing[2];
                    break;
                case OrientationEnum.Axial:
                    x = (startPoint.X - endPoint.X)*dataSpacing[0];
                    y = (startPoint.Y - endPoint.Y)*dataSpacing[1];
                    break;
                case OrientationEnum.Unknown:
                    break;
                default:
                    break;
            }


            return Math.Round(Math.Sqrt( Math.Pow(x , 2) + Math.Pow(y, 2)) , 2);
        }
        private double[] dataSpacing;

        private OrientationEnum orientationEnum;
        public string unit { get; set;}
        public string description { get; set; }
    }
}

using LungCare.SupportPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.SupportPlatformDAO.Utils
{
    class PositionOperation
    {
        public static double[] AxialCT_2_3D(double[] axialPositionCT, double[] PixelSpacing, int Width, int Height)
        {
            return new double[] { axialPositionCT[0] * PixelSpacing[0], (Width - axialPositionCT[1]) * PixelSpacing[1], axialPositionCT[2] * (PixelSpacing[2]) };
        }

        public static double[] Position_3D_2_Axial_CT(double[] position3D, double[] PixelSpacing, int Width, int Height)
        {
            return new double[] { position3D[0] / PixelSpacing[0], Width - position3D[1] / PixelSpacing[1], position3D[2] / (PixelSpacing[2]) };
        }


        public static double[] Position_3D_2_ConoralCT(double[] position3D, double[] PixelSpacing, int Width, int Height)
        {
            return new double[] { position3D[0] / PixelSpacing[0], position3D[2] / PixelSpacing[2], Width - position3D[1] / PixelSpacing[1] };
        }

        public static double[] ConoralCT_2_3D(double[] positionCoronal, double[] PixelSpacing, int Width, int Height)
        {
            return new double[] { positionCoronal[0] * PixelSpacing[0], (Width - positionCoronal[2]) * PixelSpacing[1], positionCoronal[1] * PixelSpacing[2] };
        }

        public static double[] Position_3D_2_SagitalCT(double[] position3D, double[] PixelSpacing, int Width, int Height)
        {
            return new double[] { Width - position3D[1] / PixelSpacing[1], position3D[2] / PixelSpacing[2], position3D[0] / PixelSpacing[0] };
        }

        public static double[] SagitalCT_2_3D(double[] positionSagital, double[] PixelSpacing, int Width, int Height)
        {
            return new double[] { positionSagital[2] * PixelSpacing[0], (Width - positionSagital[0]) * PixelSpacing[0], positionSagital[1] * PixelSpacing[2] };
        }

        public static double[] CT2D_2_3D(double[] point2D, OrientationEnum OrientationEnum, double[] PixelSpacing, int Width, int Height)
        {
            switch (OrientationEnum)
            {
                case OrientationEnum.Sagittal:
                    return SagitalCT_2_3D(point2D, PixelSpacing, Width, Height);
                case OrientationEnum.Coronal:
                    return ConoralCT_2_3D(point2D, PixelSpacing, Width, Height);
                case OrientationEnum.Axial:
                    return AxialCT_2_3D(point2D, PixelSpacing, Width, Height);
                case OrientationEnum.Unknown:
                    return null;
                default:
                    return null;
            }
        }


        public static double[] ThreeD_2_2D(double[] point3D, OrientationEnum OrientationEnum, double[] PixelSpacing, int Width, int Height)
        {
            switch (OrientationEnum)
            {
                case OrientationEnum.Sagittal:
                    return Position_3D_2_SagitalCT(point3D, PixelSpacing, Width, Height);
                case OrientationEnum.Coronal:
                    return Position_3D_2_ConoralCT(point3D, PixelSpacing, Width, Height);
                case OrientationEnum.Axial:
                    return Position_3D_2_Axial_CT(point3D, PixelSpacing, Width, Height);
                case OrientationEnum.Unknown:
                    return null;
                default:
                    return null;
            }
        }
    }
}

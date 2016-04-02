using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kitware.VTK;
using System.Runtime.InteropServices;
namespace LungCare_Airway_PlanNav.Control
{
    class MathOperation
    {
        public static double GetDistanceBetween3DPoints(double[] point1, double[] point2)
        {
            double distance = 0;
            for (int i = 0; i < 3; i++)
            {
                distance += Math.Pow((point1[i] - point2[i]), 2);
            }

            return Math.Sqrt(distance);
        }

        public static double[] Mutiple(vtkMatrix4x4 matrix, double[] value)
        {
            value = new double[] { value[0], value[1], value[2], 1 };
            int size = Marshal.SizeOf(typeof(double)) * value.Length;
            IntPtr pre = Marshal.AllocHGlobal(size);
            Marshal.Copy(value, 0, pre, value.Length);
            double[] result = matrix.MultiplyDoublePoint(pre);
            Marshal.FreeHGlobal(pre);
            return result;

        }
        public static double[] Mutiple(vtkTransform transform, double[] value)
        {
            vtkMatrix4x4 matrix = transform.GetMatrix();
            int size = Marshal.SizeOf(typeof(double)) * value.Length;
            IntPtr pre = Marshal.AllocHGlobal(size);
            Marshal.Copy(value, 0, pre, value.Length);
            double[] result = matrix.MultiplyDoublePoint(pre);

            return result;
        }

        public static double[] Multiply(vtkLandmarkTransform landMatrix, double[] value)
        {
            vtkMatrix4x4 matrix = landMatrix.GetMatrix();
            int size = Marshal.SizeOf(typeof(double)) * value.Length;
            IntPtr pre = Marshal.AllocHGlobal(size);
            Marshal.Copy(value, 0, pre, value.Length);
            double[] result = matrix.MultiplyDoublePoint(pre);
            Marshal.FreeHGlobal(pre);
            return result;
        }

        public static vtkMatrix4x4 GetTranform(double[] soucePoint1, double[] soucePoint2, double[] soucePoint3,
                                     double[] targetPoint1, double[] targetPoint2, double[] targetPoint3)
        {
            vtkPoints sourcePoints = new vtkPoints();
            sourcePoints.InsertNextPoint(soucePoint1[0], soucePoint1[1], soucePoint1[2]);
            sourcePoints.InsertNextPoint(soucePoint2[0], soucePoint2[1], soucePoint2[2]);
            sourcePoints.InsertNextPoint(soucePoint3[0], soucePoint3[1], soucePoint3[2]);

            vtkPoints targetPoints = new vtkPoints();
            targetPoints.InsertNextPoint(targetPoint1[0], targetPoint1[1], targetPoint1[2]);
            targetPoints.InsertNextPoint(targetPoint2[0], targetPoint2[1], targetPoint2[2]);
            targetPoints.InsertNextPoint(targetPoint3[0], targetPoint3[1], targetPoint3[2]);

            vtkLandmarkTransform landmark = new vtkLandmarkTransform();
            landmark.SetSourceLandmarks(sourcePoints);
            landmark.SetTargetLandmarks(targetPoints);
            landmark.Update();

            return landmark.GetMatrix();
        }

        public static vtkMatrix4x4 Convert(double[] elems)
        {
            vtkMatrix4x4 matrix = new vtkMatrix4x4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrix.SetElement(i, j, elems[i * 4 + j]);
                    Console.Write(elems[i * 4 + j]);
                }
                Console.WriteLine();
            }
            return matrix;
        }

        public static List<double[]> DeleteNearestPoint(List<double[]> listPoints, double[] selectedPoint)
        {
            double distance = 10000;
            int selectIndex = 0;
            for (int i = 0; i < listPoints.Count; i++)
            {
                double re = GetDistanceBetween3DPoints(selectedPoint, listPoints[i]);
                if (re < distance)
                {
                    distance = re;
                    selectIndex = i;
                }
            }
            List<double[]> result = new List<double[]>();
            if (distance < 5)
            {
                //for (int i = selectIndex; i < listPoints.Count - 1; i++)
                //{
                //    listPoints[i] = listPoints[i + 1];
                //}
                //listPoints.RemoveAt(listPoints.Count - 1);

                listPoints.RemoveAt(selectIndex);
            }
            return listPoints;

        }

        public static double[] GetNearestPoint(List<double[]> listPoints, double[] selectPoint, ref int n)
        {
            double distance = 10000;
            int selectIndex = 0;
            for (int i = 0; i < listPoints.Count; i++)
            {
                double re = GetDistanceBetween3DPoints(selectPoint, listPoints[i]);
                if (re < distance)
                {
                    distance = re;
                    selectIndex = i;

                }
            }
            if (distance < 10)
            {
                return listPoints[selectIndex];
                n = selectIndex;
            }
            else
            {
                return null;
            }
        }


        public static double[] GetNearestPoint(List<double[]> listPoints, double[] selectPoint)
        {
            double distance = 10000;
            int selectIndex = 0;
            for (int i = 0; i < listPoints.Count; i++)
            {
                double re = GetDistanceBetween3DPoints(selectPoint, listPoints[i]);
                if (re < distance)
                {
                    distance = re;
                    selectIndex = i;

                }
            }
            if (distance < 10)
            {
                return listPoints[selectIndex];
            }
            else
            {
                return null;
            }
        }


        public static void Subtract(double[] a, double[] b, ref double[] c)
        {
            c[0] = a[0] - b[0];
            c[1] = a[1] - b[1];
            c[2] = a[2] - b[2];
        }


        public static double Norm(double[] x)
        {
            return Math.Sqrt(x[0] * x[0] + x[1] * x[1] + x[2] * x[2]);
        }


        public static void Normalize(ref double[] x)
        {
            double length = Norm(x);
            x[0] /= length;
            x[1] /= length;
            x[2] /= length;
        }

        public static void Cross(double[] x, double[] y, ref double[] z)
        {
            z[0] = (x[1] * y[2]) - (x[2] * y[1]);
            z[1] = (x[2] * y[0]) - (x[0] * y[2]);
            z[2] = (x[0] * y[1]) - (x[1] * y[0]);
        }
    }
}

using System;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    /// <summary>
    ///     三维向量类
    /// </summary>
    public class Vector3
    {
        private const float E = 0.0000001f;
        public float[] vector;

        public Vector3(float x, float y, float z)
        {
            vector = new float[3] {x, y, z};
        }


        public Vector3(Vector3 vct)
        {
            vector = new float[3];


            vector[0] = vct.x;


            vector[1] = vct.y;


            vector[2] = vct.z;
        }


        public float x
        {
            get { return vector[0]; }


            set { vector[0] = value; }
        }


        public float y
        {
            get { return vector[1]; }


            set { vector[1] = value; }
        }


        public float z
        {
            get { return vector[2]; }


            set { vector[2] = value; }
        }

        public float X
        {
            get { return vector[0]; }


            set { vector[0] = value; }
        }


        public float Y
        {
            get { return vector[1]; }


            set { vector[1] = value; }
        }


        public float Z
        {
            get { return vector[2]; }


            set { vector[2] = value; }
        }


        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")";
        }


        public static Vector3 operator +(Vector3 lhs, Vector3 rhs) //向量加法
        {
            var result = new Vector3(lhs);


            result.x += rhs.x;


            result.y += rhs.y;


            result.z += rhs.z;


            return result;
        }


        public static Vector3 operator -(Vector3 lhs, Vector3 rhs) //向量减法
        {
            var result = new Vector3(lhs);


            result.x -= rhs.x;


            result.y -= rhs.y;


            result.z -= rhs.z;


            return result;
        }


        public static Vector3 operator /(Vector3 lhs, float rhs) //向量除以数量
        {
            if (rhs != 0)


                return new Vector3(lhs.x/rhs, lhs.y/rhs, lhs.z/rhs);


            return new Vector3(0, 0, 0);
        }


        public static Vector3 operator *(float lhs, Vector3 rhs) //左乘数量
        {
            return new Vector3(lhs*rhs.x, lhs*rhs.y, lhs*rhs.z);
        }


        public static Vector3 operator *(Vector3 lhs, float rhs) //右乘数量
        {
            return new Vector3(lhs.x*rhs, lhs.y*rhs, lhs.z*rhs);
        }


        public static float operator *(Vector3 lhs, Vector3 rhs) //向量数性积
        {
            return lhs.x*rhs.x + lhs.y*rhs.y + lhs.z*rhs.z;
        }


        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            if (Math.Abs(lhs.x - rhs.x) < E && Math.Abs(lhs.y - rhs.y) < E && Math.Abs(lhs.z - rhs.z) < E)


                return true;


            return false;
        }


        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return !(lhs == rhs);
        }


        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        /// <summary>
        ///     向量叉积，求与两向量垂直的向量
        /// </summary>
        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            var r = new Vector3(0, 0, 0);


            r.x = (v1.y*v2.z) - (v1.z*v2.y);


            r.y = (v1.z*v2.x) - (v1.x*v2.z);


            r.z = (v1.x*v2.y) - (v1.y*v2.x);


            return r;
        }


        /// <summary>
        ///     求向量长度
        /// </summary>
        public static float Magnitude(Vector3 v1)
        {
            return (float) Math.Sqrt((v1.x*v1.x) + (v1.y*v1.y) + (v1.z*v1.z));
        }

        public void Normalize()
        {
            Vector3 ret = Normalize(this);
            this.x = ret.x;
            this.y = ret.y;
            this.z = ret.z;
        }

        /// <summary>
        ///     单位化向量
        /// </summary>
        public static Vector3 Normalize(Vector3 v1)
        {
            float magnitude = Magnitude(v1);


            v1 = v1/magnitude;


            return v1;
        }
    }
}
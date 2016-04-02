#if NDIControl
using NDIControl;
#endif
using System;
using System.Collections.Generic;

namespace AirwayEMT配准结果生成器.NDIServer
{
    public class AuroraSensorData
    {
        public AuroraSensorData()
        {
            this.TimeTick = DateTime.Now.Ticks;
        }

        public bool Is6D
        {
            get
            {
                return HasSignal && Error != 0;
            }
        }

#if NDIControl
        public TransformData ToTransformData()
        {
            if (!HasSignal)
            {
                return null;
            }
            else
            {
                TransformData transformData = new TransformData { matrix = new double[4][] };

                transformData.matrix[0] = new double[4];
                transformData.matrix[1] = new double[4];
                transformData.matrix[2] = new double[4];
                transformData.matrix[3] = new double[4];

                transformData.matrix[0][0] = Matrix11;
                transformData.matrix[0][1] = Matrix12;
                transformData.matrix[0][2] = Matrix13;
                transformData.matrix[0][3] = Matrix14;

                transformData.matrix[1][0] = Matrix21;
                transformData.matrix[1][1] = Matrix22;
                transformData.matrix[1][2] = Matrix23;
                transformData.matrix[1][3] = Matrix24;

                transformData.matrix[2][0] = Matrix31;
                transformData.matrix[2][1] = Matrix32;
                transformData.matrix[2][2] = Matrix33;
                transformData.matrix[2][3] = Matrix34;

                transformData.matrix[3][0] = Matrix41;
                transformData.matrix[3][1] = Matrix42;
                transformData.matrix[3][2] = Matrix43;
                transformData.matrix[3][3] = Matrix44;

                transformData.translation = new double[]
                    {
                        Matrix14,
                        Matrix24,
                        Matrix34
                    };

                return transformData;
            }
        }

        public AuroraSensorData(TransformData transformData, int sensorIndex)
            : this()
        {
            this.TimeTick = DateTime.Now.Ticks;

            this.SensorIdx = sensorIndex;

            if (transformData == null)
            {
                HasSignal = false;
            }
            else
            {
                HasSignal = true;

                Matrix11 = transformData.matrix[0][0];
                Matrix12 = transformData.matrix[0][1];
                Matrix13 = transformData.matrix[0][2];
                Matrix14 = transformData.matrix[0][3];

                Matrix21 = transformData.matrix[1][0];
                Matrix22 = transformData.matrix[1][1];
                Matrix23 = transformData.matrix[1][2];
                Matrix24 = transformData.matrix[1][3];

                Matrix31 = transformData.matrix[2][0];
                Matrix32 = transformData.matrix[2][1];
                Matrix33 = transformData.matrix[2][2];
                Matrix34 = transformData.matrix[2][3];

                Matrix41 = transformData.matrix[3][0];
                Matrix42 = transformData.matrix[3][1];
                Matrix43 = transformData.matrix[3][2];
                Matrix44 = transformData.matrix[3][3];
            }
        }
#endif

        public override string ToString()
        {
            if (HasSignal)
            {
                string ret =  string.Format("{0} Handle:{1} Frame:{7} x:{3} y:{4} z:{5} Error:{6} Status:{8} {9}",
                    TimeStamp.ToLongTimeString(), 
                    SensorIdx,
                    HasSignal,
                    Matrix14.ToString("F1").PadLeft(6, ' '),
                    Matrix24.ToString("F1").PadLeft(6, ' '),
                    Matrix34.ToString("F1").PadLeft(6, ' '),
                    Error.ToString("F1").PadLeft(4, ' '), 
                    FrameNumber,
                    PortStatus,
                    Is6D ? "6D" : "5D");
                if (PartiallyOutOfVolume)
                {
                    ret += " PartiallyOutOfVolume";
                }

                return ret;
            }
            else
            {
                string ret = string.Format("{0} Handle:{1} Status:{2}", TimeStamp.ToLongTimeString(), SensorIdx, PortStatus);
                if (SensorCoilIsBroken)
                {
                    ret += " Sensor is broken!";
                }
                if (OutOfVolume)
                {
                    ret += " OutOfVolume!";
                }

                return ret;
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return new DateTime(TimeTick);
            }
        }

        public long TimeTick;

        public bool HasSignal;

        public int SensorIdx;

        public double Matrix11;
        public double Matrix12;
        public double Matrix13;
        public double Matrix14;

        public double Matrix21;
        public double Matrix22;
        public double Matrix23;
        public double Matrix24;

        public double Matrix31;
        public double Matrix32;
        public double Matrix33;
        public double Matrix34;

        public double Matrix41;
        public double Matrix42;
        public double Matrix43;
        public double Matrix44;

        public long FrameNumber;

        public double Error;

        public string PortStatus;

        public bool Occupied
        {
            get
            {
                return false;
            }
        }

        public bool GPIOLine1Closed
        {
            get
            {
                return false;
            }
        }

        public bool GPIOLine2Closed
        {
            get
            {
                return false;
            }
        }

        public bool GPIOLine3Closed
        {
            get
            {
                return false;
            }
        }

        public bool Initialized
        {
            get
            {
                return false;
            }
        }

        public bool Enabled
        {
            get
            {
                return false;
            }
        }

        public bool OutOfVolume
        {
            get
            {
                string bin = Convert.ToString(Convert.ToInt32(PortStatus, 16), 2).PadLeft(9, '0');
                //return bin
                return bin[2] == '1';
            }
        }

        public bool PartiallyOutOfVolume
        {
            get
            {
                string bin = Convert.ToString(Convert.ToInt32(PortStatus, 16), 2).PadLeft(9, '0');
                //return bin
                return bin[1] == '1';
            }
        }

        public bool SensorCoilIsBroken
        {
            get
            {
                string bin = Convert.ToString(Convert.ToInt32(PortStatus, 16), 2).PadLeft(9, '0');
                //return bin
                return bin[0] == '1';
            }
        }

        public static AuroraSensorData FromByteArray(byte[] byteArray)
        {
            AuroraSensorData ret = new AuroraSensorData();

            int startIndex = sizeof(int);

            ret.TimeTick = BitConverter.ToInt64(byteArray, startIndex);
            startIndex += sizeof(long);

            ret.HasSignal = BitConverter.ToBoolean(byteArray, startIndex);
            startIndex += sizeof(bool);

            ret.SensorIdx = BitConverter.ToInt32(byteArray, startIndex);
            startIndex += sizeof(int);

            ret.Matrix11 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix12 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix13 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix14 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);

            ret.Matrix21 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix22 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix23 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix24 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);

            ret.Matrix31 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix32 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix33 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix34 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);

            ret.Matrix41 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix42 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix43 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            ret.Matrix44 = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            
            ret.FrameNumber = BitConverter.ToInt32(byteArray, startIndex); startIndex += sizeof(long);
            ret.Error = BitConverter.ToDouble(byteArray, startIndex); startIndex += sizeof(double);
            
            return ret;
        }

        public byte[] FromByteArray()
        {
            //byte[] ret = new byte[sizeof(long) + sizeof(bool) + sizeof(int) * 17];
            var ret = new List<byte>();

            ret.AddRange(BitConverter.GetBytes(TimeTick));
            ret.AddRange(BitConverter.GetBytes(HasSignal));
            ret.AddRange(BitConverter.GetBytes(SensorIdx));

            ret.AddRange(BitConverter.GetBytes(Matrix11));
            ret.AddRange(BitConverter.GetBytes(Matrix12));
            ret.AddRange(BitConverter.GetBytes(Matrix13));
            ret.AddRange(BitConverter.GetBytes(Matrix14));

            ret.AddRange(BitConverter.GetBytes(Matrix21));
            ret.AddRange(BitConverter.GetBytes(Matrix22));
            ret.AddRange(BitConverter.GetBytes(Matrix23));
            ret.AddRange(BitConverter.GetBytes(Matrix24));

            ret.AddRange(BitConverter.GetBytes(Matrix31));
            ret.AddRange(BitConverter.GetBytes(Matrix32));
            ret.AddRange(BitConverter.GetBytes(Matrix33));
            ret.AddRange(BitConverter.GetBytes(Matrix34));

            ret.AddRange(BitConverter.GetBytes(Matrix41));
            ret.AddRange(BitConverter.GetBytes(Matrix42));
            ret.AddRange(BitConverter.GetBytes(Matrix43));
            ret.AddRange(BitConverter.GetBytes(Matrix44));

            ret.AddRange(BitConverter.GetBytes(FrameNumber));
            ret.AddRange(BitConverter.GetBytes(Error));
            
            ret.InsertRange(0, BitConverter.GetBytes(ret.Count));

            return ret.ToArray();
        }
    }
}

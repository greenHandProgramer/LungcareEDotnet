/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System;
using LungCare.SupportPlatform.SupportPlatformDAO.VTK;

namespace AirwayCT.Entity
{
    [DataContract(Name = "AirwayPatient")]
    public partial class AirwayPatient
    {
        bool passed;

        public bool SelfCheck()
        {
            passed = true;
            Console.ResetColor();
            Console.WriteLine(@"Checking " + this.Name);
            Console.Write("Checking existance of raw meta image file [{0}].", 吸气末期MhdFileName);
            if (File.Exists(吸气末期MhdFileName))
            {
                PrintOK();

                Console.Write("Checking raw meta image file[{0}] ends with [.mhd].", 吸气末期MhdFileName);
                if (吸气末期MhdFileName.EndsWith(".mhd"))
                {
                    PrintOK();
                }
                else
                {
                    PrintFailed();
                }

                Console.Write("Checking raw data image file[{0}] exists.", 吸气末期MhdFileName.ReplaceFileExtentionName("raw"));
                if (File.Exists(吸气末期MhdFileName.ReplaceFileExtentionName("raw")))
                {
                    PrintOK();
                }
                else
                {
                    PrintFailed();
                }
            }
            else
            {
                PrintFailed();
            }

            Console.Write("Checking 3D airway vtp file[{0}] exists.", AirwayVTP_FileName);
            if (File.Exists(AirwayVTP_FileName))
            {
                PrintOK();
            }
            else
            {
                PrintFailed();
            }

            Console.Write("Checking airway network vtp file[{0}] exists.", AirwayNetwork_VTP_FileName);
            if (File.Exists(AirwayNetwork_VTP_FileName))
            {
                PrintOK();

                FileInfo fi = new FileInfo(AirwayNetwork_VTP_FileName);
                Console.Write("Checking airway network [{0}] is not empty. Size = [{1}k]", AirwayNetwork_VTP_FileName, fi.Length / 1024);
                if (fi.Length > 3*1024)
                {
                    PrintOK();
                }
                else
                {
                    PrintFailed();
                }

            }
            else
            {
                PrintFailed();
            }

            Console.Write("Checking fixed airway network vtp file[{0}] exists.", AirwayNetwork_FIXED_VTP_FileName);
            if (File.Exists(AirwayNetwork_FIXED_VTP_FileName))
            {
                PrintOK();
            }
            else
            {
                PrintFailed();
            }
            
            Console.WriteLine("------------------------");

            return passed;
        }

        private void PrintOK()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[PASS]");
            Console.ResetColor();
        }
        private void PrintFailed()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[FAIL]");
            Console.ResetColor();
            passed = false;
        }

        internal void DeleteFiles()
        {
            DeleteFile(吸气末期MhdFileName);
            DeleteFile(AirwayVTP_FileName);
            DeleteFile(AirwayNetwork_VTP_FileName);
            DeleteFile(SegmentedMhd_FileName);
            DeleteFile(LungRegion_VTP);
            DeleteFile(LungRegion_MHD);
            DeleteDirectory(DicomFolder);
            DeleteDirectory(PatientDataFolder);
        }

        internal void DeleteDataProcessFiles()
        {
            DeleteFile(吸气末期MhdFileName);
            //DeleteFile(呼气末期MhdFileName);
            DeleteFile(AirwayVTP_FileName);
            //DeleteFile(BodyVTP_FileName);
            DeleteFile(AirwayNetwork_VTP_FileName);
            DeleteFile(SegmentedMhd_FileName);
            //DeleteFile(EnvelopeVTP_FileName);
            //DeleteFile(SpeedImagePath);
            DeleteFile(LungRegion_VTP);
            DeleteFile(LungRegion_MHD);
            //DeleteFile(Nav_Entity_FileName);

            DeleteDirectory(PatientDataFolder);
        }

        internal void DeleteFile(string filename)
        {
            Console.WriteLine(@"Deleting " + filename);
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Failed to delete file :" + filename);
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        internal void DeleteDirectory(string directoryName)
        {
            Console.WriteLine(@"Deleting " + directoryName);
            try
            {
                if (Directory.Exists(directoryName))
                {
                    Directory.Delete(directoryName, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Failed to delete directory :" + directoryName);
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        public static AirwayPatient Load(string filename)
        {
            var serializer = new DataContractSerializer(typeof(AirwayPatient));
            AirwayPatient o = null;
            using (var stream = new MemoryStream(File.ReadAllBytes(filename)))
            {
                o = serializer.ReadObject(stream) as AirwayPatient;
            }

            return o;
        }

        public void Save(string filename)
        {
            var serializer = new DataContractSerializer(typeof(AirwayPatient));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, this);
                File.WriteAllBytes(filename, stream.ToArray());
            }
        }

        public void Save()
        {
            AirwayPatients airwayPatients = AirwayPatients.Deserialize();
            for (int index = 0; index < airwayPatients.Count; index++)
            {
                AirwayPatient patient = airwayPatients[index];
                if (patient.PatientId == this.PatientId)
                {
                    airwayPatients[index] = this;
                }
            }
            AirwayPatients.Serialize(airwayPatients);
        }
        [DataMember(Name="OrderID")]
        public string OrderID { get; set; }

        [DataMember(Name = "Institution")]
        public string Institution { get; set; }

        [DataMember(Name = "PatientId")]
        public string PatientId { get; set; }

        [DataMember(Name = "入组编号")]
        public string 入组编号 { get; set; }

        [DataMember(Name = "中文名")]
        public string 中文名 { get; set; }

        [DataMember(Name = "组别")]
        public string 组别 { get; set; }

        [DataMember(Name = "Name")]
        public string Name { get; set; }

        [DataMember(Name = "Sex")]
        public string Sex { get; set; }

        [DataMember(Name = "Age")]
        public string Age { get; set; }

        [DataMember(Name = "CreateTime")]
        public DateTime CreateTime { get; set; }

        internal void PrefillFiledsAfterImportingCT()
        {
            string folder = Path.Combine(AirwayPatients.BaseFolder, PatientId);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            吸气末期MhdFileName = Path.Combine(folder, "i.mhd");
            AirwayVTP_FileName = Path.Combine(folder, "airway.vtp");
            AirwayNetwork_VTP_FileName = Path.Combine(folder, "network.vtp");
            SegmentedMhd_FileName = Path.Combine(folder, "segmented.mha");

            LungRegion_VTP = Path.Combine(folder, "lung_region.vtp");
            LungRegion_MHD = Path.Combine(folder, "lung_region.mha");

            CreateTime = DateTime.Now;
        }

        public string Airway3DScreenshotFileName
        {
            get { return GetFile("airway3DScreenshot.bmp"); }
        }

        public string XRay冠状位
        {
            get { return GetFile("xray.bmp"); }
        }
        public string XRay矢状位
        {
            get { return GetFile("xray_矢装位.bmp"); }
        }

        public string RegistrationPath
        {
            get { return GetFile("RegistrationPath.vtp"); }
        }

        public string NavRange
        {
            get { return GetFile("NavRange.vtp"); }
        }

        public string DataProcessFolder
        {
            get
            {
                FileInfo fi = new FileInfo(吸气末期MhdFileName);
                string directory = fi.Directory.FullName;

                return directory;
            }
        }
        public string GetSubFolder(string subFolderName)
        {
            FileInfo fi = new FileInfo(吸气末期MhdFileName);
            string directory = fi.Directory.FullName;

            return Path.Combine(directory, subFolderName);
        }

        public string GetFile(string filename)
        {
            FileInfo fi = new FileInfo(吸气末期MhdFileName);
            string directory = fi.Directory.FullName;

            return Path.Combine(directory, filename);
        }

        // 用他
        [DataMember(Name = "吸气末期MhdFileName")]
        [System.ComponentModel.Browsable(false)]
        public string 吸气末期MhdFileName { get; set; }

        [DataMember(Name = "AirwayVTP_FileName")]
        [System.ComponentModel.Browsable(false)]
        public string AirwayVTP_FileName { get; set; }

        [DataMember(Name = "AirwayNetwork_VTP_FileName")]
        [System.ComponentModel.Browsable(false)]
        public string AirwayNetwork_VTP_FileName { get; set; }
        public string AirwayNetwork_FIXED_VTP_FileName
        {
            get
            {
                return GetFile("network_fixed.vtp");
            }
        }

        public string Labeling_VTP_FileName
        {
            get
            {
                return GetFile("labeling.vtp");
            }
        }

        public string AirwayNetwork_FIXED_FIRST_VTP_FileName
        {
            get
            {
                if (File.Exists(AirwayNetwork_FIXED_VTP_FileName))
                {
                    return AirwayNetwork_FIXED_VTP_FileName;
                }
                else
                {
                    return AirwayNetwork_VTP_FileName;
                }
            }
        }

        public bool HasFixedAirwayNetwork
        {
            get
            {
                return File.Exists(AirwayNetwork_FIXED_VTP_FileName);
            }
        }

        [DataMember(Name = "SegmentedMhd_FileName")]
        [System.ComponentModel.Browsable(false)]
        public string SegmentedMhd_FileName { get; set; }

        [DataMember(Name = "DicomFolder")]
        [System.ComponentModel.Browsable(false)]
        public string DicomFolder { get; set; }

        [DataMember(Name = "LungRegion_VTP")]
        [System.ComponentModel.Browsable(false)]
        public string LungRegion_VTP { get; set; }

        [DataMember(Name = "LungRegion_MHD")]
        [System.ComponentModel.Browsable(false)]
        public string LungRegion_MHD { get; set; }

        [DataMember(Name = "ProximalPoint")]
        [System.ComponentModel.Browsable(false)]
        public double[] ProximalPoint { get; set; }

        #region LesionSizing
        #endregion
        private string PatientDataFolder
        {
            get { return new FileInfo(AirwayVTP_FileName).Directory.FullName; }
        }

        public AirwayPatient()
        {
        }

        internal bool IsCTReady { get { return true; } }
        internal bool IsMetaImageReady { get { return File.Exists(吸气末期MhdFileName); } }
        internal bool IsProximalPointReady { get { return ProximalPoint != null; } }
        //internal bool IsEndSliceReady { get { return EndIdx.HasValue; } }
        internal bool IsSegmentationReady { get { return File.Exists(this.SegmentedMhd_FileName); } }
        internal bool Is3DAirwayReady { get { return File.Exists(this.AirwayVTP_FileName); } }
        internal bool Is3DLungRegionReady { get { return File.Exists(this.LungRegion_VTP); } }
        internal bool Is3DNetworkReady { get { return File.Exists(this.AirwayNetwork_VTP_FileName); } }

        internal bool IsProcessed
        {
            get
            {
                return
                    IsMetaImageReady &&
                    IsSegmentationReady &&
                    Is3DAirwayReady &&
                    Is3DNetworkReady;
            }
        }

        public List<string> LesionMHDFileNames
        {
            get
            {
                List<string> ret = new List<string>();
                for (int i = 1; i <= 10; ++i)
                {
                    string lesionFileName = GetFile("lesion" + i + ".mhd");
                    if (File.Exists(lesionFileName))
                    {
                        Console.WriteLine(lesionFileName);

                        ret.Add(lesionFileName);
                    }
                }

                return ret;
            }
        }
        
        public bool HasNoPlannedPath
        {
            get
            {
                return !HasPlannedPath;
            }
        }

        public bool HasPlannedPath
        {
            get
            {
                return this.CenterlineFileNames.Count != 0;
            }
        }

        [XmlIgnore]
        public List<double[]> FirstPlannedPath
        {
            get
            {
                if (!HasPlannedPath)
                {
                    throw new Exception(string.Format("Patient {0} has not planned path.", Name));
                }

                return VTKUtil.ReadPolyDataPoints(CenterlineFileNames[0]);
            }
        }

        [XmlIgnore]
        public List<List<double[]>> AllPlannedPath
        {
            get
            {
                if (!HasPlannedPath)
                {
                    throw new Exception(string.Format("Patient {0} has not planned path.", Name));
                }

                List<List<double[]>> ret = new List<List<double[]>>();
                foreach (var item in CenterlineFileNames)
                {
                    ret.Add(VTKUtil.ReadPolyDataPoints(item));
                }

                return ret;
            }
        }

        public void RemovePath(int pathIdx)
        {
            int numberOfPlannedPathBak = NumberOfPlannedPath;

            File.Delete(GetColoredAirwayFileName(pathIdx));
            File.Delete(GetCenterlineFileName(pathIdx));
            File.Delete(GetLesionVTPFileName(pathIdx));
            File.Delete(GetLesionMhaFileName(pathIdx));

            for (int i = pathIdx; i < numberOfPlannedPathBak - 1; ++i)
            {
                MoveIfExist(GetColoredAirwayFileName(i + 1), GetColoredAirwayFileName(i));
                MoveIfExist(GetCenterlineFileName(i + 1), GetCenterlineFileName(i));
                MoveIfExist(GetLesionVTPFileName(i + 1), GetLesionVTPFileName(i));
                MoveIfExist(GetLesionMhaFileName(i + 1), GetLesionMhaFileName(i));
            }
        }

        private void MoveIfExist(string src,string dst)
        {
            if (File.Exists(src))
            {
                File.Move(src, dst);
            }
        }

        public int NumberOfPlannedPath
        {
            get
            {
                return CenterlineFileNames.Count;
            }
        }

        public List<double[]> GetPlannedPath(int idx)
        {
            return VTKUtil.ReadPolyDataPoints(CenterlineFileNames[idx]);
        }

        public List<string> ColoredAirwayFileNames
        {
            get
            {
                List<string> ret = new List<string>();
                for (int i = 0; i <= 10; ++i)
                {
                    string lesionFileName = GetColoredAirwayFileName(i);
                    if (File.Exists(lesionFileName))
                    {
                        //Console.WriteLine(lesionFileName);
                        ret.Add(lesionFileName);
                    }
                }

                return ret;
            }
        }
        public string GetColoredPathLabelingFileName(int lesionIdx)
        {
            return GetFile("ColorPathAirway." + lesionIdx + ".vtp");
        }

        public string GetColoredAirwayFileName(int lesionIdx)
        {
            return GetFile("ColorAirway." + lesionIdx + ".vtp");
        }

        public string GetColoredAirwayRestFileName(int lesionIdx)
        {
            return GetFile("ColorAirwayRest." + lesionIdx + ".vtp");
        }

        public List<string> CenterlineFileNames
        {
            get
            {
                List<string> ret = new List<string>();
                for (int i = 0; i <= 10; ++i)
                {
                    string lesionFileName = GetCenterlineFileName(i);
                    if (File.Exists(lesionFileName))
                    {
                        ret.Add(lesionFileName);
                    }
                    else
                    {
                        break;
                    }
                }

                return ret;
            }
        }
        
        public string SegmentMarkBin
        {
            get { return GetFile("mark.bin"); }
        }

        public string AirwayPolyDataCell2NetworkCellMappingFileName
        {
            get { return GetFile("AirwayPolyDataCell2NetworkCellMapping.bin"); }
        }

        public string GetCenterlineFileName(int lesionIdx)
        {
            return GetFile("Centerline." + lesionIdx + ".vtp");
        }

        public List<string> LesionVTPFileNames
        {
            get
            {
                List<string> ret = new List<string>();
                for (int i = 0; i <= 10; ++i)
                {
                    string lesionFileName = GetLesionVTPFileName(i);
                    if (File.Exists(lesionFileName))
                    {
                        ret.Add(lesionFileName);
                    }
                }

                return ret;
            }
        }
        public string GetFixedLesionVTPFileName(int lesionIdx)
        {
            return GetFile("Lesion." + lesionIdx + ".fixed.vtp");
        }
        
        public string GetLesionVTPFileName(int lesionIdx)
        {
            return GetFile("Lesion." + lesionIdx + ".vtp");
        }
        public List<string> LesionMhaFileNames
        {
            get
            {
                List<string> ret = new List<string>();
                for (int i = 0; i <= 10; ++i)
                {
                    string lesionFileName = GetLesionMhaFileName(i);
                    if (File.Exists(lesionFileName))
                    {
                        ret.Add(lesionFileName);
                    }
                }

                return ret;
            }
        }

        public string GetBiPoints(int lesionIdx)
        {
            return GetFile(string.Format("biPoints.{0}.txt", lesionIdx));
        }

        public string GetLesionMhaFileName(int lesionIdx)
        {
            return GetFile("Lesion." + lesionIdx + ".mha");
        }

        public bool IsLabelingOutDated
        {
            get
            {
                if (!File.Exists(Labeling_VTP_FileName))
                {
                    return true;
                }
                DateTime segmentedMHDLastWriteTime = new FileInfo(this.SegmentedMhd_FileName).LastWriteTime;
                DateTime labelingLastWriteTime = new FileInfo(this.Labeling_VTP_FileName).LastWriteTime;

                bool r = labelingLastWriteTime < segmentedMHDLastWriteTime;
                return r;
            }
        }
        public bool IsAirwayPolyDataCell2NetworkCellMappingOutDated
        {
            get
            {
                if (!File.Exists(AirwayPolyDataCell2NetworkCellMappingFileName))
                {
                    return true;
                }
                DateTime segmentedMHDLastWriteTime = new FileInfo(this.SegmentedMhd_FileName).LastWriteTime;
                DateTime airwayPolyDataCell2NetworkCellMappingLastWriteTime = new FileInfo(this.AirwayPolyDataCell2NetworkCellMappingFileName).LastWriteTime;

                bool r = airwayPolyDataCell2NetworkCellMappingLastWriteTime < segmentedMHDLastWriteTime;
                return r;
            }
        }
        public bool IsSegmentMarkBinOutDated
        {
            get
            {
                if (!File.Exists(SegmentMarkBin))
                {
                    return true;
                }
                DateTime segmentedMHDLastWriteTime = new FileInfo(this.SegmentedMhd_FileName).LastWriteTime;
                DateTime SegmentMarkBinLastWriteTime = new FileInfo(this.SegmentMarkBin).LastWriteTime;

                bool r = SegmentMarkBinLastWriteTime < segmentedMHDLastWriteTime;
                return r;
            }
        }

        public bool IsAirwayVTPOutDated
        {
            get
            {
                if (!File.Exists(AirwayVTP_FileName))
                {
                    return true;
                }
                DateTime segmentedMHDLastWriteTime = new FileInfo(this.SegmentedMhd_FileName).LastWriteTime;
                DateTime mediansegmentedMHDLastWriteTime = new FileInfo(GetFile("raw_median3D.mha").FileExists() ? GetFile("raw_median3D.mha") : this.SegmentedMhd_FileName).LastWriteTime;
                
                DateTime airwayVTPLastWriteTime = new FileInfo(this.AirwayVTP_FileName).LastWriteTime;

                bool r = airwayVTPLastWriteTime < segmentedMHDLastWriteTime || airwayVTPLastWriteTime < mediansegmentedMHDLastWriteTime;
                return r;
            }
        }

        [DataMember(Name = "NotRequireLabeling")]
        public bool NotRequireLabeling { get; set; }

        public bool IsNetworkOutDated
        {
            get
            {
                if (!File.Exists(AirwayVTP_FileName))
                {
                    return true;
                }
                if (!File.Exists(AirwayNetwork_FIXED_VTP_FileName))
                {
                    return true;
                }
                if (!File.Exists(AirwayNetwork_VTP_FileName))
                {
                    return true;
                }

                DateTime networkLastWriteTime = new FileInfo(this.AirwayNetwork_VTP_FileName).LastWriteTime;
                DateTime fixednetworkLastWriteTime = new FileInfo(this.AirwayNetwork_FIXED_FIRST_VTP_FileName).LastWriteTime;
                DateTime airwayVTPLastWriteTime = new FileInfo(this.AirwayVTP_FileName).LastWriteTime;

                return networkLastWriteTime < airwayVTPLastWriteTime || fixednetworkLastWriteTime < airwayVTPLastWriteTime;
            }
        }
    }

    [DataContract(Name = "MarkerCameraEntity")]
    public class MarkerCameraEntity
    {
        [DataMember(Name = "Color")]
        public int[] Color { get; set; }

        [DataMember(Name = "MarkerPosition")]
        public double[] MarkerPosition { get; set; }
    }

    [DataContract(Name = "RegistartionSinglePathEntity")]
    public class RegistartionSinglePathEntity
    {
        [DataMember(Name = "Points")]
        public List<double[]> Points { get; set; }

        [DataMember(Name = "Color")]
        public double[] Color { get; set; }
    }
}

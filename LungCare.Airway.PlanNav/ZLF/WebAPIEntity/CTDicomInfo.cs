using System;

namespace LungCare.SupportPlatform.Models
{
    public class   CTDicomInfo
    {
        public string PatientName { get; set; }
        public string PatientSex { get; set; }
        public string PatientAge { get; set; }
        public string InstitutionName { get; set; }
        public string AcquisitionDate { get; set; }
        public string AcquisitionTime { get; set; }
        public string StudyInstanceUID { get; set; }
        public string SeriesInstanceUID { get; set; }
        public string Status { get; set; }
        public string AdditionalData { get; set; }
        public string FileName { get; set; }
        public string InstanceNumber { get; set; }
        public double SliceThickness { get; set; }
        public string DataID { get; set; }
        public string patientPosition { get; set; }
        public DateTime UploadTimestamp { get; set; }
    }


    public class CTDicomInfo1
    {
        public string PatientName { get; set; }
        public string PatientSex { get; set; }
        public string PatientAge { get; set; }
        public string InstitutionName { get; set; }
        public string AcquisitionDate { get; set; }
        public string AcquisitionTime { get; set; }
        public DateTime UploadTimestamp { get; set; }
    }

}

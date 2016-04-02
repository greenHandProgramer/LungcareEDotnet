using System;

namespace LungCare.SupportPlatform.Models
{
    public class UpdateDataRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }
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
        public string DataID { get; set; }
        public string UploadTimestamp { get; set; }
    }
}

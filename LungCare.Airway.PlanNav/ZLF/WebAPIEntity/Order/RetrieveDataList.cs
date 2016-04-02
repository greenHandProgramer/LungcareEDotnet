using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LungCare.SupportPlatform.Models
{
    public class RetrieveDataListRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }
        public string UserId { get; set; }
    }

    public class RetrieveDataListResponse : GeneralWebAPIResponse
    {
        public DataListItem[] DataList { get; set; }
    }

    public class DataListItem
    {
        public string UserId { get; set; }
        public string ChineseName { get; set; }
        public string DataID { get; set; }
        public string Status { get; set; }
        public string UploadTimestamp { get; set; }

        public string UploadTimestampHumanReadable {
            get
            {
                try
                {
                    return DateTime.ParseExact(this.UploadTimestamp, "yyyyMMdd HHmmss", null).ToString("yyyy年MM月dd日 HH:mm:ss");
                }
                catch
                {
                    return "未知";
                }
            }
        }

        public string AdditionalData { get; set; }
        public string PatientName { get; set; }
        public string PatientSex { get; set; }
        public string PatientAge { get; set; }
        public string InstitutionName { get; set; }
        public string AcquisitionDate { get; set; }
        public string AcquisitionTime { get; set; }
        public string StudyInstanceUID { get; set; }
        public string SeriesInstanceUID { get; set; }

        public string CTPackageFileID { get; set; }
        public string PostProcessFileID { get; set; }

        public string ExportAirwayPackage { get; set; }
        public string AirwayStatus { get; set; }
        public string LocalDicom { get; set; }
    }
}

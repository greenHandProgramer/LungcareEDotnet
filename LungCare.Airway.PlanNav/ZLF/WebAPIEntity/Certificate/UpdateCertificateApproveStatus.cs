namespace LungCare.SupportPlatform.Models
{
    public class UpdateCertificateApproveStatusRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }
        public bool Result { get; set; }
        public string RejectReason { get; set; }
        public string UserId { get; set; }
    }

    public class UpdateCertificateApproveStatusResponse : GeneralWebAPIResponse
    {
    }
}

namespace LungCare.SupportPlatform.Models
{
    public class SignupRequest
    {
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string ChineseName { get; set; }
        public string Institution { get; set; }
        public string Department { get; set; }
        public string CertificateImage { get; set; }
        public string Sender { get; set; }

        public string VerifyCode { get; set; }
    }

    public class SignupResponse : GeneralWebAPIResponse
    {
        //public bool Success { get; set; }
        //public string ErrorMsg { get; set; }
    }
}

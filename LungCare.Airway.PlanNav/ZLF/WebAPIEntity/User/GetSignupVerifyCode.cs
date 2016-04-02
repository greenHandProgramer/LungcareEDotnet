namespace LungCare.SupportPlatform.Models
{
    public class GetSignupVerifyCodeRequest
    {
        public string PhoneNumber { get; set; }
        public string Sender { get; set; }
    }

    public class GetSignupVerifyCodeResponse : GeneralWebAPIResponse
    {
    }
}

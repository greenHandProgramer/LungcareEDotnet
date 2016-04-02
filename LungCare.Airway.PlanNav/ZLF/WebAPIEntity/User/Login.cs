namespace LungCare.SupportPlatform.Models
{
    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Sender { get; set; }
    }

    public class LoginResponse: GeneralWebAPIResponse
    {
        public string Token { get; set; }
    }
}

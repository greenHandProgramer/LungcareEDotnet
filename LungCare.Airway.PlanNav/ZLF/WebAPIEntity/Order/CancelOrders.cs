namespace LungCare.SupportPlatform.Models
{
    public class CancelOrdersRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }
        public string UserId { get; set; }
    }

    public class CancelOrdersResponse: GeneralWebAPIResponse
    {
    }
}

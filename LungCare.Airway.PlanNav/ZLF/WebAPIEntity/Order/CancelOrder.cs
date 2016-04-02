namespace LungCare.SupportPlatform.Models
{
    public class CancelOrderRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }

        public string OrderId { get; set; }
    }

    public class CancelOrderResponse: GeneralWebAPIResponse
    {
    }
}

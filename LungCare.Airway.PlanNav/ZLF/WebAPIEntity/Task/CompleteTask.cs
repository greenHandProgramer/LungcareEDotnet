namespace LungCare.SupportPlatform.Models
{
    public class CompleteTaskRequest
    {
        public string Token { get; set; }
        public string Sender { get; set; }
        public string DataID { get; set; }
        public string Operation { get; set; }
    }

    public class CompleteTaskResponse: GeneralWebAPIResponse
    {
    }
}

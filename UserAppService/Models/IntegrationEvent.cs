namespace UserAppService.Models
{
    public class IntegrationEvent
    {
        public int ID { get; set; }
        public required string Event { get; set; }
        public required string Data { get; set; }
    }
}

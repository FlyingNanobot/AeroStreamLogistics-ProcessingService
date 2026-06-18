namespace ProcessingService.Models
{
    public class FinanceEvent
    {
        public int Id { get; set; } // DB-generated
        public string FlightId { get; set; }
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public string Payload { get; set; } // JSONB
    }

}

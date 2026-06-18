namespace ProcessingService.Models
{
    public class TelemetryHistoryDocument
    {
        public string FlightId { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }
        public double Speed { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

namespace ProcessingService.Models
{
    public class LiveFlightState
    {
        public string FlightId { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }
        public double Speed { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

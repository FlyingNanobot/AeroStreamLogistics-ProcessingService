namespace ProcessingService.Models
{
    public class ProcessedState
    {
        public string FlightId { get; set; }   // map from Icao24
        public string Callsign { get; set; }
        public string OriginCountry { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }
        public double Speed { get; set; }
        public DateTime Timestamp { get; set; }
    }

}

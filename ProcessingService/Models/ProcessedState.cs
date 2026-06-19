namespace ProcessingService.Models
{
    /// <summary>
    /// Normalized domain model representing the processed flight state that is persisted
    /// to various backends (cache, index, relational store).
    /// </summary>
    public class ProcessedState
    {
        /// <summary>Unique flight identifier (ICAO 24-bit address).</summary>
        public string? FlightId { get; set; }   // map from Icao24

        /// <summary>Human-readable callsign if available.</summary>
        public string? Callsign { get; set; }

        /// <summary>Declared origin country of the aircraft.</summary>
        public string? OriginCountry { get; set; }

        /// <summary>Latitude in decimal degrees.</summary>
        public double Lat { get; set; }

        /// <summary>Longitude in decimal degrees.</summary>
        public double Lon { get; set; }

        /// <summary>Altitude in meters (geo preferred, fallback to baro).</summary>
        public double Alt { get; set; }

        /// <summary>Speed in meters per second.</summary>
        public double Speed { get; set; }

        /// <summary>Processing timestamp in UTC.</summary>
        public DateTime Timestamp { get; set; }
    }
}

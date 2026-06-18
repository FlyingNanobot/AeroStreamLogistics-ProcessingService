namespace ProcessingService.Models
{
    /// <summary>
    /// Lightweight representation of incoming telemetry messages deserialized from Kafka.
    /// Fields mirror the external telemetry schema; nullables are used when telemetry data
    /// may be absent.
    /// </summary>
    public class TelemetryMessage
    {
        public string Icao24 { get; set; }
        public string Callsign { get; set; }
        public string OriginCountry { get; set; }
        public long? TimePosition { get; set; }
        public long? LastContact { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public double? BaroAltitude { get; set; }
        public bool OnGround { get; set; }
        public double? Velocity { get; set; }
        public double? TrueTrack { get; set; }
        public double? VerticalRate { get; set; }
        public int? Sensors { get; set; }
        public double? GeoAltitude { get; set; }
        public string Squawk { get; set; }
        public bool Spi { get; set; }
        public int PositionSource { get; set; }
    }
}

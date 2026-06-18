using ProcessingService.Models;
using System.Text.Json;

namespace ProcessingService.Services
{
    public class TelemetryProcessor
    {
        private readonly RedisWriter _redis;
        private readonly OpenSearchWriter _os;
        private readonly PostgresWriter _pg;
        private readonly ArchiveWriter _archive;
        private readonly ILogger<TelemetryProcessor> _logger;

        public TelemetryProcessor(
            RedisWriter redis,
            OpenSearchWriter os,
            PostgresWriter pg,
            ArchiveWriter archive,
            ILogger<TelemetryProcessor> logger)
        {
            _redis = redis;
            _os = os;
            _pg = pg;
            _archive = archive;
            _logger = logger;
        }

        public async Task ProcessAsync(string rawJson)
        {
            // Archive raw
            //await _archive.AppendRawAsync(rawJson);

            var msg = JsonSerializer.Deserialize<TelemetryMessage>(rawJson);
            if (msg == null) return;

            var processed = new ProcessedState
            {
                FlightId = msg.Icao24,
                Callsign = msg.Callsign?.Trim(),
                OriginCountry = msg.OriginCountry,
                Lat = msg.Latitude ?? 0,
                Lon = msg.Longitude ?? 0,
                Alt = msg.GeoAltitude ?? msg.BaroAltitude ?? 0,
                Speed = msg.Velocity ?? 0,
                Timestamp = DateTime.UtcNow
            };

            // Live UI
            await _redis.WriteAsync(processed);


            // History
            //await _os.IndexAsync(processed);

            // Finance events (example)
            //if (processed.Speed > 900)
            //{
            //    await _pg.InsertEventAsync(new FinanceEvent
            //    {
            //        FlightId = processed.FlightId,
            //        EventType = "SPEED_ANOMALY",
            //        Timestamp = processed.Timestamp,
            //        Payload = JsonSerializer.Serialize(processed)
            //    });
            //}
        }
    }
}

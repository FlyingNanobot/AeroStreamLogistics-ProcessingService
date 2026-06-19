using ProcessingService.Models;
using System.Text.Json;

namespace ProcessingService.Services
{
    /// <summary>
    /// Processes raw telemetry JSON payloads into domain objects and routes them to writers.
    ///
    /// Theory: The processor is intentionally simple: deserialize, normalize fields, and fan-out
    /// to multiple sinks (live cache, history index, relational store). Complex business rules,
    /// deduplication, or enrichment should live here; I/O and persistence are delegated to writer
    /// components to keep responsibilities separated and testable.
    /// </summary>
    public class TelemetryProcessor
    {
        private readonly RedisWriter _redis;
        private readonly OpenSearchWriter _os;
        private readonly PostgresWriter _pg;
        private readonly ArchiveWriter _archive;
        private readonly ILogger<TelemetryProcessor> _logger;

        /// <summary>
        /// Initializes the processor with the writers it will use to persist data.
        /// </summary>
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

        /// <summary>
        /// Process a raw telemetry JSON payload. This method deserializes the message,
        /// constructs a normalized ProcessedState object, and forwards it to registered writers.
        /// </summary>
        /// <param name="rawJson">Raw JSON string received from Kafka.</param>
        public async Task ProcessAsync(string rawJson)
        {
            // Optionally archive raw payloads for forensic analysis or reprocessing.
            await _archive.AppendRawAsync(rawJson);

            // Deserialize into the lightweight telemetry model. If the message cannot be
            // parsed, drop it and move on to avoid blocking the pipeline.
            var msg = JsonSerializer.Deserialize<TelemetryMessage>(rawJson);
            if (msg == null) return;

            // Normalize fields and construct the processed domain model. Use coalescing to
            // provide sensible defaults when telemetry fields are absent.
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

            // Live UI cache: low-latency write to Redis for dashboards and quick lookup.
            await _redis.WriteAsync(processed);

            // History indexing and finance/event generation are optional and may be enabled
            // depending on the deployment requirements. Keep these operations async to avoid
            // blocking the critical live-path write.
            await _os.IndexAsync(processed);

            // Example finance event generation based on business rules.
            if (processed.Speed > 900)
            {
                await _pg.InsertEventAsync(new FinanceEvent
                {
                    FlightId = processed.FlightId,
                    EventType = "SPEED_ANOMALY",
                    Timestamp = processed.Timestamp,
                    Payload = JsonSerializer.Serialize(processed)
                });
            }
        }
    }
}

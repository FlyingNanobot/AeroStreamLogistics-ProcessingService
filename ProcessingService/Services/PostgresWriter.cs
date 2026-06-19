using Npgsql;
using ProcessingService.Models;

namespace ProcessingService.Services
{
    public class PostgresWriter
    {
        private readonly string _connString;
        private readonly ILogger<PostgresWriter> _logger;

        public PostgresWriter(IConfiguration config, ILogger<PostgresWriter> logger)
        {
            _connString = config["Postgres:ConnectionString"];
            _logger = logger;
        }

        public async Task WriteAsync(ProcessedState state)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO processed_state 
                    (flight_id, callsign, origin_country, lat, lon, alt, speed, timestamp) 
                    VALUES (@f, @c, @o, @lat, @lon, @alt, @speed, @ts)";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("f", state.FlightId ?? "");
            cmd.Parameters.AddWithValue("c", state.Callsign ?? "");
            cmd.Parameters.AddWithValue("o", state.OriginCountry ?? "");
            cmd.Parameters.AddWithValue("lat", state.Lat);
            cmd.Parameters.AddWithValue("lon", state.Lon);
            cmd.Parameters.AddWithValue("alt", state.Alt);
            cmd.Parameters.AddWithValue("speed", state.Speed);
            cmd.Parameters.AddWithValue("ts", state.Timestamp);

            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert telemetry into Postgres");
            }
        }
    }

}

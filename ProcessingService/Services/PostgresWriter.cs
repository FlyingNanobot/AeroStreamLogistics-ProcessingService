using Npgsql;
using ProcessingService.Models;

namespace ProcessingService.Services
{
    public class PostgresWriter
    {
        private readonly string _connectionString;
        private readonly ILogger<PostgresWriter> _logger;

        public PostgresWriter(IConfiguration config, ILogger<PostgresWriter> logger)
        {
            _connectionString = config.GetConnectionString("Postgres");
            _logger = logger;
        }

        public async Task InsertEventAsync(FinanceEvent evt)
        {
            const string sql = @"
            INSERT INTO finance_events (flight_id, event_type, timestamp, payload)
            VALUES (@flight_id, @event_type, @timestamp, @payload);
        ";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("flight_id", evt.FlightId);
            cmd.Parameters.AddWithValue("event_type", evt.EventType);
            cmd.Parameters.AddWithValue("timestamp", evt.Timestamp);
            cmd.Parameters.AddWithValue("payload", evt.Payload);

            await cmd.ExecuteNonQueryAsync();
        }
    }

}

using ProcessingService.Models;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace ProcessingService.Services
{
    /// <summary>
    /// Responsible for writing live flight state to Redis.
    ///
    /// Theory: Redis is used as a fast in-memory cache for recent/active flight state. The writer stores
    /// a serialized snapshot keyed by flight id with a short TTL to prevent stale data accumulation.
    /// Writes are best-effort and optimized for speed; idempotency and ordering are handled by higher
    /// layers (TelemetryProcessor / Kafka consumer) when required.
    /// </summary>
    public class RedisWriter
    {
        private readonly IDatabase _db;

        /// <summary>
        /// Create a new RedisWriter using the provided connection multiplexer.
        /// </summary>
        /// <param name="redis">An active ConnectionMultiplexer used to obtain an IDatabase instance.</param>
        public RedisWriter(IConnectionMultiplexer redis)
        {
            // Keep a reference to the IDatabase which is a lightweight, thread-safe object.
            _db = redis.GetDatabase();
        }

        /// <summary>
        /// Persist the processed flight state into Redis as a JSON string.
        /// </summary>
        /// <param name="state">The processed state that will be serialized and written.</param>
        /// <returns>A Task that completes when the write to Redis has been queued/completed.</returns>
        /// <remarks>
        /// Implementation notes:
        /// - The state is serialized using System.Text.Json for compact representation.
        /// - A TTL is applied (10 minutes) to keep cache size bounded and allow automatic expiration of inactive flights.
        /// - This method returns the underlying Redis Task directly to avoid extra allocations; callers should
        ///   observe and await the returned Task to know when the operation completes.
        /// - If stronger consistency is required (compare-and-swap, versioned updates), add optimistic concurrency
        ///   controls here (e.g., using Lua scripts or WATCH/MULTI), but that increases latency.
        /// </remarks>
        public Task WriteAsync(ProcessedState state)
        {
            // Key design: prefix to avoid collisions and make it easy to scan by pattern if necessary.
            string key = $"flight:{state.FlightId}";

            // Serialize the processed state. Using System.Text.Json gives a fast, allocation-friendly serializer.
            string json = JsonSerializer.Serialize(state);

            // Store with a TTL so stale data expires automatically. Choose TTL to balance cache freshness and write frequency.
            return _db.StringSetAsync(key, json, expiry: TimeSpan.FromMinutes(10));
        }
    }

}

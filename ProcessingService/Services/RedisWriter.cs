using ProcessingService.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace ProcessingService.Services
{
    public class RedisWriter
    {
        private readonly IDatabase _db;

        public RedisWriter(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public Task WriteAsync(ProcessedState state)
        {
            string key = $"flight:{state.FlightId}";
            string json = JsonSerializer.Serialize(state);

            return _db.StringSetAsync(key, json, expiry: TimeSpan.FromMinutes(10));
        }
    }

}

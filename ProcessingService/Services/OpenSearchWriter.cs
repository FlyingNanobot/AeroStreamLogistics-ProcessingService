using OpenSearch.Client;
using ProcessingService.Models;

namespace ProcessingService.Services
{
    public class OpenSearchWriter
    {
        private readonly IOpenSearchClient _client;
        private readonly ILogger<OpenSearchWriter> _logger;

        public OpenSearchWriter(IOpenSearchClient client, ILogger<OpenSearchWriter> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task IndexAsync(ProcessedState state)
        {
            var response = await _client.IndexAsync(state, i => i
                .Index("telemetry-history")
                .Id($"{state.FlightId}-{state.Timestamp:yyyyMMdd-HHmmssfff}")
            );

            if (!response.IsValid)
                _logger.LogError("OpenSearch index failed: {Error}", response.OriginalException.Message);
        }
    }
}

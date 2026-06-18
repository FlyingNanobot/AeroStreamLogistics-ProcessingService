using Confluent.Kafka;

namespace ProcessingService.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly TelemetryProcessor _processor;
        private readonly ILogger<KafkaConsumerService> _logger;

        public KafkaConsumerService(
            TelemetryProcessor processor,
            ILogger<KafkaConsumerService> logger,
            IConfiguration config)
        {
            _processor = processor;
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = "processing-service",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnablePartitionEof = true
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _consumer.Subscribe(config["Kafka:Topic"]);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kafka Consumer started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);

                    if (result == null || result.IsPartitionEOF)
                        continue;

                    await _processor.ProcessAsync(result.Message.Value);

                    _consumer.Commit(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming message");
                }
            }
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }

}

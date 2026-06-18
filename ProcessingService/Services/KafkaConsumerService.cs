using Confluent.Kafka;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ProcessingService.Services
{
    /// <summary>
    /// Background service that consumes messages from Kafka and forwards them to the processing pipeline.
    ///
    /// Theory: The consumer uses manual commit (EnableAutoCommit=false) to give the application control over
    /// when offsets are considered processed. This enables at-least-once delivery semantics when the application
    /// commits only after successfully processing messages. The service observes CancellationToken to perform
    /// graceful shutdown and ensure in-flight messages can be completed.
    /// </summary>
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly TelemetryProcessor _processor;
        private readonly ILogger<KafkaConsumerService> _logger;

        /// <summary>
        /// Creates a new KafkaConsumerService.
        /// </summary>
        public KafkaConsumerService(
            TelemetryProcessor processor,
            ILogger<KafkaConsumerService> logger,
            IConfiguration config)
        {
            _processor = processor;
            _logger = logger;

            // Configure the consumer. Disable auto-commit so the app controls offset commits after processing.
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

        /// <summary>
        /// Main loop that consumes messages until the host signals cancellation.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kafka Consumer started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Blocking call that respects the cancellation token.
                    var result = _consumer.Consume(stoppingToken);

                    if (result == null || result.IsPartitionEOF)
                        continue;

                    // Forward the message payload to the processor. The processor is responsible for idempotency and
                    // any ordering guarantees required by the domain.
                    await _processor.ProcessAsync(result.Message.Value);

                    // Commit the offset only after successful processing to avoid data loss on restart.
                    _consumer.Commit(result);
                }
                catch (OperationCanceledException)
                {
                    // Expected during shutdown; break out to allow the service to stop gracefully.
                    break;
                }
                catch (Exception ex)
                {
                    // Log and continue. In production consider exponential backoff or poison-message handling.
                    _logger.LogError(ex, "Error consuming message");
                }
            }
        }

        /// <summary>
        /// Ensure the consumer is closed and disposed when the service is disposed.
        /// </summary>
        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }

}

using Amazon;
using Amazon.S3;
using Npgsql;
using OpenSearch.Client;
using OpenSearch.Net;
using ProcessingService.Services;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

// -----------------------------
// Redis: Fast in-memory cache
// -----------------------------
// Redis is used for live UI state and quick lookups.
// It’s ideal for ephemeral data like current flight positions,
// alerts, or operator dashboards because it offers sub-millisecond access.
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:Connection"])
);
builder.Services.AddSingleton<RedisWriter>();

// -----------------------------
// OpenSearch: Historical search & analytics
// -----------------------------
// OpenSearch stores telemetry history for time-series queries,
// anomaly investigations, and operator search dashboards.
// The client is configured with:
// - Default index: "telemetry-history"
// - Basic auth (admin/admin for dev)
// - Certificate validation disabled (dev convenience)
// - Debug mode enabled for full request/response logging
builder.Services.AddSingleton<IOpenSearchClient>(sp =>
{
    var settings = new ConnectionSettings(new Uri(builder.Configuration["OpenSearch:Url"]))
        .DefaultIndex("telemetry-history")
        .BasicAuthentication("admin", "admin")
        .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
        .DisableDirectStreaming() // ensures request/response bodies are captured for debugging
        .EnableDebugMode();       // logs detailed API call info

    return new OpenSearchClient(settings);
});
builder.Services.AddSingleton<OpenSearchWriter>();

// -----------------------------
// PostgreSQL: Durable relational store
// -----------------------------
// PostgreSQL is used for finance/audit events where ACID guarantees matter.
// Unlike Redis (ephemeral) or OpenSearch (search-optimized),
// Postgres ensures strict transactional consistency.
// Connection string is read from appsettings.json.
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connString = config.GetConnectionString("Postgres")
                     ?? config["Postgres:ConnectionString"];
    return new NpgsqlConnection(connString);
});
builder.Services.AddSingleton<PostgresWriter>();

// -----------------------------
// S3: Raw telemetry archive
// -----------------------------
// S3 (or compatible object storage) is used for compliance and reprocessing.
// Every raw telemetry JSON is stored here for long-term retention.
// This ensures you can replay data streams or audit historical records.
// Credentials and region are read from appsettings.json.
builder.Services.AddSingleton<IAmazonS3>(sp =>
    new AmazonS3Client(
        builder.Configuration["S3:AccessKey"],
        builder.Configuration["S3:SecretAccessKey"],
        RegionEndpoint.APSoutheast2
    )
);
builder.Services.AddSingleton<ArchiveWriter>();

// -----------------------------
// Core services
// -----------------------------
// TelemetryProcessor: orchestrates validation, enrichment, and routing
// KafkaConsumerService: continuously consumes telemetry from Kafka,
// then dispatches to Redis, OpenSearch, Postgres, and S3 writers.
builder.Services.AddSingleton<TelemetryProcessor>();
builder.Services.AddHostedService<KafkaConsumerService>();

// -----------------------------
// Host startup
// -----------------------------
// This builds the DI container and starts the background services.
// The app runs as a long-lived worker service, continuously processing telemetry.
var app = builder.Build();
app.Run();
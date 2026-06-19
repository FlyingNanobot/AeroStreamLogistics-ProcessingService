using Amazon;
using Amazon.S3;
using OpenSearch.Client;
using ProcessingService.Services;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:Connection"])
);

// OpenSearch
builder.Services.AddSingleton<IOpenSearchClient>(sp =>
{
    var settings = new ConnectionSettings(new Uri(builder.Configuration["OpenSearch:Url"]))
        .DefaultIndex("telemetry-history");
    return new OpenSearchClient(settings);
});

// PostgreSQL
builder.Services.AddSingleton<PostgresWriter>();

// S3
builder.Services.AddSingleton<IAmazonS3>(sp =>
    new AmazonS3Client(
        builder.Configuration["S3:AccessKey"],
        builder.Configuration["S3:SecretAccessKey"],
        RegionEndpoint.APSoutheast2
    )
);
builder.Services.AddSingleton<ArchiveWriter>();

// Core services
builder.Services.AddSingleton<RedisWriter>();
builder.Services.AddSingleton<OpenSearchWriter>();
builder.Services.AddSingleton<TelemetryProcessor>();
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();
app.Run();
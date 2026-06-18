# AeroStreamLogistics-ProcessingService

Overview
--------
This repository contains a .NET Worker Service that consumes telemetry/events (via Kafka) and writes processed results to multiple backends (Redis, Postgres, OpenSearch, archive storage).

Purpose and architecture
------------------------
- KafkaConsumerService: background worker that pulls messages from Kafka and forwards to processing pipeline.
- TelemetryProcessor: applies domain-specific transformations, deduplication and idempotency checks.
- Writers (RedisWriter, PostgresWriter, OpenSearchWriter, ArchiveWriter): take processed messages and persist them to the respective stores.

Theory and critical design notes
--------------------------------
- BackgroundService pattern: The project uses Microsoft.Extensions.Hosting BackgroundService to implement long-running, resilient workers. Each worker is responsible for graceful shutdown, observing CancellationToken, and retry behavior.
- Idempotency: The system makes best-effort idempotent writes (see Utils/Idempotency.cs). This avoids duplication when retries occur or messages are reprocessed.
- Redis usage: Redis is used as a fast cache for live flight state. Writes are optimized to minimize churn and use atomic update patterns where possible.
- Ordering & retries: Kafka provides partition ordering; the consumer preserves ordering per-partition. Writers should be tolerant of out-of-order messages when eventual consistency is acceptable.

How to build & run
-------------------
Requires .NET 10 SDK. From the repository root:

	dotnet build
	dotnet run --project ProcessingService

Configuration
-------------
See ProcessingService/appsettings.json and appsettings.Development.json for Kafka, Redis, Postgres, and OpenSearch connection settings.

Adding documentation
--------------------
This change adds XML documentation to public APIs and explanatory inline comments for critical logic. Maintainers should keep comments up-to-date when modifying public behavior.

Contributing
------------
File an issue or PR on the repository. Keep changes small and run a build before submitting.

using System.Text.Json;
using Confluent.Kafka;
using InventoryCore.Idempotency;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Inventory.AuditWorker;

public sealed class AuditWorkerService : BackgroundService
{
    private readonly ILogger<AuditWorkerService> _logger;
    private readonly IIdempotencyStore _idempotency;
    private readonly NpgsqlDataSource _db;
    private readonly KafkaAuditOptions _kafka;

    public AuditWorkerService(
        ILogger<AuditWorkerService> logger,
        IIdempotencyStore idempotency,
        NpgsqlDataSource db,
        IOptions<KafkaAuditOptions> kafka)
    {
        _logger = logger;
        _idempotency = idempotency;
        _db = db;
        _kafka = kafka.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cfg = new ConsumerConfig
        {
            BootstrapServers = _kafka.BootstrapServers,
            GroupId = _kafka.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(cfg).Build();
        consumer.Subscribe(_kafka.Topics);
        _logger.LogInformation("Audit worker subscribed to {Topics}", string.Join(", ", _kafka.Topics));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cr = consumer.Consume(TimeSpan.FromSeconds(1));
                if (cr?.Message?.Value is null)
                    continue;

                await HandleAsync(cr.Message.Value, stoppingToken).ConfigureAwait(false);
                consumer.Commit(cr);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
            }
        }
    }

    private async Task HandleAsync(string json, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (!root.TryGetProperty("after", out var after) || after.ValueKind == JsonValueKind.Null)
            return;

        var outboxId = after.GetProperty("id").GetGuid();

        if (!await _idempotency.TryAcquireAsync($"audit:{outboxId}", TimeSpan.FromHours(24), ct).ConfigureAwait(false))
        {
            _logger.LogDebug("Skip duplicate audit for outbox {Id}", outboxId);
            return;
        }

        var aggregateId = after.GetProperty("aggregate_id").GetString() ?? "";
        var aggregateType = after.GetProperty("aggregate_type").GetString() ?? "";
        var eventType = after.GetProperty("event_type").GetString() ?? "";
        var actorId = after.TryGetProperty("actor_id", out var actorEl) ? actorEl.GetString() ?? "" : "";
        var occurredAt = after.GetProperty("occurred_at").GetDateTimeOffset();

        var payloadRaw = after.GetProperty("payload");
        var payloadStr = payloadRaw.ValueKind == JsonValueKind.String
            ? payloadRaw.GetString() ?? "{}"
            : payloadRaw.GetRawText();

        string? beforeJson = null;
        string? afterJson = null;

        try
        {
            using var payloadDoc = JsonDocument.Parse(payloadStr);
            var payloadRoot = payloadDoc.RootElement;

            if (payloadRoot.TryGetProperty("Before", out var beforeEl) && beforeEl.ValueKind != JsonValueKind.Null)
                beforeJson = beforeEl.GetRawText();
            else if (payloadRoot.TryGetProperty("before", out var beforeLo) && beforeLo.ValueKind != JsonValueKind.Null)
                beforeJson = beforeLo.GetRawText();

            if (payloadRoot.TryGetProperty("After", out var afterEl) && afterEl.ValueKind != JsonValueKind.Null)
                afterJson = afterEl.GetRawText();
            else if (payloadRoot.TryGetProperty("after", out var afterLo) && afterLo.ValueKind != JsonValueKind.Null)
                afterJson = afterLo.GetRawText();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse payload for outbox {Id}", outboxId);
        }

        await InsertAuditEntryAsync(outboxId, aggregateId, aggregateType, eventType, actorId,
            beforeJson, afterJson, occurredAt, ct).ConfigureAwait(false);

        _logger.LogDebug("Recorded audit entry for {AggregateType}/{EventType} on {AggregateId}",
            aggregateType, eventType, aggregateId);
    }

    private async Task InsertAuditEntryAsync(
        Guid outboxId, string aggregateId, string aggregateType, string eventType,
        string actorId, string? beforeJson, string? afterJson,
        DateTimeOffset occurredAt, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO audit_log (id, outbox_id, aggregate_id, aggregate_type, event_type, actor_id, before_json, after_json, occurred_at, recorded_at)
            VALUES (@id, @outboxId, @aggregateId, @aggregateType, @eventType, @actorId, @beforeJson::jsonb, @afterJson::jsonb, @occurredAt, @recordedAt)
            ON CONFLICT (outbox_id) DO NOTHING
            """;

        await using var conn = await _db.OpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("outboxId", outboxId);
        cmd.Parameters.AddWithValue("aggregateId", aggregateId);
        cmd.Parameters.AddWithValue("aggregateType", aggregateType);
        cmd.Parameters.AddWithValue("eventType", eventType);
        cmd.Parameters.AddWithValue("actorId", actorId);
        cmd.Parameters.AddWithValue("beforeJson", (object?)beforeJson ?? DBNull.Value);
        cmd.Parameters.AddWithValue("afterJson", (object?)afterJson ?? DBNull.Value);
        cmd.Parameters.AddWithValue("occurredAt", occurredAt);
        cmd.Parameters.AddWithValue("recordedAt", DateTimeOffset.UtcNow);

        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}

public sealed class KafkaAuditOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "inventory-audit-worker";
    public string[] Topics { get; set; } = [];
}

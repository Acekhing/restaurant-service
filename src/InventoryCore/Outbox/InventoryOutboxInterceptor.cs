using Inventory.Contracts.Outbox;
using InventoryCore.Actor;
using InventoryCore.RowVersion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace InventoryCore.Outbox;

public sealed class InventoryOutboxInterceptor<TContext, TOutbox> : SaveChangesInterceptor
    where TContext : DbContext
    where TOutbox : OutboxEntryBase, new()
{
    private readonly IActorContext _actor;
    private readonly IOutboxEntityMapper _mapper;

    public InventoryOutboxInterceptor(IActorContext actor, IOutboxEntityMapper mapper)
    {
        _actor = actor;
        _mapper = mapper;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is TContext ctx)
            AppendOutbox(ctx);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is TContext ctx)
            AppendOutbox(ctx);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AppendOutbox(TContext context)
    {
        BumpRowVersions(context);
        var actorId = _actor.ActorId;

        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;
            if (entry.Entity is TOutbox)
                continue;

            var descriptor = _mapper.Describe(entry);
            if (descriptor is null)
                continue;

            Dictionary<string, object?>? before;
            Dictionary<string, object?>? after;

            switch (entry.State)
            {
                case EntityState.Added:
                    before = null;
                    after = EntitySnapshotSerializer.Snapshot(entry.CurrentValues);
                    break;
                case EntityState.Modified:
                    before = EntitySnapshotSerializer.Snapshot(entry.OriginalValues);
                    after = EntitySnapshotSerializer.Snapshot(entry.CurrentValues);
                    break;
                case EntityState.Deleted:
                    before = EntitySnapshotSerializer.Snapshot(entry.OriginalValues);
                    after = null;
                    break;
                default:
                    continue;
            }

            var payload = new UnifiedAuditPayload
            {
                Actor = actorId,
                AggregateId = descriptor.AggregateId,
                Timestamp = DateTimeOffset.UtcNow,
                Before = before,
                After = after
            };

            var row = new TOutbox
            {
                Id = Guid.NewGuid(),
                AggregateId = descriptor.AggregateId,
                AggregateType = descriptor.AggregateType,
                EventType = descriptor.EventType,
                Payload = EntitySnapshotSerializer.SerializePayload(payload),
                ActorId = actorId,
                OccurredAt = DateTimeOffset.UtcNow,
                ProcessedAt = null,
                RetryCount = 0,
                Error = null
            };

            context.Set<TOutbox>().Add(row);
        }
    }

    private static void BumpRowVersions(TContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Modified)
                continue;
            if (entry.Entity is not IRowVersioned)
                continue;

            var prop = entry.Property(nameof(IRowVersioned.RowVersion));
            if (prop.OriginalValue is long orig)
                prop.CurrentValue = orig + 1;
        }
    }
}

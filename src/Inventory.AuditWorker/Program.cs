using Inventory.AuditWorker;
using InventoryCore.Idempotency;
using Npgsql;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaAuditOptions>(builder.Configuration.GetSection("Kafka"));

var redisCs = builder.Configuration["Redis:ConnectionString"]
              ?? throw new InvalidOperationException("Redis:ConnectionString required");
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisCs));
builder.Services.AddSingleton<IIdempotencyStore>(sp =>
    new RedisIdempotencyStore(sp.GetRequiredService<IConnectionMultiplexer>(),
        builder.Configuration["Redis:KeyPrefix"] ?? "outbox:audit:"));

var dbCs = builder.Configuration["Database:ConnectionString"]
           ?? throw new InvalidOperationException("Database:ConnectionString required");
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(dbCs).Build());

builder.Services.AddHostedService<AuditWorkerService>();

var host = builder.Build();
host.Run();

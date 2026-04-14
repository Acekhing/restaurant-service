using Elastic.Clients.Elasticsearch;
using Inventory.ElasticProjector;
using InventoryCore.Idempotency;
using Npgsql;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaConsumerOptions>(builder.Configuration.GetSection("Kafka"));

var redisCs = builder.Configuration["Redis:ConnectionString"]
              ?? throw new InvalidOperationException("Redis:ConnectionString required");
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisCs));
builder.Services.AddSingleton<IIdempotencyStore>(sp =>
    new RedisIdempotencyStore(sp.GetRequiredService<IConnectionMultiplexer>(),
        builder.Configuration["Redis:KeyPrefix"] ?? "outbox:"));

var esUri = builder.Configuration["Elasticsearch:Uri"]
            ?? throw new InvalidOperationException("Elasticsearch:Uri required");
builder.Services.AddSingleton(new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(esUri))));

var dbCs = builder.Configuration["Database:ConnectionString"]
           ?? throw new InvalidOperationException("Database:ConnectionString required");
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(dbCs).Build());

builder.Services.AddHostedService<ElasticProjectorWorker>();

var host = builder.Build();
host.Run();

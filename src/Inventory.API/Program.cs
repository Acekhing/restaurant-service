using Elastic.Clients.Elasticsearch;
using Inventory.API.Data;
using Inventory.API.Data.Entities;
using Inventory.API.Services;
using InventoryCore.Actor;
using InventoryCore.Outbox;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175")
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IActorContext, HttpActorContext>();
builder.Services.AddScoped<IOutboxEntityMapper, InventoryOutboxEntityMapper>();
builder.Services.AddScoped<InventoryOutboxInterceptor<InventoryDbContext, InventoryOutbox>>();

var esUri = builder.Configuration["Elasticsearch:Uri"]
            ?? throw new InvalidOperationException("Elasticsearch:Uri required");
builder.Services.AddSingleton(new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(esUri))));

var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(
    builder.Configuration.GetConnectionString("Inventory")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:Inventory"));
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<InventoryDbContext>((sp, o) =>
{
    o.UseNpgsql(dataSource)
        .UseSnakeCaseNamingConvention()
        .AddInterceptors(sp.GetRequiredService<InventoryOutboxInterceptor<InventoryDbContext, InventoryOutbox>>());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

public partial class Program;

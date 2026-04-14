using System.Text.Json;
using Infrastructure.StressSimulation.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Infrastructure.StressSimulation.Api;

public static class SimulationEndpoints
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static void MapSimulationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stress");

        group.MapGet("/scenarios", () =>
        {
            var scenarios = ScenarioDefinition.AllScenarios.Select(s => new ScenarioInfo
            {
                Name = s.Name,
                Description = s.Description,
                DurationMinutes = s.Duration.TotalMinutes,
                WriteMultiplier = s.WriteMultiplier,
                ReadMultiplier = s.ReadMultiplier,
                FailureComponent = s.Failure?.Component.ToString()
            });
            return Results.Ok(scenarios);
        });

        group.MapPost("/run", (RunRequest? body, SimulationRunnerService runner) =>
        {
            var names = body?.ScenarioNames ?? [];
            var run = runner.StartRun(names, body?.Branches, body?.InventoryItems, body?.Menus, body?.ConcurrentUsers);
            return Results.Ok(new RunResponse { RunId = run.RunId });
        });

        group.MapGet("/stream/{runId}", async (string runId, SimulationRunnerService runner, HttpContext ctx) =>
        {
            var run = runner.GetRun(runId);
            if (run is null)
            {
                ctx.Response.StatusCode = 404;
                return;
            }

            ctx.Response.ContentType = "text/event-stream";
            ctx.Response.Headers.CacheControl = "no-cache";
            ctx.Response.Headers.Connection = "keep-alive";

            var reader = run.Channel.Reader;
            var ct = ctx.RequestAborted;

            try
            {
                await foreach (var evt in reader.ReadAllAsync(ct))
                {
                    var json = JsonSerializer.Serialize(evt.Data, JsonOpts);
                    await ctx.Response.WriteAsync($"event: {evt.Type}\n", ct);
                    await ctx.Response.WriteAsync($"data: {json}\n\n", ct);
                    await ctx.Response.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                // Client disconnected
            }
        });

        group.MapGet("/report/{runId}", (string runId, SimulationRunnerService runner) =>
        {
            var run = runner.GetRun(runId);
            if (run is null)
                return Results.NotFound();
            if (!run.IsComplete || run.Report is null)
                return Results.StatusCode(202);
            return Results.Text(run.Report, "text/markdown");
        });
    }
}

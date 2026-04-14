using InventoryCore.Actor;

namespace Inventory.API.Services;

public sealed class HttpActorContext : IActorContext
{
    private readonly IHttpContextAccessor _http;

    public HttpActorContext(IHttpContextAccessor http) => _http = http;

    public string ActorId =>
        _http.HttpContext?.User?.Identity?.Name
        ?? _http.HttpContext?.Connection.RemoteIpAddress?.ToString()
        ?? "system";
}

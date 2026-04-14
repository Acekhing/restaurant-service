using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Inventory.Contracts.ReadModel;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/menus/search")]
public sealed class MenuSearchController : ControllerBase
{
    private const string IndexName = "menus";
    private readonly ElasticsearchClient _es;

    public MenuSearchController(ElasticsearchClient es) => _es = es;

    public sealed record SortItemEntry(string Id, int SortOrder);

    [HttpPatch("{id}/sort-items")]
    public async Task<IActionResult> SortItems(string id, [FromBody] List<SortItemEntry> body, CancellationToken ct)
    {
        var getResp = await _es.GetAsync<MenuReadModel>(IndexName, id, ct);
        if (!getResp.IsValidResponse || getResp.Source is null)
            return NotFound();

        var doc = getResp.Source;
        if (doc.InventoryItems is null or { Count: 0 })
            return NoContent();

        var sortMap = body.ToDictionary(x => x.Id, x => x.SortOrder);
        foreach (var item in doc.InventoryItems)
        {
            if (sortMap.TryGetValue(item.Id, out var order))
                item.SortOrder = order;
        }

        doc.InventoryItems = doc.InventoryItems.OrderBy(x => x.SortOrder).ToList();

        var indexResp = await _es.IndexAsync(doc, i => i.Index(IndexName).Id(id), ct);
        if (!indexResp.IsValidResponse)
            return StatusCode(502, new { error = "Failed to update sort order", detail = indexResp.DebugInformation });

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<MenuSearchResultDto>> Search(
        [FromQuery] string? q,
        [FromQuery] string? ownerId,
        [FromQuery] bool? active,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        size = Math.Clamp(size, 1, 100);
        page = Math.Max(1, page);
        var from = (page - 1) * size;

        var resp = await _es.SearchAsync<MenuReadModel>(s => s
            .Index(IndexName)
            .From(from)
            .Size(size)
            .Query(BuildQuery(q, ownerId, active)), ct);

        if (!resp.IsValidResponse)
            return StatusCode(502, new { error = "Elasticsearch query failed", detail = resp.DebugInformation });

        return Ok(new MenuSearchResultDto
        {
            Items = resp.Documents.ToList(),
            Total = resp.Total,
            Page = page,
            Size = size
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MenuReadModel>> GetById(string id, CancellationToken ct)
    {
        var resp = await _es.GetAsync<MenuReadModel>(IndexName, id, ct);

        if (!resp.IsValidResponse || resp.Source is null)
            return NotFound();

        return Ok(resp.Source);
    }

    private static Action<QueryDescriptor<MenuReadModel>> BuildQuery(
        string? q, string? ownerId, bool? active)
    {
        return descriptor =>
        {
            var filters = new List<Action<QueryDescriptor<MenuReadModel>>>();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q;
                filters.Add(f => f.MultiMatch(mm => mm
                    .Query(term)
                    .Fields(new[] { "categoryName", "description", "ownerName" })
                    .Fuzziness(new Fuzziness("AUTO"))));
            }

            if (!string.IsNullOrWhiteSpace(ownerId))
            {
                var owner = ownerId;
                filters.Add(f => f.Term(t => t.Field(new Elastic.Clients.Elasticsearch.Field("ownerId.keyword")).Value(owner)));
            }

            if (active.HasValue)
            {
                var val = active.Value;
                filters.Add(f => f.Term(t => t.Field(x => x.IsActive).Value(val)));
            }

            if (filters.Count == 0)
            {
                descriptor.MatchAll(new MatchAllQuery());
                return;
            }

            descriptor.Bool(b => b.Must(filters.ToArray()));
        };
    }
}

public sealed class MenuSearchResultDto
{
    public IReadOnlyList<MenuReadModel> Items { get; init; } = [];
    public long Total { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }
}

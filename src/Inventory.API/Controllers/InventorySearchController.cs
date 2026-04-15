using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Inventory.API.Elasticsearch;
using Inventory.Contracts.ReadModel;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory/search")]
public sealed class InventorySearchController : ControllerBase
{
    private const string IndexName = "inventory";
    private readonly ElasticsearchClient _es;

    public InventorySearchController(ElasticsearchClient es) => _es = es;

    [HttpGet]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromQuery] string? q,
        [FromQuery] string? itemType,
        [FromQuery] string? retailerType,
        [FromQuery] string? ownerId,
        [FromQuery] bool? available,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        size = Math.Clamp(size, 1, 100);
        page = Math.Max(1, page);
        var from = (page - 1) * size;

        var resp = await _es.SearchAsync<InventoryReadModel>(s => s
            .Index(IndexName)
            .From(from)
            .Size(size)
            .Query(BuildQuery(q, itemType, retailerType, ownerId, available)), ct);

        if (!resp.IsValidResponse)
        {
            if (resp.IsMissingOrUnavailableSearchIndex())
            {
                return Ok(new SearchResultDto
                {
                    Items = [],
                    Total = 0,
                    Page = page,
                    Size = size
                });
            }

            return StatusCode(502, new { error = "Elasticsearch query failed", detail = resp.DebugInformation });
        }

        return Ok(new SearchResultDto
        {
            Items = resp.Documents.ToList(),
            Total = resp.Total,
            Page = page,
            Size = size
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryReadModel>> GetById(string id, CancellationToken ct)
    {
        var resp = await _es.GetAsync<InventoryReadModel>(IndexName, id, ct);

        if (!resp.IsValidResponse || resp.Source is null)
            return NotFound();

        return Ok(resp.Source);
    }

    private static Action<QueryDescriptor<InventoryReadModel>> BuildQuery(
        string? q, string? itemType, string? retailerType, string? ownerId, bool? available)
    {
        return descriptor =>
        {
            var filters = new List<Action<QueryDescriptor<InventoryReadModel>>>();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q;
                filters.Add(f => f.MultiMatch(mm => mm
                    .Query(term)
                    .Fields(new[] { "name", "shortName", "tags", "ownerName" })
                    .Fuzziness(new Fuzziness("AUTO"))));
            }

            if (!string.IsNullOrWhiteSpace(itemType))
            {
                var type = itemType;
                filters.Add(f => f.Term(t => t.Field(new Elastic.Clients.Elasticsearch.Field("itemType.keyword")).Value(type)));
            }

            if (!string.IsNullOrWhiteSpace(retailerType))
            {
                var rt = retailerType;
                filters.Add(f => f.Term(t => t.Field(new Elastic.Clients.Elasticsearch.Field("retailerType.keyword")).Value(rt)));
            }

            if (!string.IsNullOrWhiteSpace(ownerId))
            {
                var owner = ownerId;
                filters.Add(f => f.Term(t => t.Field(new Elastic.Clients.Elasticsearch.Field("retailerId.keyword")).Value(owner)));
            }

            if (available.HasValue)
            {
                var val = available.Value;
                filters.Add(f => f.Term(t => t.Field(x => x.IsAvailable).Value(val)));
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

public sealed class SearchResultDto
{
    public IReadOnlyList<InventoryReadModel> Items { get; init; } = [];
    public long Total { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }
}

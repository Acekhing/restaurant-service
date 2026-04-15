using Elastic.Transport.Products.Elasticsearch;

namespace Inventory.API.Elasticsearch;

internal static class ElasticsearchResponseExtensions
{
    /// <summary>
    /// True when the search cannot run because the index is missing or not available yet
    /// (e.g. elastic-projector has not created it). Prefer empty results over 502 for reads.
    /// </summary>
    public static bool IsMissingOrUnavailableSearchIndex(this ElasticsearchResponse response)
    {
        if (response.IsValidResponse)
            return false;

        if (response.ElasticsearchServerError?.Status is 404)
            return true;

        var dbg = response.DebugInformation ?? string.Empty;
        if (dbg.Contains("index_not_found_exception", StringComparison.OrdinalIgnoreCase))
            return true;
        if (dbg.Contains("no such index", StringComparison.OrdinalIgnoreCase))
            return true;
        if (dbg.Contains("index_closed_exception", StringComparison.OrdinalIgnoreCase))
            return true;

        var err = response.ElasticsearchServerError?.Error;
        if (err?.RootCause is null)
            return false;

        foreach (var c in err.RootCause)
        {
            if (c.Type is "index_not_found_exception"
                or "no_shard_available_action_exception"
                or "index_closed_exception")
                return true;
        }

        return false;
    }
}

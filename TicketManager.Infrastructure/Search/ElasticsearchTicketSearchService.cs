using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using TicketManager.Application.Interfaces;

namespace TicketManager.Infrastructure.Search;

public class ElasticsearchTicketSearchService : ITicketSearchIndex
{
    private const string IndexName = "tickets";

    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchTicketSearchService> _logger;

    public ElasticsearchTicketSearchService(ElasticsearchClient client, ILogger<ElasticsearchTicketSearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task IndexAsync(Guid id, string title, string description, CancellationToken ct = default)
    {
        try
        {
            var document = new TicketSearchDocument(id, title, description);
            await _client.IndexAsync(document, x => x.Index(IndexName).Id(id.ToString()), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Fail open: search is a convenience feature, not the source of
            // truth (Postgres is). An indexing failure must never break
            // ticket creation/updates.
            _logger.LogWarning(ex, "Failed to index ticket {TicketId} in Elasticsearch", id);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            await _client.DeleteAsync(IndexName, id.ToString(), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to delete ticket {TicketId} from Elasticsearch", id);
        }
    }

    public async Task<IReadOnlyList<Guid>> SearchAsync(string query, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.SearchAsync<TicketSearchDocument>(s => s
                .Indices(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh.Match(m => m.Field(f => f.Title).Query(query)),
                            sh => sh.Match(m => m.Field(f => f.Description).Query(query))
                        )
                    )
                ), ct);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch search for {Query} returned an invalid response", query);
                return [];
            }

            return response.Documents.Select(d => d.Id).ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to search tickets in Elasticsearch for {Query}", query);
            return [];
        }
    }
}

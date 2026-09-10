using MediatR;
using TicketManager.Application.DTOs;
using TicketManager.Application.Interfaces;
using TicketManager.Domain.Entities;
using TicketManager.Domain.Interfaces;

namespace TicketManager.Application.Queries.SearchTickets;

public class SearchTicketsHandler : IRequestHandler<SearchTicketsQuery, IEnumerable<TicketDto>>
{
    private readonly ITicketRepository _repo;
    private readonly ITicketSearchIndex _searchIndex;

    public SearchTicketsHandler(ITicketRepository repo, ITicketSearchIndex searchIndex)
    {
        _repo = repo;
        _searchIndex = searchIndex;
    }

    public async Task<IEnumerable<TicketDto>> Handle(SearchTicketsQuery query, CancellationToken ct)
    {
        var ids = await _searchIndex.SearchAsync(query.Query, ct);
        var tickets = await _repo.GetByIdsAsync(ids, ct);
        var byId = tickets.ToDictionary(t => t.Id);

        // Preserve Elasticsearch's relevance ranking rather than DB order.
        return ids
            .Select(id => byId.GetValueOrDefault(id))
            .OfType<Ticket>()
            .Select(TicketDto.FromDomain);
    }
}

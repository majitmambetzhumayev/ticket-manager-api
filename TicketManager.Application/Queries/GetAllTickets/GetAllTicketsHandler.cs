using MediatR;
using TicketManager.Application.DTOs;
using TicketManager.Domain.Interfaces;

namespace TicketManager.Application.Queries.GetAllTickets;

public class GetAllTicketsHandler : IRequestHandler<GetAllTicketsQuery, IEnumerable<TicketDto>>
{
    private readonly ITicketRepository _repo;

    public GetAllTicketsHandler(ITicketRepository repo) => _repo = repo;

    public async Task<IEnumerable<TicketDto>> Handle(GetAllTicketsQuery query, CancellationToken ct)
    {
        var tickets = await _repo.GetAllAsync(query.Status, ct);
        return tickets.Select(TicketDto.FromDomain);
    }
}

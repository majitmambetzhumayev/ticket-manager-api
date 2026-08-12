using MediatR;
using TicketManager.Application.DTOs;
using TicketManager.Application.Exceptions;
using TicketManager.Domain.Interfaces;

namespace TicketManager.Application.Queries.GetTicketById;

public class GetTicketByIdHandler : IRequestHandler<GetTicketByIdQuery, TicketDto>
{
    private readonly ITicketRepository _repo;

    public GetTicketByIdHandler(ITicketRepository repo) => _repo = repo;

    public async Task<TicketDto> Handle(GetTicketByIdQuery query, CancellationToken ct)
    {
        var ticket = await _repo.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException($"Ticket {query.Id} not found.");

        return TicketDto.FromDomain(ticket);
    }
}

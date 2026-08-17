using MediatR;
using TicketManager.Application.DTOs;
using TicketManager.Application.Exceptions;
using TicketManager.Domain.Interfaces;

namespace TicketManager.Application.Commands.ResolveTicket;

public class ResolveTicketHandler : IRequestHandler<ResolveTicketCommand, TicketDto>
{
    private readonly ITicketRepository _repo;

    public ResolveTicketHandler(ITicketRepository repo) => _repo = repo;

    public async Task<TicketDto> Handle(ResolveTicketCommand cmd, CancellationToken ct)
    {
        var ticket = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException($"Ticket {cmd.Id} not found.");

        ticket.Resolve(cmd.ResolutionNotes);
        await _repo.UpdateAsync(ticket, ct);

        return TicketDto.FromDomain(ticket);
    }
}

using MediatR;
using TicketManager.Application.DTOs;
using TicketManager.Application.Exceptions;
using TicketManager.Domain.Interfaces;

namespace TicketManager.Application.Commands.CloseTicket;

public class CloseTicketHandler : IRequestHandler<CloseTicketCommand, TicketDto>
{
    private readonly ITicketRepository _repo;

    public CloseTicketHandler(ITicketRepository repo) => _repo = repo;

    public async Task<TicketDto> Handle(CloseTicketCommand cmd, CancellationToken ct)
    {
        var ticket = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException($"Ticket {cmd.Id} not found.");

        ticket.Close();
        await _repo.UpdateAsync(ticket, ct);

        return TicketDto.FromDomain(ticket);
    }
}

using MediatR;
using TicketManager.Application.DTOs;
using TicketManager.Application.Exceptions;
using TicketManager.Domain.Interfaces;

namespace TicketManager.Application.Commands.UpdateTicket;

public class UpdateTicketHandler : IRequestHandler<UpdateTicketCommand, TicketDto>
{
    private readonly ITicketRepository _repo;

    public UpdateTicketHandler(ITicketRepository repo) => _repo = repo;

    public async Task<TicketDto> Handle(UpdateTicketCommand cmd, CancellationToken ct)
    {
        var ticket = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException($"Ticket {cmd.Id} not found.");

        ticket.UpdateDetails(cmd.Title, cmd.Description);
        await _repo.UpdateAsync(ticket, ct);

        return TicketDto.FromDomain(ticket);
    }
}

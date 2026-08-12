using MediatR;
using TicketManager.Application.Exceptions;
using TicketManager.Domain.Interfaces;

namespace TicketManager.Application.Commands.DeleteTicket;

public class DeleteTicketHandler : IRequestHandler<DeleteTicketCommand>
{
    private readonly ITicketRepository _repo;

    public DeleteTicketHandler(ITicketRepository repo) => _repo = repo;

    public async Task Handle(DeleteTicketCommand cmd, CancellationToken ct)
    {
        var ticket = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException($"Ticket {cmd.Id} not found.");

        ticket.EnsureDeletable();
        await _repo.DeleteAsync(cmd.Id, ct);
    }
}

using MediatR;
using TicketManager.Application.Exceptions;
using TicketManager.Application.Interfaces;
using TicketManager.Domain.Interfaces;

namespace TicketManager.Application.Commands.DeleteTicket;

public class DeleteTicketHandler : IRequestHandler<DeleteTicketCommand>
{
    private readonly ITicketRepository _repo;
    private readonly ITicketSearchIndex _searchIndex;

    public DeleteTicketHandler(ITicketRepository repo, ITicketSearchIndex searchIndex)
    {
        _repo = repo;
        _searchIndex = searchIndex;
    }

    public async Task Handle(DeleteTicketCommand cmd, CancellationToken ct)
    {
        var ticket = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException($"Ticket {cmd.Id} not found.");

        ticket.EnsureDeletable();
        await _repo.DeleteAsync(cmd.Id, ct);
        await _searchIndex.DeleteAsync(cmd.Id, ct);
    }
}

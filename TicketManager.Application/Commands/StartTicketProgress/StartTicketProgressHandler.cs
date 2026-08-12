using MediatR;
using TicketManager.Application.DTOs;
using TicketManager.Application.Exceptions;
using TicketManager.Domain.Interfaces;

namespace TicketManager.Application.Commands.StartTicketProgress;

public class StartTicketProgressHandler : IRequestHandler<StartTicketProgressCommand, TicketDto>
{
    private readonly ITicketRepository _repo;

    public StartTicketProgressHandler(ITicketRepository repo) => _repo = repo;

    public async Task<TicketDto> Handle(StartTicketProgressCommand cmd, CancellationToken ct)
    {
        var ticket = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException($"Ticket {cmd.Id} not found.");

        ticket.StartProgress();
        await _repo.UpdateAsync(ticket, ct);

        return TicketDto.FromDomain(ticket);
    }
}

using MediatR;
using TicketManager.Application.DTOs;
using TicketManager.Application.Interfaces;
using TicketManager.Domain.Entities;
using TicketManager.Domain.Interfaces;

namespace TicketManager.Application.Commands.CreateTicket;

public class CreateTicketHandler : IRequestHandler<CreateTicketCommand, TicketDto>
{
    private readonly ITicketRepository _repo;
    private readonly ITicketAIClassifier _classifier;

    public CreateTicketHandler(ITicketRepository repo, ITicketAIClassifier classifier)
    {
        _repo = repo;
        _classifier = classifier;
    }

    public async Task<TicketDto> Handle(CreateTicketCommand cmd, CancellationToken ct)
    {
        var priority = await _classifier.ClassifyAsync(cmd.Title, cmd.Description, ct);
        var ticket = Ticket.Create(cmd.Title, cmd.Description, cmd.UserId, priority);
        await _repo.AddAsync(ticket, ct);
        return TicketDto.FromDomain(ticket);
    }
}

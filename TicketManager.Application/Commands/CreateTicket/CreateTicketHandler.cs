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
    private readonly ITicketSearchIndex _searchIndex;

    public CreateTicketHandler(ITicketRepository repo, ITicketAIClassifier classifier, ITicketSearchIndex searchIndex)
    {
        _repo = repo;
        _classifier = classifier;
        _searchIndex = searchIndex;
    }

    public async Task<TicketDto> Handle(CreateTicketCommand cmd, CancellationToken ct)
    {
        var classification = await _classifier.ClassifyAsync(cmd.Title, cmd.Description, ct);
        var ticket = Ticket.Create(
            cmd.Title, cmd.Description, cmd.UserId,
            classification.Priority, classification.Category,
            classification.SuggestedResponse, classification.GroundedInHistory);
        await _repo.AddAsync(ticket, ct);
        await _searchIndex.IndexAsync(ticket.Id, ticket.Title, ticket.Description, ct);
        return TicketDto.FromDomain(ticket);
    }
}

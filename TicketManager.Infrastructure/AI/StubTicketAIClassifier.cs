using TicketManager.Application.Interfaces;
using TicketManager.Domain.Enums;

namespace TicketManager.Infrastructure.AI;

public class StubTicketAIClassifier : ITicketAIClassifier
{
    public Task<TicketPriority> ClassifyAsync(string title, string description, CancellationToken ct = default) =>
        Task.FromResult(TicketPriority.Medium);
}

using TicketManager.Application.Interfaces;

namespace TicketManager.Infrastructure.AI;

public class StubTicketAIClassifier : ITicketAIClassifier
{
    public Task<TicketClassificationResult> ClassifyAsync(string title, string description, CancellationToken ct = default) =>
        Task.FromResult(TicketClassificationResult.Default);
}

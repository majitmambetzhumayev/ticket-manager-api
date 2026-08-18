namespace TicketManager.Application.Interfaces;

public interface ITicketAIClassifier
{
    Task<TicketClassificationResult> ClassifyAsync(string title, string description, CancellationToken ct = default);
}

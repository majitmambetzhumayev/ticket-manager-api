using TicketManager.Domain.Enums;

namespace TicketManager.Application.Interfaces;

public interface ITicketAIClassifier
{
    Task<TicketPriority> ClassifyAsync(string title, string description, CancellationToken ct = default);
}

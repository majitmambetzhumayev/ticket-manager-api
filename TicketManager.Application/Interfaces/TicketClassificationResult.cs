using TicketManager.Domain.Enums;

namespace TicketManager.Application.Interfaces;

public record TicketClassificationResult(
    TicketPriority Priority,
    string Category,
    string? SuggestedResponse,
    bool GroundedInHistory)
{
    public static readonly TicketClassificationResult Default = new(TicketPriority.Medium, "General", null, false);
}

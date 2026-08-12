namespace TicketManager.Application.DTOs;

public record TicketHistoryEntryDto(
    Guid Id,
    string Field,
    string OldValue,
    string NewValue,
    DateTime ChangedAt);

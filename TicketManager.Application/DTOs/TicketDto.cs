using TicketManager.Domain.Entities;
using TicketManager.Domain.Enums;

namespace TicketManager.Application.DTOs;

public record TicketDto(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid UserId,
    DateTime CreatedAt,
    IReadOnlyCollection<TicketHistoryEntryDto> History)
{
    public static TicketDto FromDomain(Ticket ticket) => new(
        ticket.Id,
        ticket.Title,
        ticket.Description,
        ticket.Status,
        ticket.Priority,
        ticket.UserId,
        ticket.CreatedAt,
        ticket.History.Select(h => new TicketHistoryEntryDto(h.Id, h.Field, h.OldValue, h.NewValue, h.ChangedAt)).ToList());
}

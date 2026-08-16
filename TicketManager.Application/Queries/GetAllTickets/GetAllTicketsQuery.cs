using MediatR;
using TicketManager.Application.DTOs;
using TicketManager.Domain.Enums;

namespace TicketManager.Application.Queries.GetAllTickets;

public record GetAllTicketsQuery(TicketStatus? Status = null) : IRequest<IEnumerable<TicketDto>>;

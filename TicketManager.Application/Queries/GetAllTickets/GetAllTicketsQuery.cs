using MediatR;
using TicketManager.Application.DTOs;

namespace TicketManager.Application.Queries.GetAllTickets;

public record GetAllTicketsQuery : IRequest<IEnumerable<TicketDto>>;

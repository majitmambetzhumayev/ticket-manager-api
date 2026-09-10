using MediatR;
using TicketManager.Application.DTOs;

namespace TicketManager.Application.Queries.SearchTickets;

public record SearchTicketsQuery(string Query) : IRequest<IEnumerable<TicketDto>>;

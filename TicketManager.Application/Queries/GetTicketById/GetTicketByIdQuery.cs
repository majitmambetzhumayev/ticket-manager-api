using MediatR;
using TicketManager.Application.DTOs;

namespace TicketManager.Application.Queries.GetTicketById;

public record GetTicketByIdQuery(Guid Id) : IRequest<TicketDto>;

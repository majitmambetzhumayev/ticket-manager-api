using TicketManager.Application.DTOs;
using MediatR;

namespace TicketManager.Application.Commands.CreateTicket;

public record CreateTicketCommand(string Title, string Description, Guid UserId) : IRequest<TicketDto>;

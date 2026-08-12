using MediatR;
using TicketManager.Application.DTOs;

namespace TicketManager.Application.Commands.UpdateTicket;

public record UpdateTicketCommand(Guid Id, string Title, string Description) : IRequest<TicketDto>;

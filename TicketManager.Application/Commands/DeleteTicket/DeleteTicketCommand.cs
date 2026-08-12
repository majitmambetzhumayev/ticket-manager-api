using MediatR;

namespace TicketManager.Application.Commands.DeleteTicket;

public record DeleteTicketCommand(Guid Id) : IRequest;

using MediatR;
using TicketManager.Application.DTOs;

namespace TicketManager.Application.Commands.CloseTicket;

public record CloseTicketCommand(Guid Id) : IRequest<TicketDto>;

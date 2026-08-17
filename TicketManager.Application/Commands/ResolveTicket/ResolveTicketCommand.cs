using MediatR;
using TicketManager.Application.DTOs;

namespace TicketManager.Application.Commands.ResolveTicket;

public record ResolveTicketCommand(Guid Id, string ResolutionNotes) : IRequest<TicketDto>;

using MediatR;
using TicketManager.Application.DTOs;

namespace TicketManager.Application.Commands.StartTicketProgress;

public record StartTicketProgressCommand(Guid Id) : IRequest<TicketDto>;

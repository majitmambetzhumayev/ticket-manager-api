using FluentValidation;

namespace TicketManager.Application.Commands.ResolveTicket;

public class ResolveTicketValidator : AbstractValidator<ResolveTicketCommand>
{
    public ResolveTicketValidator() => RuleFor(x => x.Id).NotEmpty();
}

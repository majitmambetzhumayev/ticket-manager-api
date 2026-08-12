using FluentValidation;

namespace TicketManager.Application.Commands.CloseTicket;

public class CloseTicketValidator : AbstractValidator<CloseTicketCommand>
{
    public CloseTicketValidator() => RuleFor(x => x.Id).NotEmpty();
}

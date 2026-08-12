using FluentValidation;

namespace TicketManager.Application.Commands.DeleteTicket;

public class DeleteTicketValidator : AbstractValidator<DeleteTicketCommand>
{
    public DeleteTicketValidator() => RuleFor(x => x.Id).NotEmpty();
}

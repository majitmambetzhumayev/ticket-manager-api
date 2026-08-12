using FluentValidation;

namespace TicketManager.Application.Commands.StartTicketProgress;

public class StartTicketProgressValidator : AbstractValidator<StartTicketProgressCommand>
{
    public StartTicketProgressValidator() => RuleFor(x => x.Id).NotEmpty();
}

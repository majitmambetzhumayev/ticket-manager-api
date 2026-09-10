using FluentValidation;

namespace TicketManager.Application.Queries.SearchTickets;

public class SearchTicketsValidator : AbstractValidator<SearchTicketsQuery>
{
    public SearchTicketsValidator()
    {
        RuleFor(x => x.Query).NotEmpty().MaximumLength(200);
    }
}

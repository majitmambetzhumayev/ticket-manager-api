using TicketManager.Domain.Enums;
using TicketManager.Domain.Exceptions;

namespace TicketManager.Domain.Entities;

public class Ticket
{
    private readonly List<TicketHistoryEntry> _history = new();

    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? ResolutionNotes { get; private set; }
    public string Category { get; private set; } = null!;
    public string? SuggestedResponse { get; private set; }
    public bool GroundedInHistory { get; private set; }
    public IReadOnlyCollection<TicketHistoryEntry> History => _history.AsReadOnly();

    private Ticket() { } // EF Core

    public static Ticket Create(
        string title,
        string description,
        Guid userId,
        TicketPriority priority,
        string category,
        string? suggestedResponse = null,
        bool groundedInHistory = false)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required.");
        if (string.IsNullOrWhiteSpace(category)) throw new DomainException("Category is required.");

        return new Ticket
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Status = TicketStatus.Open,
            Priority = priority,
            Category = category,
            SuggestedResponse = suggestedResponse,
            GroundedInHistory = groundedInHistory,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(string title, string description)
    {
        if (Status is TicketStatus.Resolved or TicketStatus.Closed)
            throw new DomainException("Cannot update a resolved or closed ticket.");
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required.");

        if (title != Title) _history.Add(new TicketHistoryEntry(nameof(Title), Title, title));
        if (description != Description) _history.Add(new TicketHistoryEntry(nameof(Description), Description, description));

        Title = title;
        Description = description;
    }

    public void StartProgress() => TransitionTo(TicketStatus.InProgress, TicketStatus.Open);

    public void Resolve(string resolutionNotes)
    {
        if (string.IsNullOrWhiteSpace(resolutionNotes)) throw new DomainException("Resolution notes are required.");

        TransitionTo(TicketStatus.Resolved, TicketStatus.InProgress);
        ResolutionNotes = resolutionNotes;
    }

    public void Close() => TransitionTo(TicketStatus.Closed, TicketStatus.Resolved);

    public void EnsureDeletable()
    {
        if (Status != TicketStatus.Open) throw new DomainException("Only open tickets can be deleted.");
    }

    private void TransitionTo(TicketStatus next, TicketStatus requiredCurrent)
    {
        if (Status != requiredCurrent)
            throw new DomainException($"Cannot move ticket from {Status} to {next}.");

        _history.Add(new TicketHistoryEntry(nameof(Status), Status.ToString(), next.ToString()));
        Status = next;
    }
}

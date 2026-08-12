namespace TicketManager.Domain.Entities;

public class TicketHistoryEntry
{
    public Guid Id { get; private set; }
    public string Field { get; private set; } = null!;
    public string OldValue { get; private set; } = null!;
    public string NewValue { get; private set; } = null!;
    public DateTime ChangedAt { get; private set; }

    private TicketHistoryEntry() { } // EF Core

    internal TicketHistoryEntry(string field, string oldValue, string newValue)
    {
        Id = Guid.NewGuid();
        Field = field;
        OldValue = oldValue;
        NewValue = newValue;
        ChangedAt = DateTime.UtcNow;
    }
}

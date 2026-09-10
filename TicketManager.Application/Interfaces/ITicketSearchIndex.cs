namespace TicketManager.Application.Interfaces;

public interface ITicketSearchIndex
{
    Task IndexAsync(Guid id, string title, string description, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> SearchAsync(string query, CancellationToken ct = default);
}

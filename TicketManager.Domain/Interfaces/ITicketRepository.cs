using TicketManager.Domain.Entities;
using TicketManager.Domain.Enums;

namespace TicketManager.Domain.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Ticket>> GetAllAsync(TicketStatus? status = null, CancellationToken ct = default);
    Task AddAsync(Ticket ticket, CancellationToken ct = default);
    Task UpdateAsync(Ticket ticket, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

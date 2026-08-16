using Microsoft.EntityFrameworkCore;
using TicketManager.Domain.Entities;
using TicketManager.Domain.Interfaces;
using TicketManager.Infrastructure.Persistence;

namespace TicketManager.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context) => _context = context;

    public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IEnumerable<Ticket>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Tickets.ToListAsync(ct);

    public async Task AddAsync(Ticket ticket, CancellationToken ct = default)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Ticket ticket, CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var ticket = await _context.Tickets.FindAsync([id], ct);
        if (ticket is not null)
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync(ct);
        }
    }
}

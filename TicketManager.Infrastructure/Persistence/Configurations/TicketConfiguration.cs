using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManager.Domain.Entities;

namespace TicketManager.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(2000);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.ResolutionNotes).HasMaxLength(2000);
        builder.Property(t => t.Category).IsRequired().HasMaxLength(50);
        builder.Property(t => t.SuggestedResponse).HasMaxLength(4000);

        builder.OwnsMany(t => t.History, history =>
        {
            history.HasKey(h => h.Id);
            history.Property(h => h.Id).ValueGeneratedNever();
            history.WithOwner().HasForeignKey("TicketId");
            history.Property<Guid>("TicketId");
        });
    }
}

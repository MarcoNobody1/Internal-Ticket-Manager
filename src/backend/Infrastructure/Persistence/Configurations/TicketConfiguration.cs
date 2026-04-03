using InternalTicketManager.Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternalTicketManager.Infrastructure.Persistence.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ticket => ticket.Description)
            .HasMaxLength(4000);

        builder.Property(ticket => ticket.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(ticket => ticket.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(ticket => ticket.AssignedUserId)
            .HasMaxLength(100);

        builder.Property(ticket => ticket.CreatedByUsername)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ticket => ticket.CreatedAtUtc)
            .IsRequired();

        builder.Property(ticket => ticket.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(ticket => ticket.ProjectId);
        builder.HasIndex(ticket => ticket.Status);
        builder.HasIndex(ticket => ticket.Priority);

        builder.HasMany(ticket => ticket.Comments)
            .WithOne(comment => comment.Ticket)
            .HasForeignKey(comment => comment.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

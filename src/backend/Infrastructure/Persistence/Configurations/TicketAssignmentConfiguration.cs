using InternalTicketManager.Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternalTicketManager.Infrastructure.Persistence.Configurations;

public sealed class TicketAssignmentConfiguration : IEntityTypeConfiguration<TicketAssignment>
{
    public void Configure(EntityTypeBuilder<TicketAssignment> builder)
    {
        builder.ToTable("TicketAssignments");

        builder.HasKey(assignment => new { assignment.TicketId, assignment.UserId });

        builder.Property(assignment => assignment.AssignedAtUtc)
            .IsRequired();

        builder.HasIndex(assignment => assignment.UserId);

        builder.HasOne(assignment => assignment.User)
            .WithMany(user => user.TicketAssignments)
            .HasForeignKey(assignment => assignment.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

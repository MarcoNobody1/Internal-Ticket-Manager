using InternalTicketManager.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternalTicketManager.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(project => project.Description)
            .HasMaxLength(2000);

        builder.Property(project => project.CreatedAtUtc)
            .IsRequired();

        builder.Property(project => project.UpdatedAtUtc);

        builder.HasMany(project => project.Tickets)
            .WithOne(ticket => ticket.Project)
            .HasForeignKey(ticket => ticket.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

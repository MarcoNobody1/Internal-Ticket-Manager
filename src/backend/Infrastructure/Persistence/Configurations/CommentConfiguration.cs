using InternalTicketManager.Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternalTicketManager.Infrastructure.Persistence.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.AuthorUsername)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(comment => comment.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(comment => comment.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(comment => comment.TicketId);
    }
}

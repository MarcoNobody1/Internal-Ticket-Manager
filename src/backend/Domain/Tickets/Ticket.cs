namespace InternalTicketManager.Domain.Tickets;

public sealed class Ticket
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TicketStatus Status { get; set; }

    public TicketPriority Priority { get; set; }

    public Guid ProjectId { get; set; }

    public string? AssignedUserId { get; set; }

    public string CreatedByUsername { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public Projects.Project Project { get; set; } = null!;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

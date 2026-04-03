namespace InternalTicketManager.Domain.Projects;

public sealed class Project
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Tickets.Ticket> Tickets { get; set; } = new List<Tickets.Ticket>();
}

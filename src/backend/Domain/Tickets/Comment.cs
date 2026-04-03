namespace InternalTicketManager.Domain.Tickets;

public sealed class Comment
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public string AuthorUsername { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public Ticket Ticket { get; set; } = null!;
}

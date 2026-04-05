using InternalTicketManager.Domain.Auth;

namespace InternalTicketManager.Domain.Tickets;

public sealed class TicketAssignment
{
    public Guid TicketId { get; set; }

    public Guid UserId { get; set; }

    public DateTime AssignedAtUtc { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public User User { get; set; } = null!;
}

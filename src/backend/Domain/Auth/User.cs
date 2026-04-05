namespace InternalTicketManager.Domain.Auth;

public sealed class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string NormalizedUsername { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Tickets.TicketAssignment> TicketAssignments { get; set; } = new List<Tickets.TicketAssignment>();
}

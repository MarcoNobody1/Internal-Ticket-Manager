namespace InternalTicketManager.Domain.Auth;

public sealed class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}

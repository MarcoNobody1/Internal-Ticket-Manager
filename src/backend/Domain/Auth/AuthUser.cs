namespace InternalTicketManager.Domain.Auth;

public sealed record AuthUser(
    string Username,
    string Password,
    UserRole Role);

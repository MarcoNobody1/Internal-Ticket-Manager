namespace InternalTicketManager.Application.Auth;

public sealed record LoginResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string Username,
    string Role);

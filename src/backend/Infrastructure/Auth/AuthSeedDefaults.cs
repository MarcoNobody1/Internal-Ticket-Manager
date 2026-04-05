using InternalTicketManager.Domain.Auth;

namespace InternalTicketManager.Infrastructure.Auth;

public static class AuthSeedDefaults
{
    public const string AdminUsername = "admin.demo";
    public const string AdminPassword = "AdminDemo123!";
    public const string DeveloperUsername = "developer.demo";
    public const string DeveloperPassword = "DeveloperDemo123!";

    public static string NormalizeName(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    public static IReadOnlyList<(UserRole Role, string Username, string Password)> Users =>
    [
        (UserRole.Admin, AdminUsername, AdminPassword),
        (UserRole.Developer, DeveloperUsername, DeveloperPassword)
    ];
}

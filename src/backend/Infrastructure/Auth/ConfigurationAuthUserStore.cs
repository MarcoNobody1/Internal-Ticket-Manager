using InternalTicketManager.Application.Auth;
using InternalTicketManager.Domain.Auth;
using Microsoft.Extensions.Configuration;

namespace InternalTicketManager.Infrastructure.Auth;

public sealed class ConfigurationAuthUserStore : IAuthUserStore
{
    private const string SectionName = "AuthDemoUsers";
    private readonly IReadOnlyDictionary<string, AuthUser> _usersByUsername;

    public ConfigurationAuthUserStore(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _usersByUsername = configuration
            .GetSection(SectionName)
            .Get<List<AuthDemoUserSettings>>()
            ?.Select(MapUser)
            .ToDictionary(user => user.Username, StringComparer.OrdinalIgnoreCase)
            ?? throw new InvalidOperationException("AuthDemoUsers configuration is required.");

        if (_usersByUsername.Count == 0)
        {
            throw new InvalidOperationException("At least one demo auth user must be configured.");
        }
    }

    public Task<AuthUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        _usersByUsername.TryGetValue(username, out AuthUser? user);
        return Task.FromResult(user);
    }

    private static AuthUser MapUser(AuthDemoUserSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Username) || string.IsNullOrWhiteSpace(settings.Password))
        {
            throw new InvalidOperationException("Demo auth users require username and password values.");
        }

        if (!Enum.TryParse<UserRole>(settings.Role, ignoreCase: true, out UserRole role))
        {
            throw new InvalidOperationException($"Unsupported demo auth role '{settings.Role}'. Allowed values are Admin and Developer.");
        }

        return new AuthUser(settings.Username, settings.Password, role);
    }
}

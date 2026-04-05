using InternalTicketManager.Application.Auth;
using InternalTicketManager.Domain.Auth;
using InternalTicketManager.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace InternalTicketManager.Infrastructure.Persistence;

public sealed class ApplicationDbInitializer
{
    private readonly TicketingDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public ApplicationDbInitializer(TicketingDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await SeedRolesAsync(cancellationToken);
        await SeedUsersAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var role in Enum.GetValues<UserRole>())
        {
            var roleId = (int)role;
            var roleName = role.ToString();
            var normalizedRoleName = AuthSeedDefaults.NormalizeName(roleName);

            var existingRole = await _dbContext.Roles.SingleOrDefaultAsync(item => item.Id == roleId, cancellationToken);
            if (existingRole is null)
            {
                _dbContext.Roles.Add(new Role
                {
                    Id = roleId,
                    Name = roleName,
                    NormalizedName = normalizedRoleName
                });

                continue;
            }

            if (!string.Equals(existingRole.Name, roleName, StringComparison.Ordinal) ||
                !string.Equals(existingRole.NormalizedName, normalizedRoleName, StringComparison.Ordinal))
            {
                existingRole.Name = roleName;
                existingRole.NormalizedName = normalizedRoleName;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        foreach (var (role, username, password) in AuthSeedDefaults.Users)
        {
            var normalizedUsername = AuthSeedDefaults.NormalizeName(username);
            var existingUser = await _dbContext.Users.SingleOrDefaultAsync(
                user => user.NormalizedUsername == normalizedUsername,
                cancellationToken);

            if (existingUser is not null)
            {
                continue;
            }

            _dbContext.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                NormalizedUsername = normalizedUsername,
                PasswordHash = _passwordHasher.HashPassword(password),
                RoleId = (int)role,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

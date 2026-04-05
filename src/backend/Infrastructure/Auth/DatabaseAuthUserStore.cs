using InternalTicketManager.Application.Auth;
using InternalTicketManager.Domain.Auth;
using InternalTicketManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InternalTicketManager.Infrastructure.Auth;

public sealed class DatabaseAuthUserStore : IAuthUserStore
{
    private readonly TicketingDbContext _dbContext;

    public DatabaseAuthUserStore(TicketingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = AuthSeedDefaults.NormalizeName(username);

        return await _dbContext.Users
            .AsNoTracking()
            .Include(user => user.Role)
            .Where(user => user.NormalizedUsername == normalizedUsername)
            .Select(user => new AuthUser(
                user.Username,
                user.PasswordHash,
                (UserRole)user.RoleId))
            .SingleOrDefaultAsync(cancellationToken);
    }
}

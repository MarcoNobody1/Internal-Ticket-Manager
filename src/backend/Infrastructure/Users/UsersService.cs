using InternalTicketManager.Application.Auth;
using InternalTicketManager.Application.Users;
using InternalTicketManager.Domain.Auth;
using InternalTicketManager.Infrastructure.Auth;
using InternalTicketManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InternalTicketManager.Infrastructure.Users;

public sealed class UsersService : IUsersService
{
    private readonly TicketingDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UsersService(TicketingDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<UserResponse>> GetUsersAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Username)
            .Select(user => new UserResponse(
                user.Id,
                user.Username,
                user.Role.Name,
                user.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new UserResponse(
                user.Id,
                user.Username,
                user.Role.Name,
                user.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var normalizedUsername = AuthSeedDefaults.NormalizeName(request.Username);
        await EnsureUsernameIsUniqueAsync(normalizedUsername, null, cancellationToken);

        var role = ParseRole(request.Role);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username.Trim(),
            NormalizedUsername = normalizedUsername,
            PasswordHash = _passwordHasher.HashPassword(request.Password.Trim()),
            RoleId = (int)role,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(user, role);
    }

    public async Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(existingUser => existingUser.Id == id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var normalizedUsername = AuthSeedDefaults.NormalizeName(request.Username);
        await EnsureUsernameIsUniqueAsync(normalizedUsername, id, cancellationToken);

        var role = ParseRole(request.Role);
        await EnsureAdminRoleChangeIsAllowedAsync(user, role, cancellationToken);

        user.Username = request.Username.Trim();
        user.NormalizedUsername = normalizedUsername;
        user.RoleId = (int)role;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(request.Password.Trim());
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(user, role);
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(existingUser => existingUser.Id == id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        await EnsureAdminDeleteIsAllowedAsync(user, cancellationToken);

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureUsernameIsUniqueAsync(string normalizedUsername, Guid? currentUserId, CancellationToken cancellationToken)
    {
        var usernameExists = await _dbContext.Users.AnyAsync(
            user => user.NormalizedUsername == normalizedUsername && user.Id != currentUserId,
            cancellationToken);

        if (usernameExists)
        {
            throw new UserManagementValidationException(nameof(CreateUserRequest.Username), "Username must be unique.");
        }
    }

    private async Task EnsureAdminRoleChangeIsAllowedAsync(User user, UserRole newRole, CancellationToken cancellationToken)
    {
        if (user.RoleId != (int)UserRole.Admin || newRole == UserRole.Admin)
        {
            return;
        }

        var adminCount = await CountAdminsAsync(cancellationToken);
        if (adminCount <= 1)
        {
            throw new UserManagementValidationException(nameof(UpdateUserRequest.Role), "At least one admin user is required.");
        }
    }

    private async Task EnsureAdminDeleteIsAllowedAsync(User user, CancellationToken cancellationToken)
    {
        if (user.RoleId != (int)UserRole.Admin)
        {
            return;
        }

        var adminCount = await CountAdminsAsync(cancellationToken);
        if (adminCount <= 1)
        {
            throw new UserManagementValidationException(nameof(UpdateUserRequest.Role), "At least one admin user is required.");
        }
    }

    private Task<int> CountAdminsAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Users.CountAsync(user => user.RoleId == (int)UserRole.Admin, cancellationToken);
    }

    private static UserRole ParseRole(string role)
    {
        return Enum.Parse<UserRole>(role.Trim(), true);
    }

    private static UserResponse MapToResponse(User user, UserRole role)
    {
        return new UserResponse(user.Id, user.Username, role.ToString(), user.CreatedAtUtc);
    }
}

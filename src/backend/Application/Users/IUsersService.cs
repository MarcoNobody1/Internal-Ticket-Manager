using InternalTicketManager.Application.Users;

namespace InternalTicketManager.Application.Users;

public interface IUsersService
{
    Task<IReadOnlyList<UserResponse>> GetUsersAsync(CancellationToken cancellationToken);

    Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);

    Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken);
}

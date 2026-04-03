using System.Threading;

namespace InternalTicketManager.Application.Auth;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

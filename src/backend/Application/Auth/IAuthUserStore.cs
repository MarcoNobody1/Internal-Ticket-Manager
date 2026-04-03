using System.Threading;
using InternalTicketManager.Domain.Auth;

namespace InternalTicketManager.Application.Auth;

public interface IAuthUserStore
{
    Task<AuthUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
}

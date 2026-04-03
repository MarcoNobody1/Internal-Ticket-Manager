using InternalTicketManager.Domain.Auth;

namespace InternalTicketManager.Application.Auth;

public interface IJwtTokenGenerator
{
    LoginResult GenerateToken(AuthUser user);
}

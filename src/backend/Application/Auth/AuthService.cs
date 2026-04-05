using InternalTicketManager.Domain.Auth;

namespace InternalTicketManager.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IAuthUserStore _authUserStore;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IAuthUserStore authUserStore,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _authUserStore = authUserStore;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var username = request.Username.Trim();
        AuthUser? user = await _authUserStore.FindByUsernameAsync(username, cancellationToken);
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        return _jwtTokenGenerator.GenerateToken(user);
    }
}

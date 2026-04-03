using InternalTicketManager.Domain.Auth;

namespace InternalTicketManager.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IAuthUserStore _authUserStore;
    private readonly ICredentialValidator _credentialValidator;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IAuthUserStore authUserStore,
        ICredentialValidator credentialValidator,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _authUserStore = authUserStore;
        _credentialValidator = credentialValidator;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        AuthUser? user = await _authUserStore.FindByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !_credentialValidator.IsValid(request.Password, user.Password))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        return _jwtTokenGenerator.GenerateToken(user);
    }
}

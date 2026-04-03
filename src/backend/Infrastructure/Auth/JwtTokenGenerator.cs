using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InternalTicketManager.Application.Auth;
using InternalTicketManager.Domain.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InternalTicketManager.Infrastructure.Auth;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration is required.");

        if (string.IsNullOrWhiteSpace(_options.Issuer) || string.IsNullOrWhiteSpace(_options.Audience))
        {
            throw new InvalidOperationException("Jwt issuer and audience are required.");
        }

        if (string.IsNullOrWhiteSpace(_options.SigningKey) || _options.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt signing key must be at least 32 characters long.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public LoginResult GenerateToken(AuthUser user)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAtUtc = now.AddMinutes(_options.TokenLifetimeMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now.UtcDateTime).ToString(), ClaimValueTypes.Integer64),
            new Claim("role", user.Role.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            Expires = expiresAtUtc.UtcDateTime,
            SigningCredentials = _signingCredentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return new LoginResult(
            handler.WriteToken(token),
            expiresAtUtc,
            user.Username,
            user.Role.ToString());
    }
}

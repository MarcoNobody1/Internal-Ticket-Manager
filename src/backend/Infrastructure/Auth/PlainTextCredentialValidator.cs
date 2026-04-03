using InternalTicketManager.Application.Auth;

namespace InternalTicketManager.Infrastructure.Auth;

public sealed class PlainTextCredentialValidator : ICredentialValidator
{
    public bool IsValid(string providedPassword, string expectedPassword)
    {
        return string.Equals(providedPassword, expectedPassword, StringComparison.Ordinal);
    }
}

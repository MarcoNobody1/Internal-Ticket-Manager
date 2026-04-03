namespace InternalTicketManager.Application.Auth;

public interface ICredentialValidator
{
    bool IsValid(string providedPassword, string expectedPassword);
}

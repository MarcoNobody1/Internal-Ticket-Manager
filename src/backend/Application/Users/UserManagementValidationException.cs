namespace InternalTicketManager.Application.Users;

public sealed class UserManagementValidationException : Exception
{
    public UserManagementValidationException(string key, string error)
        : this(new Dictionary<string, string[]>
        {
            [key] = [error]
        })
    {
    }

    public UserManagementValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("User management validation failed.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

using System.ComponentModel.DataAnnotations;
using InternalTicketManager.Domain.Auth;

namespace InternalTicketManager.Application.Users;

public sealed record CreateUserRequest : IValidatableObject
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            yield return new ValidationResult("Username is required.", [nameof(Username)]);
        }
        else if (Username.Trim().Length > 100)
        {
            yield return new ValidationResult("Username must be 100 characters or fewer.", [nameof(Username)]);
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            yield return new ValidationResult("Password is required.", [nameof(Password)]);
        }

        if (string.IsNullOrWhiteSpace(Role))
        {
            yield return new ValidationResult("Role is required.", [nameof(Role)]);
        }
        else if (!Enum.TryParse<UserRole>(Role.Trim(), true, out _))
        {
            yield return new ValidationResult("Role must be Admin or Developer.", [nameof(Role)]);
        }
    }
}

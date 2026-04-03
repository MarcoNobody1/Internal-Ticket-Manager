using System.ComponentModel.DataAnnotations;

namespace InternalTicketManager.Application.Projects;

public sealed record CreateProjectRequest : IValidatableObject
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    [MaxLength(NameMaxLength)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(DescriptionMaxLength)]
    public string? Description { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Name is required.", [nameof(Name)]);
        }
    }
}

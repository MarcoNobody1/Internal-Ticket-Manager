using System.ComponentModel.DataAnnotations;

namespace InternalTicketManager.Application.Projects;

public sealed record UpdateProjectRequest : IValidatableObject
{
    [MaxLength(CreateProjectRequest.NameMaxLength)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(CreateProjectRequest.DescriptionMaxLength)]
    public string? Description { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Name is required.", [nameof(Name)]);
        }
    }
}

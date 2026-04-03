using System.ComponentModel.DataAnnotations;
using InternalTicketManager.Domain.Tickets;

namespace InternalTicketManager.Application.Tickets;

public sealed record UpdateTicketRequest : IValidatableObject
{
    [MaxLength(CreateTicketRequest.TitleMaxLength)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(CreateTicketRequest.DescriptionMaxLength)]
    public string? Description { get; init; }

    public TicketStatus Status { get; init; } = TicketStatus.Open;

    public TicketPriority Priority { get; init; } = TicketPriority.Medium;

    public Guid ProjectId { get; init; }

    [MaxLength(CreateTicketRequest.AssignedUserIdMaxLength)]
    public string? AssignedUserId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult("Title is required.", [nameof(Title)]);
        }

        if (ProjectId == Guid.Empty)
        {
            yield return new ValidationResult("ProjectId is required.", [nameof(ProjectId)]);
        }

        if (!Enum.IsDefined(Status))
        {
            yield return new ValidationResult("Status is invalid.", [nameof(Status)]);
        }

        if (!Enum.IsDefined(Priority))
        {
            yield return new ValidationResult("Priority is invalid.", [nameof(Priority)]);
        }
    }
}

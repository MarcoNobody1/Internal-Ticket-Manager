using System.ComponentModel.DataAnnotations;

namespace InternalTicketManager.Application.Tickets;

public sealed record CreateCommentRequest : IValidatableObject
{
    public const int AuthorUsernameMaxLength = 100;
    public const int ContentMaxLength = 2000;

    [MaxLength(AuthorUsernameMaxLength)]
    public string AuthorUsername { get; init; } = string.Empty;

    [MaxLength(ContentMaxLength)]
    public string Content { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(AuthorUsername))
        {
            yield return new ValidationResult("AuthorUsername is required.", [nameof(AuthorUsername)]);
        }

        if (string.IsNullOrWhiteSpace(Content))
        {
            yield return new ValidationResult("Content is required.", [nameof(Content)]);
        }
    }
}

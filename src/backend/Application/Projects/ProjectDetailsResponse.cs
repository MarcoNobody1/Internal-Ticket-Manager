namespace InternalTicketManager.Application.Projects;

public sealed record ProjectDetailsResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<ProjectTicketSummaryResponse> OpenTickets);

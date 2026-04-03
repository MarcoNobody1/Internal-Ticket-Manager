namespace InternalTicketManager.Application.Projects;

public sealed record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

using InternalTicketManager.Domain.Tickets;

namespace InternalTicketManager.Application.Tickets;

public sealed record TicketResponse(
    Guid Id,
    string Title,
    string? Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid ProjectId,
    IReadOnlyList<TicketAssigneeResponse> AssignedDevelopers,
    string CreatedByUsername,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

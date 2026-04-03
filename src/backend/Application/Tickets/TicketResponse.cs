using InternalTicketManager.Domain.Tickets;

namespace InternalTicketManager.Application.Tickets;

public sealed record TicketResponse(
    Guid Id,
    string Title,
    string? Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid ProjectId,
    string? AssignedUserId,
    string CreatedByUsername,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

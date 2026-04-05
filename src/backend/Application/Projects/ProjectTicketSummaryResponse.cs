using InternalTicketManager.Application.Tickets;
using InternalTicketManager.Domain.Tickets;

namespace InternalTicketManager.Application.Projects;

public sealed record ProjectTicketSummaryResponse(
    Guid Id,
    string Title,
    TicketStatus Status,
    TicketPriority Priority,
    string CreatedByUsername,
    DateTime UpdatedAtUtc,
    IReadOnlyList<TicketAssigneeResponse> AssignedDevelopers);

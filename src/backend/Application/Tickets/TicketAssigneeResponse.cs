namespace InternalTicketManager.Application.Tickets;

public sealed record TicketAssigneeResponse(
    Guid Id,
    string Username);

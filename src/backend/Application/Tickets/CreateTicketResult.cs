namespace InternalTicketManager.Application.Tickets;

public sealed record CreateTicketResult(TicketResponse? Ticket, bool ProjectNotFound, bool AssignedDevelopersInvalid);

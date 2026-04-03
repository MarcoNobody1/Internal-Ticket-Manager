namespace InternalTicketManager.Application.Tickets;

public sealed record UpdateTicketResult(TicketResponse? Ticket, bool TicketNotFound, bool ProjectNotFound);

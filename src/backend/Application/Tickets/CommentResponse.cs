namespace InternalTicketManager.Application.Tickets;

public sealed record CommentResponse(
    Guid Id,
    Guid TicketId,
    string AuthorUsername,
    string Content,
    DateTime CreatedAtUtc);

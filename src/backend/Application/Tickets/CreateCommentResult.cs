namespace InternalTicketManager.Application.Tickets;

public sealed record CreateCommentResult(CommentResponse? Comment, bool TicketNotFound);

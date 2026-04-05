using InternalTicketManager.Application.Common;

namespace InternalTicketManager.Application.Tickets;

public interface ITicketService
{
    Task<PagedResult<TicketResponse>> GetTicketsAsync(GetTicketsRequest request, CancellationToken cancellationToken);

    Task<TicketResponse?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CommentResponse>?> GetCommentsAsync(Guid ticketId, CancellationToken cancellationToken);

    Task<CreateTicketResult> CreateTicketAsync(CreateTicketRequest request, CancellationToken cancellationToken);

    Task<CreateCommentResult> CreateCommentAsync(Guid ticketId, CreateCommentRequest request, CancellationToken cancellationToken);

    Task<UpdateTicketResult> UpdateTicketAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteTicketAsync(Guid id, CancellationToken cancellationToken);
}

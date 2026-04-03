namespace InternalTicketManager.Application.Tickets;

public interface ITicketService
{
    Task<IReadOnlyList<TicketResponse>> GetTicketsAsync(CancellationToken cancellationToken);

    Task<TicketResponse?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<CreateTicketResult> CreateTicketAsync(CreateTicketRequest request, CancellationToken cancellationToken);

    Task<UpdateTicketResult> UpdateTicketAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken);
}

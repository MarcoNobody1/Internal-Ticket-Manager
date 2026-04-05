using System.ComponentModel.DataAnnotations;
using InternalTicketManager.Domain.Tickets;

namespace InternalTicketManager.Application.Tickets;

public sealed class GetTicketsRequest
{
    public TicketStatus? Status { get; set; }

    public TicketPriority? Priority { get; set; }

    public Guid? ProjectId { get; set; }

    public Guid? AssignedUserId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be 1 or greater.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;
}

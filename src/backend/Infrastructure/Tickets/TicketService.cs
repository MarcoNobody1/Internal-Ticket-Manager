using InternalTicketManager.Application.Tickets;
using InternalTicketManager.Domain.Tickets;
using InternalTicketManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InternalTicketManager.Infrastructure.Tickets;

public sealed class TicketService : ITicketService
{
    private readonly TicketingDbContext _dbContext;

    public TicketService(TicketingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TicketResponse>> GetTicketsAsync(CancellationToken cancellationToken)
    {
        var tickets = await _dbContext.Tickets
            .AsNoTracking()
            .OrderByDescending(ticket => ticket.CreatedAtUtc)
            .ThenBy(ticket => ticket.Title)
            .ToListAsync(cancellationToken);

        return tickets.Select(MapToResponse).ToList();
    }

    public async Task<TicketResponse?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .AsNoTracking()
            .SingleOrDefaultAsync(existingTicket => existingTicket.Id == id, cancellationToken);

        return ticket is null ? null : MapToResponse(ticket);
    }

    public async Task<CreateTicketResult> CreateTicketAsync(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var projectExists = await ProjectExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return new CreateTicketResult(null, ProjectNotFound: true);
        }

        var utcNow = DateTime.UtcNow;
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = NormalizeOptionalValue(request.Description),
            Status = request.Status,
            Priority = request.Priority,
            ProjectId = request.ProjectId,
            AssignedUserId = NormalizeOptionalValue(request.AssignedUserId),
            CreatedByUsername = request.CreatedByUsername.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };

        _dbContext.Tickets.Add(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateTicketResult(MapToResponse(ticket), ProjectNotFound: false);
    }

    public async Task<UpdateTicketResult> UpdateTicketAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .SingleOrDefaultAsync(existingTicket => existingTicket.Id == id, cancellationToken);

        if (ticket is null)
        {
            return new UpdateTicketResult(null, TicketNotFound: true, ProjectNotFound: false);
        }

        var projectExists = await ProjectExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return new UpdateTicketResult(null, TicketNotFound: false, ProjectNotFound: true);
        }

        ticket.Title = request.Title.Trim();
        ticket.Description = NormalizeOptionalValue(request.Description);
        ticket.Status = request.Status;
        ticket.Priority = request.Priority;
        ticket.ProjectId = request.ProjectId;
        ticket.AssignedUserId = NormalizeOptionalValue(request.AssignedUserId);
        ticket.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateTicketResult(MapToResponse(ticket), TicketNotFound: false, ProjectNotFound: false);
    }

    private async Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(project => project.Id == projectId, cancellationToken);
    }

    private static TicketResponse MapToResponse(Ticket ticket)
    {
        return new TicketResponse(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.ProjectId,
            ticket.AssignedUserId,
            ticket.CreatedByUsername,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc);
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

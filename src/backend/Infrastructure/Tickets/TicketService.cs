using InternalTicketManager.Application.Tickets;
using InternalTicketManager.Domain.Auth;
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
            .Include(ticket => ticket.Assignments)
            .ThenInclude(assignment => assignment.User)
            .OrderByDescending(ticket => ticket.CreatedAtUtc)
            .ThenBy(ticket => ticket.Title)
            .ToListAsync(cancellationToken);

        return tickets.Select(MapToResponse).ToList();
    }

    public async Task<TicketResponse?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .AsNoTracking()
            .Include(ticket => ticket.Assignments)
            .ThenInclude(assignment => assignment.User)
            .SingleOrDefaultAsync(existingTicket => existingTicket.Id == id, cancellationToken);

        return ticket is null ? null : MapToResponse(ticket);
    }

    public async Task<IReadOnlyList<CommentResponse>?> GetCommentsAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticketExists = await TicketExistsAsync(ticketId, cancellationToken);
        if (!ticketExists)
        {
            return null;
        }

        var comments = await _dbContext.Comments
            .AsNoTracking()
            .Where(comment => comment.TicketId == ticketId)
            .OrderBy(comment => comment.CreatedAtUtc)
            .ThenBy(comment => comment.Id)
            .ToListAsync(cancellationToken);

        return comments.Select(MapToResponse).ToList();
    }

    public async Task<CreateTicketResult> CreateTicketAsync(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var projectExists = await ProjectExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return new CreateTicketResult(null, ProjectNotFound: true, AssignedDevelopersInvalid: false);
        }

        var assignedDeveloperIds = NormalizeAssignedDeveloperIds(request.AssignedDeveloperIds);
        var assignedDevelopersValid = await AssignedDevelopersAreValidAsync(assignedDeveloperIds, cancellationToken);
        if (!assignedDevelopersValid)
        {
            return new CreateTicketResult(null, ProjectNotFound: false, AssignedDevelopersInvalid: true);
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
            CreatedByUsername = request.CreatedByUsername.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };

        foreach (var developerId in assignedDeveloperIds)
        {
            ticket.Assignments.Add(new TicketAssignment
            {
                TicketId = ticket.Id,
                UserId = developerId,
                AssignedAtUtc = utcNow
            });
        }

        _dbContext.Tickets.Add(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _dbContext.Entry(ticket)
            .Collection(existingTicket => existingTicket.Assignments)
            .Query()
            .Include(assignment => assignment.User)
            .LoadAsync(cancellationToken);

        return new CreateTicketResult(MapToResponse(ticket), ProjectNotFound: false, AssignedDevelopersInvalid: false);
    }

    public async Task<CreateCommentResult> CreateCommentAsync(Guid ticketId, CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .SingleOrDefaultAsync(existingTicket => existingTicket.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            return new CreateCommentResult(null, TicketNotFound: true);
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            AuthorUsername = request.AuthorUsername.Trim(),
            Content = request.Content.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Comments.Add(comment);
        ticket.UpdatedAtUtc = comment.CreatedAtUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateCommentResult(MapToResponse(comment), TicketNotFound: false);
    }

    public async Task<UpdateTicketResult> UpdateTicketAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .Include(existingTicket => existingTicket.Assignments)
            .SingleOrDefaultAsync(existingTicket => existingTicket.Id == id, cancellationToken);

        if (ticket is null)
        {
            return new UpdateTicketResult(null, TicketNotFound: true, ProjectNotFound: false, AssignedDevelopersInvalid: false);
        }

        var projectExists = await ProjectExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return new UpdateTicketResult(null, TicketNotFound: false, ProjectNotFound: true, AssignedDevelopersInvalid: false);
        }

        var assignedDeveloperIds = NormalizeAssignedDeveloperIds(request.AssignedDeveloperIds);
        var assignedDevelopersValid = await AssignedDevelopersAreValidAsync(assignedDeveloperIds, cancellationToken);
        if (!assignedDevelopersValid)
        {
            return new UpdateTicketResult(null, TicketNotFound: false, ProjectNotFound: false, AssignedDevelopersInvalid: true);
        }

        ticket.Title = request.Title.Trim();
        ticket.Description = NormalizeOptionalValue(request.Description);
        ticket.Status = request.Status;
        ticket.Priority = request.Priority;
        ticket.ProjectId = request.ProjectId;
        ticket.UpdatedAtUtc = DateTime.UtcNow;

        ticket.Assignments.Clear();
        foreach (var developerId in assignedDeveloperIds)
        {
            ticket.Assignments.Add(new TicketAssignment
            {
                TicketId = ticket.Id,
                UserId = developerId,
                AssignedAtUtc = ticket.UpdatedAtUtc
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _dbContext.Entry(ticket)
            .Collection(existingTicket => existingTicket.Assignments)
            .Query()
            .Include(assignment => assignment.User)
            .LoadAsync(cancellationToken);

        return new UpdateTicketResult(MapToResponse(ticket), TicketNotFound: false, ProjectNotFound: false, AssignedDevelopersInvalid: false);
    }

    public async Task<bool> DeleteTicketAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .SingleOrDefaultAsync(existingTicket => existingTicket.Id == id, cancellationToken);

        if (ticket is null)
        {
            return false;
        }

        _dbContext.Tickets.Remove(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(project => project.Id == projectId, cancellationToken);
    }

    private async Task<bool> TicketExistsAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        return await _dbContext.Tickets
            .AsNoTracking()
            .AnyAsync(ticket => ticket.Id == ticketId, cancellationToken);
    }

    private async Task<bool> AssignedDevelopersAreValidAsync(IReadOnlyCollection<Guid> assignedDeveloperIds, CancellationToken cancellationToken)
    {
        if (assignedDeveloperIds.Count == 0)
        {
            return true;
        }

        var developerCount = await _dbContext.Users
            .AsNoTracking()
            .CountAsync(
                user => assignedDeveloperIds.Contains(user.Id) && user.RoleId == (int)UserRole.Developer,
                cancellationToken);

        return developerCount == assignedDeveloperIds.Count;
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
            ticket.Assignments
                .OrderBy(assignment => assignment.User.Username)
                .Select(assignment => new TicketAssigneeResponse(assignment.UserId, assignment.User.Username))
                .ToList(),
            ticket.CreatedByUsername,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc);
    }

    private static CommentResponse MapToResponse(Comment comment)
    {
        return new CommentResponse(
            comment.Id,
            comment.TicketId,
            comment.AuthorUsername,
            comment.Content,
            comment.CreatedAtUtc);
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static IReadOnlyList<Guid> NormalizeAssignedDeveloperIds(IReadOnlyList<Guid>? assignedDeveloperIds)
    {
        return (assignedDeveloperIds ?? [])
            .Where(developerId => developerId != Guid.Empty)
            .Distinct()
            .ToList();
    }
}

using InternalTicketManager.Application.Projects;
using InternalTicketManager.Application.Tickets;
using InternalTicketManager.Domain.Projects;
using InternalTicketManager.Domain.Tickets;
using InternalTicketManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InternalTicketManager.Infrastructure.Projects;

public sealed class ProjectService : IProjectService
{
    private readonly TicketingDbContext _dbContext;

    public ProjectService(TicketingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProjectResponse>> GetProjectsAsync(CancellationToken cancellationToken)
    {
        var projects = await _dbContext.Projects
            .AsNoTracking()
            .OrderBy(project => project.Name)
            .ToListAsync(cancellationToken);

        return projects.Select(MapToResponse).ToList();
    }

    public async Task<ProjectDetailsResponse?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .Include(existingProject => existingProject.Tickets.Where(ticket => ticket.Status != TicketStatus.Resolved && ticket.Status != TicketStatus.Closed))
            .ThenInclude(ticket => ticket.Assignments)
            .ThenInclude(assignment => assignment.User)
            .SingleOrDefaultAsync(project => project.Id == id, cancellationToken);

        return project is null ? null : MapToDetailsResponse(project);
    }

    public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = NormalizeDescription(request.Description),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(project);
    }

    public async Task<ProjectResponse?> UpdateProjectAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .SingleOrDefaultAsync(existingProject => existingProject.Id == id, cancellationToken);

        if (project is null)
        {
            return null;
        }

        project.Name = request.Name.Trim();
        project.Description = NormalizeDescription(request.Description);
        project.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(project);
    }

    public async Task<bool> DeleteProjectAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .SingleOrDefaultAsync(existingProject => existingProject.Id == id, cancellationToken);

        if (project is null)
        {
            return false;
        }

        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ProjectResponse MapToResponse(Project project)
    {
        return new ProjectResponse(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedAtUtc,
            project.UpdatedAtUtc);
    }

    private static ProjectDetailsResponse MapToDetailsResponse(Project project)
    {
        return new ProjectDetailsResponse(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedAtUtc,
            project.UpdatedAtUtc,
            project.Tickets
                .OrderByDescending(ticket => ticket.UpdatedAtUtc)
                .ThenBy(ticket => ticket.Title)
                .Select(ticket => new ProjectTicketSummaryResponse(
                    ticket.Id,
                    ticket.Title,
                    ticket.Status,
                    ticket.Priority,
                    ticket.CreatedByUsername,
                    ticket.UpdatedAtUtc,
                    ticket.Assignments
                        .OrderBy(assignment => assignment.User.Username)
                        .Select(assignment => new TicketAssigneeResponse(assignment.UserId, assignment.User.Username))
                        .ToList()))
                .ToList());
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        return description.Trim();
    }
}

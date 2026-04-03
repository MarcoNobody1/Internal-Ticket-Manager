using InternalTicketManager.Application.Projects;
using InternalTicketManager.Domain.Projects;
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

    public async Task<ProjectResponse?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .SingleOrDefaultAsync(project => project.Id == id, cancellationToken);

        return project is null ? null : MapToResponse(project);
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

    private static ProjectResponse MapToResponse(Project project)
    {
        return new ProjectResponse(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedAtUtc,
            project.UpdatedAtUtc);
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

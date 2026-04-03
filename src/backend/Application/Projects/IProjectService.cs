namespace InternalTicketManager.Application.Projects;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectResponse>> GetProjectsAsync(CancellationToken cancellationToken);

    Task<ProjectResponse?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken);

    Task<ProjectResponse?> UpdateProjectAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken);
}

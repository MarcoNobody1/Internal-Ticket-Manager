namespace InternalTicketManager.Application.Projects;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectResponse>> GetProjectsAsync(CancellationToken cancellationToken);

    Task<ProjectDetailsResponse?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken);

    Task<ProjectResponse?> UpdateProjectAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteProjectAsync(Guid id, CancellationToken cancellationToken);
}

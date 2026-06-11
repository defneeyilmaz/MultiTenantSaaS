namespace MultiTenantSaaS.Application.Contracts.Projects;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<ProjectDto> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default);
}

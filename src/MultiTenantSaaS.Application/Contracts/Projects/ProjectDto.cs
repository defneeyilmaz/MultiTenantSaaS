namespace MultiTenantSaaS.Application.Contracts.Projects;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt);

public sealed record CreateProjectRequest(string Name, string? Description);

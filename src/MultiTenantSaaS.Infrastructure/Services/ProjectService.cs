using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Application.Contracts.Projects;
using MultiTenantSaaS.Application.Contracts.Tenancy;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Infrastructure.Persistence;

namespace MultiTenantSaaS.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ProjectService(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<ProjectDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();

        return await _dbContext.Projects
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto(p.Id, p.Name, p.Description, p.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectDto> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Project name is required.");
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = request.Description?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProjectDto(project.Id, project.Name, project.Description, project.CreatedAt);
    }

    private Guid GetRequiredTenantId() =>
        _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is not set.");
}

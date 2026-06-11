using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Application.Contracts.Tasks;
using MultiTenantSaaS.Application.Contracts.Tenancy;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Infrastructure.Persistence;

namespace MultiTenantSaaS.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public TaskService(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<TaskDto>> ListAsync(
        Guid? projectId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();

        var query = _dbContext.Tasks
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId);

        if (projectId is not null)
        {
            query = query.Where(t => t.ProjectId == projectId);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskDto(
                t.Id,
                t.ProjectId,
                t.Title,
                t.Description,
                t.Status,
                t.AssignedToUserId,
                t.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskDto> CreateAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var title = request.Title.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Task title is required.");
        }

        var projectExists = await _dbContext.Projects
            .AnyAsync(p => p.Id == request.ProjectId && p.TenantId == tenantId, cancellationToken);

        if (!projectExists)
        {
            throw new InvalidOperationException("Project not found.");
        }

        if (request.AssignedToUserId is not null)
        {
            var assigneeIsMember = await _dbContext.UserTenantMemberships
                .AnyAsync(
                    m => m.UserId == request.AssignedToUserId
                         && m.TenantId == tenantId
                         && m.IsActive,
                    cancellationToken);

            if (!assigneeIsMember)
            {
                throw new InvalidOperationException("Assigned user is not a member of this tenant.");
            }
        }

        var task = new ProjectTask
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = request.ProjectId,
            Title = title,
            Description = request.Description?.Trim(),
            AssignedToUserId = request.AssignedToUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TaskDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.AssignedToUserId,
            task.CreatedAt);
    }

    public async Task<TaskDto> UpdateStatusAsync(
        Guid taskId,
        UpdateTaskStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();

        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId, cancellationToken);

        if (task is null)
        {
            throw new InvalidOperationException("Task not found.");
        }

        if (!Enum.IsDefined(request.Status))
        {
            throw new InvalidOperationException("Invalid task status.");
        }

        task.Status = request.Status;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TaskDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.AssignedToUserId,
            task.CreatedAt);
    }

    private Guid GetRequiredTenantId() =>
        _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is not set.");
}

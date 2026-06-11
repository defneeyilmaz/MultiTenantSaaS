using MultiTenantSaaS.Domain.Common;
using MultiTenantSaaS.Domain.Enums;

namespace MultiTenantSaaS.Domain.Entities;

public class ProjectTask : ITenantEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Todo;

    public Guid? AssignedToUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Project Project { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;

    public ApplicationUser? AssignedToUser { get; set; }
}

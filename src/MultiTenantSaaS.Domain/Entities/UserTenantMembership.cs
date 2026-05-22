using MultiTenantSaaS.Domain.Common;
using MultiTenantSaaS.Domain.Enums;

namespace MultiTenantSaaS.Domain.Entities;

public class UserTenantMembership : ITenantEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public MembershipRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset JoinedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}

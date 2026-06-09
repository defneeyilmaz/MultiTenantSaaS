using MultiTenantSaaS.Application.Contracts.Tenancy;

namespace MultiTenantSaaS.Infrastructure.Tenancy;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }

    public string? TenantSlug { get; private set; }

    public bool HasTenant => TenantId is not null;

    public void SetTenant(Guid tenantId, string tenantSlug)
    {
        TenantId = tenantId;
        TenantSlug = tenantSlug;
    }
}

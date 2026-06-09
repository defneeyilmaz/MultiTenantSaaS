namespace MultiTenantSaaS.Application.Contracts.Tenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }

    string? TenantSlug { get; }

    bool HasTenant { get; }

    void SetTenant(Guid tenantId, string tenantSlug);
}

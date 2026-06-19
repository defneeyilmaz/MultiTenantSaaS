using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Application.Contracts.Audit;
using MultiTenantSaaS.Application.Contracts.Tenancy;
using MultiTenantSaaS.Application.Contracts.Tenants;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Shared.Constants;

namespace MultiTenantSaaS.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditService _auditService;

    public TenantService(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IAuditService auditService)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _auditService = auditService;
    }

    public async Task<TenantDto> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();

        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
        {
            throw new InvalidOperationException("Tenant not found.");
        }

        return MapToDto(tenant);
    }

    public async Task<TenantDto> UpdateSettingsAsync(
        UpdateTenantSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Tenant name is required.");
        }

        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
        {
            throw new InvalidOperationException("Tenant not found.");
        }

        tenant.Name = name;
        tenant.Domain = string.IsNullOrWhiteSpace(request.Domain) ? null : request.Domain.Trim();
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditActions.TenantSettingsUpdated,
            details: $"Updated tenant settings for {tenant.Name}.",
            entityType: nameof(Tenant),
            entityId: tenant.Id,
            tenantId: tenant.Id,
            cancellationToken: cancellationToken);

        return MapToDto(tenant);
    }

    private Guid GetRequiredTenantId() =>
        _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is not set.");

    private static TenantDto MapToDto(Domain.Entities.Tenant tenant) =>
        new(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.Domain,
            tenant.IsActive,
            tenant.CreatedAt);
}

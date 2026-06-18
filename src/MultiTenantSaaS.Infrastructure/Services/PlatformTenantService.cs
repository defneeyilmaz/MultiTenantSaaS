using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Application.Contracts.Platform;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Shared.Utilities;

namespace MultiTenantSaaS.Infrastructure.Services;

public class PlatformTenantService : IPlatformTenantService
{
    private readonly AppDbContext _dbContext;

    public PlatformTenantService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PlatformTenantDto>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
            .AsNoTracking()
            .OrderBy(tenant => tenant.Name)
            .Select(tenant => new PlatformTenantDto(
                tenant.Id,
                tenant.Name,
                tenant.Slug,
                tenant.Domain,
                tenant.IsActive,
                tenant.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<PlatformTenantDto> CreateAsync(
        CreatePlatformTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Tenant name is required.");
        }

        var slugSource = string.IsNullOrWhiteSpace(request.Slug) ? name : request.Slug.Trim();
        var slug = SlugGenerator.From(slugSource);

        if (await _dbContext.Tenants.AnyAsync(tenant => tenant.Slug == slug, cancellationToken))
        {
            throw new InvalidOperationException($"Tenant slug '{slug}' is already taken.");
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Domain = string.IsNullOrWhiteSpace(request.Domain) ? null : request.Domain.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(tenant);
    }

    public async Task<PlatformTenantDto> UpdateAsync(
        Guid tenantId,
        UpdatePlatformTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
        {
            throw new InvalidOperationException("Tenant not found.");
        }

        if (request.Name is not null)
        {
            var name = request.Name.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Tenant name is required.");
            }

            tenant.Name = name;
        }

        if (request.Domain is not null)
        {
            tenant.Domain = string.IsNullOrWhiteSpace(request.Domain) ? null : request.Domain.Trim();
        }

        if (request.IsActive.HasValue)
        {
            tenant.IsActive = request.IsActive.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(tenant);
    }

    private static PlatformTenantDto MapToDto(Tenant tenant) =>
        new(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.Domain,
            tenant.IsActive,
            tenant.CreatedAt);
}

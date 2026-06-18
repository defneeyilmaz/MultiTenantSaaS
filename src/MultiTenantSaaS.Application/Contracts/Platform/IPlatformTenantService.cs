namespace MultiTenantSaaS.Application.Contracts.Platform;

public interface IPlatformTenantService
{
    Task<IReadOnlyList<PlatformTenantDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<PlatformTenantDto> CreateAsync(
        CreatePlatformTenantRequest request,
        CancellationToken cancellationToken = default);

    Task<PlatformTenantDto> UpdateAsync(
        Guid tenantId,
        UpdatePlatformTenantRequest request,
        CancellationToken cancellationToken = default);
}

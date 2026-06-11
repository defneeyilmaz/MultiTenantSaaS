namespace MultiTenantSaaS.Application.Contracts.Tenants;

public interface ITenantService
{
    Task<TenantDto> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task<TenantDto> UpdateSettingsAsync(
        UpdateTenantSettingsRequest request,
        CancellationToken cancellationToken = default);
}

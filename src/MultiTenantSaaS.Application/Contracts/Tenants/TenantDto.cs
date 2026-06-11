namespace MultiTenantSaaS.Application.Contracts.Tenants;

public sealed record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    string? Domain,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record UpdateTenantSettingsRequest(string Name, string? Domain);

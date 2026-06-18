namespace MultiTenantSaaS.Application.Contracts.Platform;

public sealed record PlatformTenantDto(
    Guid Id,
    string Name,
    string Slug,
    string? Domain,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record CreatePlatformTenantRequest(
    string Name,
    string? Slug,
    string? Domain);

public sealed record UpdatePlatformTenantRequest(
    string? Name,
    string? Domain,
    bool? IsActive);

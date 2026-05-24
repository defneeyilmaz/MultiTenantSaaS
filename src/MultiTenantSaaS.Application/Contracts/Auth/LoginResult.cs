namespace MultiTenantSaaS.Application.Contracts.Auth;

public sealed record LoginResult(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    Guid TenantId,
    string TenantSlug,
    Guid UserId,
    string Email,
    string Role);

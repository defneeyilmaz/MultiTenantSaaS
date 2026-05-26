namespace MultiTenantSaaS.Application.Contracts.Auth;

public sealed record AuthTokensResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    Guid TenantId,
    string TenantSlug,
    Guid UserId,
    string Email,
    string Role);

namespace MultiTenantSaaS.Application.Contracts.Auth;

public sealed record CompanySignupResult(
    Guid TenantId,
    string TenantSlug,
    Guid UserId,
    string AdminEmail,
    string Role);

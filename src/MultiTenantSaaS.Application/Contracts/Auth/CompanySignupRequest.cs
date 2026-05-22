namespace MultiTenantSaaS.Application.Contracts.Auth;

public sealed record CompanySignupRequest(
    string CompanyName,
    string AdminEmail,
    string AdminPassword,
    string? CompanySlug = null,
    string? AdminFullName = null);

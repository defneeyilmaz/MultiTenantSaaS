namespace MultiTenantSaaS.Application.Contracts.Auth;

public sealed record VerifyEmailRequest(string Email, string Token);

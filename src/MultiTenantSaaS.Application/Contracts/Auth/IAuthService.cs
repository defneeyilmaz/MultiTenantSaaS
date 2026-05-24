namespace MultiTenantSaaS.Application.Contracts.Auth;

public interface IAuthService
{
    Task<CompanySignupResult> CompanySignupAsync(
        CompanySignupRequest request,
        CancellationToken cancellationToken = default);

    Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}

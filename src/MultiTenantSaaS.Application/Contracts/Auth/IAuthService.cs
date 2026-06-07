namespace MultiTenantSaaS.Application.Contracts.Auth;

public interface IAuthService
{
    Task<CompanySignupResult> CompanySignupAsync(
        CompanySignupRequest request,
        CancellationToken cancellationToken = default);

    Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthTokensResult> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);
}

using Microsoft.AspNetCore.Mvc;
using MultiTenantSaaS.Application.Contracts.Auth;

namespace MultiTenantSaaS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("company-signup")]
    [ProducesResponseType(typeof(CompanySignupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanySignupResponse>> CompanySignup(
        [FromBody] CompanySignupApiRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _authService.CompanySignupAsync(
                new CompanySignupRequest(
                    request.CompanyName,
                    request.AdminEmail,
                    request.AdminPassword,
                    request.CompanySlug,
                    request.AdminFullName),
                cancellationToken);

            return CreatedAtAction(
                nameof(CompanySignup),
                new CompanySignupResponse(
                    result.TenantId,
                    result.TenantSlug,
                    result.UserId,
                    result.AdminEmail,
                    result.Role));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Company signup failed");
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokensResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokensResponse>> Login(
        [FromBody] LoginApiRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _authService.LoginAsync(
                new LoginRequest(request.Email, request.Password, request.TenantSlug),
                cancellationToken);

            return Ok(new AuthTokensResponse(
                result.AccessToken,
                result.AccessTokenExpiresAt,
                result.RefreshToken,
                result.RefreshTokenExpiresAt,
                result.TenantId,
                result.TenantSlug,
                result.UserId,
                result.Email,
                result.Role));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Login failed");
        }
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthTokensResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokensResponse>> RefreshToken(
        [FromBody] RefreshTokenApiRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _authService.RefreshTokenAsync(
                new RefreshTokenRequest(request.RefreshToken),
                cancellationToken);

            return Ok(new AuthTokensResponse(
                result.AccessToken,
                result.AccessTokenExpiresAt,
                result.RefreshToken,
                result.RefreshTokenExpiresAt,
                result.TenantId,
                result.TenantSlug,
                result.UserId,
                result.Email,
                result.Role));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Refresh token failed");
        }
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenApiRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await _authService.LogoutAsync(
            new RefreshTokenRequest(request.RefreshToken),
            cancellationToken);

        return NoContent();
    }
}

public sealed record CompanySignupApiRequest(
    string CompanyName,
    string AdminEmail,
    string AdminPassword,
    string? CompanySlug,
    string? AdminFullName);

public sealed record CompanySignupResponse(
    Guid TenantId,
    string TenantSlug,
    Guid UserId,
    string AdminEmail,
    string Role);

public sealed record LoginApiRequest(string Email, string Password, string TenantSlug);

public sealed record RefreshTokenApiRequest(string RefreshToken);

public sealed record AuthTokensResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    Guid TenantId,
    string TenantSlug,
    Guid UserId,
    string Email,
    string Role);

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
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
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

            return Ok(new LoginResponse(
                result.AccessToken,
                result.ExpiresAt,
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

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    Guid TenantId,
    string TenantSlug,
    Guid UserId,
    string Email,
    string Role);

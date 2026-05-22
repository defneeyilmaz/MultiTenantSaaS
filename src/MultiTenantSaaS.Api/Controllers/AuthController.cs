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

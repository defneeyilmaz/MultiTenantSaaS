using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantSaaS.Application.Contracts.Tenants;
using MultiTenantSaaS.Shared.Constants;

namespace MultiTenantSaaS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetCurrent(CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _tenantService.GetCurrentAsync(cancellationToken);
            return Ok(tenant);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Tenant not found");
        }
    }

    [HttpPut("current/settings")]
    [Authorize(Policy = PermissionPolicies.SettingsManage)]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TenantDto>> UpdateSettings(
        [FromBody] UpdateTenantSettingsRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var tenant = await _tenantService.UpdateSettingsAsync(request, cancellationToken);
            return Ok(tenant);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Tenant settings update failed");
        }
    }
}

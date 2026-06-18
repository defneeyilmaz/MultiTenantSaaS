using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantSaaS.Application.Contracts.Platform;
using MultiTenantSaaS.Shared.Constants;

namespace MultiTenantSaaS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/platform/tenants")]
public class PlatformTenantsController : ControllerBase
{
    private readonly IPlatformTenantService _platformTenantService;

    public PlatformTenantsController(IPlatformTenantService platformTenantService)
    {
        _platformTenantService = platformTenantService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionPolicies.TenantsView)]
    [ProducesResponseType(typeof(IReadOnlyList<PlatformTenantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlatformTenantDto>>> List(
        CancellationToken cancellationToken)
    {
        var tenants = await _platformTenantService.ListAsync(cancellationToken);
        return Ok(tenants);
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.TenantsManage)]
    [ProducesResponseType(typeof(PlatformTenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlatformTenantDto>> Create(
        [FromBody] CreatePlatformTenantRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var tenant = await _platformTenantService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(List), new { id = tenant.Id }, tenant);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Tenant creation failed");
        }
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.TenantsManage)]
    [ProducesResponseType(typeof(PlatformTenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlatformTenantDto>> Update(
        Guid id,
        [FromBody] UpdatePlatformTenantRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var tenant = await _platformTenantService.UpdateAsync(id, request, cancellationToken);
            return Ok(tenant);
        }
        catch (InvalidOperationException ex)
        {
            var statusCode = ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return Problem(
                detail: ex.Message,
                statusCode: statusCode,
                title: "Tenant update failed");
        }
    }
}

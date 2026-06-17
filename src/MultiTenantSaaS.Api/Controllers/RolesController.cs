using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantSaaS.Application.Contracts.Roles;
using MultiTenantSaaS.Shared.Constants;

namespace MultiTenantSaaS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("roles")]
    [Authorize(Policy = PermissionPolicies.RolesView)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> ListRoles(CancellationToken cancellationToken)
    {
        var roles = await _roleService.ListRolesAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpPost("roles")]
    [Authorize(Policy = PermissionPolicies.RolesManage)]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleDto>> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var role = await _roleService.CreateRoleAsync(request, cancellationToken);
            return CreatedAtAction(nameof(ListRoles), new { id = role.Id }, role);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Role creation failed");
        }
    }

    [HttpGet("permissions")]
    [Authorize(Policy = PermissionPolicies.PermissionsView)]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PermissionDto>>> ListPermissions(
        CancellationToken cancellationToken)
    {
        var permissions = await _roleService.ListPermissionsAsync(cancellationToken);
        return Ok(permissions);
    }

    [HttpPut("roles/{id:guid}/permissions")]
    [Authorize(Policy = PermissionPolicies.RolesManage)]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleDto>> UpdateRolePermissions(
        Guid id,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var role = await _roleService.UpdateRolePermissionsAsync(id, request, cancellationToken);
            return Ok(role);
        }
        catch (InvalidOperationException ex)
        {
            var statusCode = ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return Problem(
                detail: ex.Message,
                statusCode: statusCode,
                title: "Role permissions update failed");
        }
    }
}

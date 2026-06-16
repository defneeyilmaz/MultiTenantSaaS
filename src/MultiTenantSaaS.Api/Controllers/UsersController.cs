using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantSaaS.Application.Contracts.Users;
using MultiTenantSaaS.Shared.Constants;

namespace MultiTenantSaaS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionPolicies.UsersView)]
    [ProducesResponseType(typeof(IReadOnlyList<TenantUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TenantUserDto>>> List(CancellationToken cancellationToken)
    {
        var users = await _userService.ListAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost("invite")]
    [Authorize(Policy = PermissionPolicies.UsersInvite)]
    [ProducesResponseType(typeof(InvitationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvitationDto>> Invite(
        [FromBody] InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryGetCurrentUserId(out var invitedByUserId))
        {
            return UnauthorizedProblem();
        }

        try
        {
            var invitation = await _userService.InviteAsync(request, invitedByUserId, cancellationToken);
            return CreatedAtAction(nameof(Invite), invitation);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invitation failed");
        }
    }

    [HttpPost("accept-invitation")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AcceptInvitationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AcceptInvitationResult>> AcceptInvitation(
        [FromBody] AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _userService.AcceptInvitationAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invitation acceptance failed");
        }
    }

    [HttpPatch("{id:guid}/role")]
    [Authorize(Policy = PermissionPolicies.UsersManage)]
    [ProducesResponseType(typeof(TenantUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantUserDto>> AssignRole(
        Guid id,
        [FromBody] AssignUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryGetCurrentUserId(out var actingUserId))
        {
            return UnauthorizedProblem();
        }

        try
        {
            var user = await _userService.AssignRoleAsync(id, request, actingUserId, cancellationToken);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            var statusCode = ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return Problem(
                detail: ex.Message,
                statusCode: statusCode,
                title: "Role assignment failed");
        }
    }

    [HttpPatch("{id:guid}/disable")]
    [Authorize(Policy = PermissionPolicies.UsersManage)]
    [ProducesResponseType(typeof(TenantUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantUserDto>> Disable(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var actingUserId))
        {
            return UnauthorizedProblem();
        }

        try
        {
            var user = await _userService.DisableAsync(id, actingUserId, cancellationToken);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            var statusCode = ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return Problem(
                detail: ex.Message,
                statusCode: statusCode,
                title: "User disable failed");
        }
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdValue, out userId);
    }

    private ActionResult UnauthorizedProblem() =>
        Problem(
            detail: "The authenticated user could not be resolved.",
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Unauthorized");
}

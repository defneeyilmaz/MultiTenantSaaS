using Microsoft.AspNetCore.Authorization;
using MultiTenantSaaS.Shared.Constants;

namespace MultiTenantSaaS.Infrastructure.Authorization;

public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAuthorizationRequirement requirement)
    {
        if (context.User.HasClaim(AppConstants.PermissionClaim, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

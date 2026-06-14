using Microsoft.AspNetCore.Authorization;

namespace MultiTenantSaaS.Infrastructure.Authorization;

public sealed class PermissionAuthorizationRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

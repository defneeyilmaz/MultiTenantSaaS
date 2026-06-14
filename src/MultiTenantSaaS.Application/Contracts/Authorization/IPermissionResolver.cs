namespace MultiTenantSaaS.Application.Contracts.Authorization;

public interface IPermissionResolver
{
    Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(
        string roleName,
        CancellationToken cancellationToken = default);
}

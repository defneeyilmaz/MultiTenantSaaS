using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Application.Contracts.Authorization;
using MultiTenantSaaS.Infrastructure.Persistence;

namespace MultiTenantSaaS.Infrastructure.Services;

public class PermissionResolver : IPermissionResolver
{
    private readonly AppDbContext _dbContext;

    public PermissionResolver(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.Role.Name == roleName)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}

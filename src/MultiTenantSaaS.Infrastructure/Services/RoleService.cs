using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Application.Contracts.Roles;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Shared.Constants;

namespace MultiTenantSaaS.Infrastructure.Services;

public class RoleService : IRoleService
{
    private static readonly HashSet<string> ProtectedRoleNames =
    [
        RoleNames.PlatformAdmin
    ];

    private readonly AppDbContext _dbContext;

    public RoleService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RoleDto>> ListRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _dbContext.AppRoles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);

        return await MapRolesAsync(roles, cancellationToken);
    }

    public async Task<RoleDto> CreateRoleAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Role name is required.");
        }

        if (await _dbContext.AppRoles.AnyAsync(role => role.Name == name, cancellationToken))
        {
            throw new InvalidOperationException($"Role '{name}' already exists.");
        }

        var role = new AppRole
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = request.Description?.Trim()
        };

        _dbContext.AppRoles.Add(role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await MapRoleAsync(role, cancellationToken);
    }

    public async Task<IReadOnlyList<PermissionDto>> ListPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.Name)
            .Select(permission => new PermissionDto(
                permission.Id,
                permission.Name,
                permission.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleDto> UpdateRolePermissionsAsync(
        Guid roleId,
        UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await _dbContext.AppRoles
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            throw new InvalidOperationException("Role not found.");
        }

        if (ProtectedRoleNames.Contains(role.Name))
        {
            throw new InvalidOperationException("This role cannot be modified.");
        }

        var permissionNames = request.PermissionNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .Where(permission => permissionNames.Contains(permission.Name))
            .ToListAsync(cancellationToken);

        if (permissions.Count != permissionNames.Count)
        {
            throw new InvalidOperationException("One or more permissions are invalid.");
        }

        var existingMappings = await _dbContext.RolePermissions
            .Where(mapping => mapping.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _dbContext.RolePermissions.RemoveRange(existingMappings);
        _dbContext.RolePermissions.AddRange(permissions.Select(permission => new RolePermission
        {
            RoleId = roleId,
            PermissionId = permission.Id
        }));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await MapRoleAsync(role, cancellationToken);
    }

    private async Task<IReadOnlyList<RoleDto>> MapRolesAsync(
        IReadOnlyList<AppRole> roles,
        CancellationToken cancellationToken)
    {
        var roleIds = roles.Select(role => role.Id).ToList();

        var permissionLookup = await _dbContext.RolePermissions
            .AsNoTracking()
            .Where(mapping => roleIds.Contains(mapping.RoleId))
            .Join(
                _dbContext.Permissions.AsNoTracking(),
                mapping => mapping.PermissionId,
                permission => permission.Id,
                (mapping, permission) => new { mapping.RoleId, permission.Name })
            .ToListAsync(cancellationToken);

        return roles
            .Select(role => new RoleDto(
                role.Id,
                role.Name,
                role.Description,
                permissionLookup
                    .Where(item => item.RoleId == role.Id)
                    .Select(item => item.Name)
                    .OrderBy(name => name)
                    .ToList()))
            .ToList();
    }

    private async Task<RoleDto> MapRoleAsync(AppRole role, CancellationToken cancellationToken)
    {
        var permissions = await _dbContext.RolePermissions
            .AsNoTracking()
            .Where(mapping => mapping.RoleId == role.Id)
            .Join(
                _dbContext.Permissions.AsNoTracking(),
                mapping => mapping.PermissionId,
                permission => permission.Id,
                (_, permission) => permission.Name)
            .OrderBy(name => name)
            .ToListAsync(cancellationToken);

        return new RoleDto(role.Id, role.Name, role.Description, permissions);
    }
}

using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Shared.Constants;

namespace MultiTenantSaaS.Infrastructure.Persistence.Seed;

public static class AuthorizationSeedData
{
    public static readonly Guid PlatformAdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TenantAdminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ManagerRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid EmployeeRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.AppRoles.AnyAsync(cancellationToken))
        {
            return;
        }

        var permissions = CreatePermissions();
        var roles = CreateRoles();
        var rolePermissions = CreateRolePermissions(permissions);

        dbContext.Permissions.AddRange(permissions);
        dbContext.AppRoles.AddRange(roles);
        dbContext.RolePermissions.AddRange(rolePermissions);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<Permission> CreatePermissions() =>
    [
        CreatePermission("a1000001-0000-0000-0000-000000000001", PermissionNames.UsersInvite, "Invite users to the tenant"),
        CreatePermission("a1000001-0000-0000-0000-000000000002", PermissionNames.UsersManage, "Manage tenant users"),
        CreatePermission("a1000001-0000-0000-0000-000000000003", PermissionNames.UsersView, "View tenant users"),
        CreatePermission("a1000001-0000-0000-0000-000000000004", PermissionNames.ProjectsCreate, "Create projects"),
        CreatePermission("a1000001-0000-0000-0000-000000000005", PermissionNames.ProjectsView, "View projects"),
        CreatePermission("a1000001-0000-0000-0000-000000000006", PermissionNames.ProjectsManage, "Manage projects"),
        CreatePermission("a1000001-0000-0000-0000-000000000007", PermissionNames.TasksCreate, "Create tasks"),
        CreatePermission("a1000001-0000-0000-0000-000000000008", PermissionNames.TasksView, "View tasks"),
        CreatePermission("a1000001-0000-0000-0000-000000000009", PermissionNames.TasksManage, "Manage and assign tasks"),
        CreatePermission("a1000001-0000-0000-0000-00000000000a", PermissionNames.TasksUpdateStatus, "Update task status"),
        CreatePermission("a1000001-0000-0000-0000-00000000000b", PermissionNames.AuditView, "View audit logs"),
        CreatePermission("a1000001-0000-0000-0000-00000000000c", PermissionNames.SettingsManage, "Manage tenant settings"),
        CreatePermission("a1000001-0000-0000-0000-00000000000d", PermissionNames.TenantsManage, "Manage tenants"),
        CreatePermission("a1000001-0000-0000-0000-00000000000e", PermissionNames.TenantsView, "View tenants"),
        CreatePermission("a1000001-0000-0000-0000-00000000000f", PermissionNames.RolesManage, "Manage roles"),
        CreatePermission("a1000001-0000-0000-0000-000000000010", PermissionNames.RolesView, "View roles"),
        CreatePermission("a1000001-0000-0000-0000-000000000011", PermissionNames.PermissionsView, "View permissions")
    ];

    private static List<AppRole> CreateRoles() =>
    [
        CreateRole(PlatformAdminRoleId, RoleNames.PlatformAdmin, "Platform-wide administration"),
        CreateRole(TenantAdminRoleId, RoleNames.TenantAdmin, "Tenant administration"),
        CreateRole(ManagerRoleId, RoleNames.Manager, "Team and project management"),
        CreateRole(EmployeeRoleId, RoleNames.Employee, "Standard tenant user")
    ];

    private static List<RolePermission> CreateRolePermissions(IReadOnlyList<Permission> permissions)
    {
        var permissionIds = permissions.ToDictionary(p => p.Name, p => p.Id);

        return
        [
            ..MapRole(RoleNames.PlatformAdmin, permissionIds,
            [
                PermissionNames.TenantsManage,
                PermissionNames.TenantsView,
                PermissionNames.AuditView
            ]),
            ..MapRole(RoleNames.TenantAdmin, permissionIds,
            [
                PermissionNames.UsersInvite,
                PermissionNames.UsersManage,
                PermissionNames.UsersView,
                PermissionNames.AuditView,
                PermissionNames.SettingsManage,
                PermissionNames.RolesManage,
                PermissionNames.RolesView,
                PermissionNames.PermissionsView,
                PermissionNames.ProjectsView,
                PermissionNames.TasksView
            ]),
            ..MapRole(RoleNames.Manager, permissionIds,
            [
                PermissionNames.UsersView,
                PermissionNames.ProjectsCreate,
                PermissionNames.ProjectsView,
                PermissionNames.ProjectsManage,
                PermissionNames.TasksCreate,
                PermissionNames.TasksView,
                PermissionNames.TasksManage,
                PermissionNames.TasksUpdateStatus
            ]),
            ..MapRole(RoleNames.Employee, permissionIds,
            [
                PermissionNames.ProjectsView,
                PermissionNames.TasksView,
                PermissionNames.TasksUpdateStatus
            ])
        ];
    }

    private static IEnumerable<RolePermission> MapRole(
        string roleName,
        IReadOnlyDictionary<string, Guid> permissionIds,
        IEnumerable<string> permissionNames)
    {
        var roleId = roleName switch
        {
            RoleNames.PlatformAdmin => PlatformAdminRoleId,
            RoleNames.TenantAdmin => TenantAdminRoleId,
            RoleNames.Manager => ManagerRoleId,
            RoleNames.Employee => EmployeeRoleId,
            _ => throw new InvalidOperationException($"Unknown role '{roleName}'.")
        };

        return permissionNames.Select(permissionName => new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionIds[permissionName]
        });
    }

    private static Permission CreatePermission(string id, string name, string description) =>
        new()
        {
            Id = Guid.Parse(id),
            Name = name,
            Description = description
        };

    private static AppRole CreateRole(Guid id, string name, string description) =>
        new()
        {
            Id = id,
            Name = name,
            Description = description
        };
}

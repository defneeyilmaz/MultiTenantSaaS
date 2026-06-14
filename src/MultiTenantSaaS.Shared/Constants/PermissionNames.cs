namespace MultiTenantSaaS.Shared.Constants;

public static class PermissionNames
{
    public const string UsersInvite = "users.invite";
    public const string UsersManage = "users.manage";
    public const string UsersView = "users.view";

    public const string ProjectsCreate = "projects.create";
    public const string ProjectsView = "projects.view";
    public const string ProjectsManage = "projects.manage";

    public const string TasksCreate = "tasks.create";
    public const string TasksView = "tasks.view";
    public const string TasksManage = "tasks.manage";
    public const string TasksUpdateStatus = "tasks.update_status";

    public const string AuditView = "audit.view";
    public const string SettingsManage = "settings.manage";

    public const string TenantsManage = "tenants.manage";
    public const string TenantsView = "tenants.view";

    public const string RolesManage = "roles.manage";
    public const string RolesView = "roles.view";
    public const string PermissionsView = "permissions.view";

    public static IReadOnlyList<string> All { get; } =
    [
        UsersInvite,
        UsersManage,
        UsersView,
        ProjectsCreate,
        ProjectsView,
        ProjectsManage,
        TasksCreate,
        TasksView,
        TasksManage,
        TasksUpdateStatus,
        AuditView,
        SettingsManage,
        TenantsManage,
        TenantsView,
        RolesManage,
        RolesView,
        PermissionsView
    ];
}

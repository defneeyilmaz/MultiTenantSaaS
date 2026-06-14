namespace MultiTenantSaaS.Shared.Constants;

public static class PermissionPolicies
{
    public const string Prefix = "Permission:";

    public const string UsersInvite = Prefix + PermissionNames.UsersInvite;
    public const string UsersManage = Prefix + PermissionNames.UsersManage;
    public const string UsersView = Prefix + PermissionNames.UsersView;

    public const string ProjectsCreate = Prefix + PermissionNames.ProjectsCreate;
    public const string ProjectsView = Prefix + PermissionNames.ProjectsView;
    public const string ProjectsManage = Prefix + PermissionNames.ProjectsManage;

    public const string TasksCreate = Prefix + PermissionNames.TasksCreate;
    public const string TasksView = Prefix + PermissionNames.TasksView;
    public const string TasksManage = Prefix + PermissionNames.TasksManage;
    public const string TasksUpdateStatus = Prefix + PermissionNames.TasksUpdateStatus;

    public const string AuditView = Prefix + PermissionNames.AuditView;
    public const string SettingsManage = Prefix + PermissionNames.SettingsManage;

    public const string TenantsManage = Prefix + PermissionNames.TenantsManage;
    public const string TenantsView = Prefix + PermissionNames.TenantsView;

    public const string RolesManage = Prefix + PermissionNames.RolesManage;
    public const string RolesView = Prefix + PermissionNames.RolesView;
    public const string PermissionsView = Prefix + PermissionNames.PermissionsView;
}

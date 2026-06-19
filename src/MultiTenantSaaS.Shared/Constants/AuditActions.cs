namespace MultiTenantSaaS.Shared.Constants;

public static class AuditActions
{
    public const string AuthLogin = "auth.login";
    public const string AuthLogout = "auth.logout";

    public const string UserInvited = "user.invited";
    public const string UserInvitationAccepted = "user.invitation_accepted";
    public const string UserRoleAssigned = "user.role_assigned";
    public const string UserDisabled = "user.disabled";

    public const string TenantSettingsUpdated = "tenant.settings_updated";

    public const string PlatformTenantCreated = "platform.tenant_created";
    public const string PlatformTenantUpdated = "platform.tenant_updated";
}

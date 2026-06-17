namespace MultiTenantSaaS.Application.Contracts.Roles;

public sealed record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyList<string> Permissions);

public sealed record PermissionDto(
    Guid Id,
    string Name,
    string? Description);

public sealed record CreateRoleRequest(string Name, string? Description);

public sealed record UpdateRolePermissionsRequest(IReadOnlyList<string> PermissionNames);

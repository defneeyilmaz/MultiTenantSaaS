namespace MultiTenantSaaS.Application.Contracts.Users;

public sealed record InviteUserRequest(string Email, string Role);

public sealed record AcceptInvitationRequest(
    string Email,
    string Token,
    string Password,
    string? FullName);

public sealed record InvitationDto(
    Guid Id,
    string Email,
    string Role,
    DateTimeOffset ExpiresAt);

public sealed record AcceptInvitationResult(
    Guid UserId,
    string Email,
    Guid TenantId,
    string TenantSlug,
    string Role);

public sealed record TenantUserDto(
    Guid UserId,
    string Email,
    string? FullName,
    string Role,
    bool IsActive,
    DateTimeOffset JoinedAt);

public sealed record AssignUserRoleRequest(string Role);

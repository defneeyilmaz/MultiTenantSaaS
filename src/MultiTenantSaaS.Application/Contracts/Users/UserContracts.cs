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

namespace MultiTenantSaaS.Application.Contracts.Users;

public interface IUserService
{
    Task<InvitationDto> InviteAsync(
        InviteUserRequest request,
        Guid invitedByUserId,
        CancellationToken cancellationToken = default);

    Task<AcceptInvitationResult> AcceptInvitationAsync(
        AcceptInvitationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantUserDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<TenantUserDto> AssignRoleAsync(
        Guid userId,
        AssignUserRoleRequest request,
        Guid actingUserId,
        CancellationToken cancellationToken = default);

    Task<TenantUserDto> DisableAsync(
        Guid userId,
        Guid actingUserId,
        CancellationToken cancellationToken = default);
}
